using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class SecurityAI : Guard
{
    // Enum to define the different states of the guard.
    public enum GuardState { Patrolling, Following, Chasing, Dialogue }
    // Current state of the guard, initialized to Patrolling.
    public GuardState currentGuardState = GuardState.Patrolling;

    [Header("Player Detection Settings")]
    // Distance at which the guard starts following the player.
    public float followDistance = 3f;
    // Range within which the guard can detect the player.
    public float alertRange = 20f;
    // Duration for which the guard remembers the player's last known position.
    public float memoryDuration = 60f;
    // Flag to indicate if the player has been caught.
    private bool playerCaught = false;

    // Transform representing the hand target for animations or interactions.
    public Transform handTarget;

    // Timer to track the duration of the chase state.
    private float chaseTimer = 0f;

    // Flags to indicate if the guard is following or chasing the player.
    private bool _isFollowingPlayer;
    private bool _isChasingPlayer;

    // Reference to the dialogue script for interactions.
    public MichaelSecurityDialogueScript dialogueScript;
    // Flag to indicate if the dialogue has been completed.
    private bool dialogueCompleted = false;

    // Reference to the HandReach component for hand interactions.
    private HandReach handReach;

    [Header("References")]
    // Transform of the player object.
    private Transform _player;
    // List to store transforms of players remembered by the guard.
    public List<Transform> playersInMemory;

    [Header("Dialogue Trigger Settings")]
    // Radius within which the dialogue is triggered.
    public float dialogueTriggerRadius = 3f;

    [Header("UI Settings")]
    // Canvas for the "caught" UI.
    public Canvas caughtUICanvas;

    // Reference to the SecurityMovement script for movement control.
    public SecurityMovement securityMovement;
    // Reference to the FieldOfView script for player detection.
    private FieldOfView fieldOfView;
    // Reference to the PlayerMemoryTracker script for player memory.
    private PlayerMemoryTracker playerMemoryTracker;
    // Reference to the Animator component for animations.
    private Animator animator;

    [Header("Chase Conditions")]
    // Flag to enable or disable player chasing.
    public bool canChasePlayer = true;

    // Transform representing the target for chasing.
    private Transform chaseTarget;

    // Store the original NavMeshAgent state and rotation.
    private bool originalNavMeshAgentState;
    private Quaternion originalRotation;

    // Override the player property from the base class.
    public override Transform player
    {
        get => _player;
        set => _player = value;
    }

    // Override the isFollowingPlayer property from the base class.
    public override bool isFollowingPlayer
    {
        get => _isFollowingPlayer;
        set => _isFollowingPlayer = value;
    }

    // Override the isChasingPlayer property from the base class.
    public override bool isChasingPlayer
    {
        get => _isChasingPlayer;
        set => _isChasingPlayer = value;
    }

    // Override the navMeshAgent property from the base class.
    public override NavMeshAgent navMeshAgent { get; set; }

    // Override the runSpeed property from the base class.
    public override float runSpeed { get; set; }

    // Start method called when the script is initialized.
    void Start()
    {
        // Get necessary components.
        securityMovement = GetComponent<SecurityMovement>();
        handReach = GetComponent<HandReach>();
        fieldOfView = GetComponent<FieldOfView>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Set the player reference.
        SetPlayerReference();

        // Get the PlayerMemoryTracker component from the player object.
        if (player != null)
        {
            playerMemoryTracker = player.GetComponent<PlayerMemoryTracker>();
        }

        // Disable the caught UI canvas if assigned.
        if (caughtUICanvas != null)
        {
            caughtUICanvas.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Caught UI Canvas is not assigned in the inspector!");
        }
    }

    // Update method called every frame.
    void Update()
    {
        // Handle guard behavior based on the current state.
        switch (currentGuardState)
        {
            case GuardState.Patrolling:
                HandlePatrolling();
                break;
            case GuardState.Following:
                HandleFollowing();
                break;
            case GuardState.Chasing:
                HandleChasing();
                break;
            case GuardState.Dialogue:
                HandleDialogue();
                break;
        }

        // Update animations.
        UpdateAnimation();
        // Handle player detection.
        HandlePlayerDetection();

        // Debug input to chase the player.
        if (Input.GetKeyDown(KeyCode.V))
        {
            securityMovement.ChaseTarget(player);
        }
    }

    // Method to set the player reference.
    private void SetPlayerReference()
    {
        // Return if the player reference is already set.
        if (_player != null) return;

        // Find the player object by tag.
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        // Set the player reference if found, otherwise log an error.
        if (playerObject != null) _player = playerObject.transform;
        else Debug.LogError("Player object not found!");
    }

    // Handles player detection logic.
    private void HandlePlayerDetection()
    {
        // Check if the guard is patrolling, has a valid field of view, can see the player, and can chase.
        if (currentGuardState == GuardState.Patrolling && fieldOfView != null && fieldOfView.playerRef != null && fieldOfView.canSeePlayer && canChasePlayer)
        {
            // Start a coroutine to delay the chase after player detection.
            StartCoroutine(ChaseDelay(fieldOfView.playerRef));
        }
    }

    // Starts chasing the player.
    public override void StartChasingPlayer(Transform playerTransform)
    {
        // Check if the player transform is valid and has the correct tag.
        if (playerTransform == null || !playerTransform.CompareTag("Player")) return;

        // Set the guard state to chasing and update related flags.
        currentGuardState = GuardState.Chasing;
        isFollowingPlayer = false;
        isChasingPlayer = true;
        player = playerTransform;

        // Start chasing the player using the SecurityMovement component if available.
        if (securityMovement != null)
        {
            securityMovement.StartChasing(playerTransform);
        }

        // Start reaching for the hand target using the HandReach component if available.
        if (handReach != null)
        {
            handReach.StartReaching(handTarget);
        }
        else
        {
            Debug.LogWarning("HandReach component missing!");
        }

        // Add the player to the memory list and start the memory coroutine if not already in memory.
        if (!playersInMemory.Contains(player))
        {
            playersInMemory.Add(player);
            StartCoroutine(RememberPlayer(player));

            // Register the chase with the PlayerMemoryTracker if available.
            if (playerMemoryTracker != null)
            {
                playerMemoryTracker.RegisterChase();
            }
        }
    }

    // Disables the ability to chase the player.
    public void DisableChasing()
    {
        // Set the chase flag to false.
        canChasePlayer = false;
        // Stop chasing if the guard is currently chasing.
        if (isChasingPlayer)
        {
            StopChasingPlayer();
        }
    }

    // Stops chasing the player.
    public override void StopChasingPlayer()
    {
        // Set the guard state to patrolling and update related flags.
        currentGuardState = GuardState.Patrolling;
        isChasingPlayer = false;

        // Stop reaching using the HandReach component if available.
        if (handReach != null)
        {
            handReach.StopReaching();
        }
        // Stop chasing using the SecurityMovement component if available.
        if (securityMovement != null)
        {
            securityMovement.StopChasing();
        }
    }

    // Coroutine to delay the start of the chase.
    private IEnumerator ChaseDelay(Transform playerTransform)
    {
        yield return new WaitForSeconds(1f);
        StartChasingPlayer(playerTransform);
    }

    // Coroutine to remember the player for a set duration.
    private IEnumerator RememberPlayer(Transform detectedPlayer)
    {
        yield return new WaitForSeconds(memoryDuration);
        playersInMemory.Remove(detectedPlayer);
    }

    // Starts following the player.
    public override void FollowPlayer(Transform playerTransform)
    {
        // Check if the player transform is valid and has the correct tag.
        if (playerTransform == null || !playerTransform.CompareTag("Player")) return;

        // Start a detection cooldown coroutine.
        StartCoroutine(DetectionCooldown(2f));

        // Set the guard state to following and update related flags.
        currentGuardState = GuardState.Following;
        isChasingPlayer = false;
        isFollowingPlayer = true;
        player = playerTransform;

        // Start following the player using the SecurityMovement component if available.
        if (securityMovement != null)
        {
            securityMovement.StartFollowing(playerTransform);
        }
    }

    // Resumes patrolling.
    public override void ResumePatrolling()
    {
        // Set the guard state to patrolling and update related flags.
        currentGuardState = GuardState.Patrolling;
        isFollowingPlayer = false;
        isChasingPlayer = false;

        // Resume patrolling using the SecurityMovement component if available.
        if (securityMovement != null)
        {
            securityMovement.ResumePatrolling();
        }
    }

    // Coroutine to apply a detection cooldown.
    private IEnumerator DetectionCooldown(float duration)
    {
        // Disable the field of view, wait for the duration, and then re-enable it.
        if (fieldOfView != null)
        {
            fieldOfView.enabled = false;
            yield return new WaitForSeconds(duration);
            fieldOfView.enabled = true;
        }
    }

    // Moves the guard to the player and starts dialogue.
    public void MoveToPlayerAndStartDialogue()
    {
        // Start a coroutine to move the guard to the player.
        StartCoroutine(MoveGuardToPlayer());
    }

    // Coroutine to move the guard to the player and initiate dialogue.
    private IEnumerator MoveGuardToPlayer()
    {
        // Check if the player transform is valid.
        if (player != null)
        {
            // Calculate the target position in front of the player.
            Vector3 targetPosition = player.position + (-player.forward * 1.5f);
            // Set the NavMeshAgent's destination to the target position.
            navMeshAgent.SetDestination(targetPosition);

            // Wait until the guard reaches the target position.
            while (Vector3.Distance(transform.position, targetPosition) > 0.5f)
            {
                yield return null;
            }

            // Make the guard look at the player.
            transform.LookAt(player);
            // Adjust the guard's rotation to face the player directly.
            Vector3 newRotation = transform.rotation.eulerAngles;
            newRotation.y = 0;
            transform.rotation = Quaternion.Euler(newRotation);

            // Show the dialogue if the dialogue script is assigned.
            if (dialogueScript != null)
            {
                dialogueScript.ShowDialogue();
            }
            else
            {
                Debug.LogError("Dialogue script not assigned in SecurityGuards!");
            }
        }
    }

    // Stores the original state of the NavMeshAgent and rotation.
    public void StoreOriginalState()
    {
        // Store the original NavMeshAgent state if available.
        if (navMeshAgent != null)
        {
            originalNavMeshAgentState = navMeshAgent.isStopped;
        }
        // Store the original rotation.
        originalRotation = transform.rotation;
    }

    // Stops the guard and makes it look at the player.
    public void StopAndLookAtPlayer(Transform playerTransform)
    {
        // Stop the NavMeshAgent if available.
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
        }

        // Make the guard look at the player if the player transform is valid.
        if (playerTransform != null)
        {
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            directionToPlayer.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = lookRotation;
        }
    }

    // Restores the original state of the NavMeshAgent and rotation.
    public void RestoreOriginalState()
    {
        // Restore the original NavMeshAgent state if available.
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = originalNavMeshAgentState;
        }
        // Restore the original rotation.
        transform.rotation = originalRotation;
        // Set the guard state to patrolling.
        currentGuardState = GuardState.Patrolling;
    }

    // Handles the chasing behavior.
    private void HandleChasing()
    {
        // Stop chasing if the player is null.
        if (player == null)
        {
            StopChasingPlayer();
            return;
        }

        // Continue chasing if the player is in the field of view or memory.
        if (fieldOfView != null && (fieldOfView.canSeePlayer || playersInMemory.Contains(player)))
        {
            currentGuardState = GuardState.Chasing;
            isChasingPlayer = true;

            // Start chasing the player using the SecurityMovement component if available.
            if (securityMovement != null)
            {
                securityMovement.StartChasing(player);
            }

            // Start reaching using the HandReach component if available.
            if (handReach != null)
            {
                handReach.StartReaching(player);
            }

            // Reset the chase timer.
            chaseTimer = 5f;
        }
        else
        {
            // Decrement the chase timer.
            chaseTimer -= Time.deltaTime;
            // Stop chasing if the timer reaches zero.
            if (chaseTimer <= 0)
            {
                StopChasingPlayer();
            }
        }

        // Check if the player has been caught.
        CheckPlayerCaught();
    }

    // Sets the guard to chase a specific target.
    public void ChaseTarget(Transform target)
    {
        // Check if the target is null.
        if (target == null)
        {
            Debug.LogError("Chase target is null!");
            ResumePatrolling();
            return;
        }

        // Set the guard state to chasing and the chase target.
        currentGuardState = GuardState.Chasing;
        chaseTarget = target;
        // Set the NavMeshAgent's speed and destination.
        navMeshAgent.speed = runSpeed;
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(target.position);
    }

    // Method called when the dialogue is finished.
    public void DialogueFinished()
    {
        // Set the dialogue completed flag to true.
        dialogueCompleted = true;
    }

    // Checks if the player has been caught.
    private void CheckPlayerCaught()
    {
        // Return if the player is null or too far away.
        if (player == null || Vector3.Distance(transform.position, player.position) >= 1.5f) return;

        // Set the player caught flag to true.
        playerCaught = true;
        // Activate the caught UI.
        ActivateCaughtUI();
        // Kick the player out of the game.
        KickPlayerOutOfGame();
    }

    // Activates the caught UI.
    private void ActivateCaughtUI()
    {
        // Check if the caught UI canvas is assigned.
        if (caughtUICanvas == null)
        {
            Debug.LogError("Caught UI Canvas is not assigned in the inspector!");
            return;
        }

        // Enable the caught UI canvas and set cursor properties.
        caughtUICanvas.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Pause the game.
        Time.timeScale = 0f;
    }

    // Kicks the player out of the game.
    private void KickPlayerOutOfGame()
    {
        // Return if the player is null or has the wrong tag.
        if (player == null || !player.CompareTag("Player")) return;

        // Log a message and disable the player's movement.
        Debug.Log("Player caught and kicked out of the game.");
        player.GetComponent<PlayerMovement>().enabled = false;
    }

    // Updates the guard's animations.
    private void UpdateAnimation()
    {
        // Return if the animator is null.
        if (animator == null) return;

        // Set the animator's speed and chasing parameters.
        animator.SetFloat("Speed", navMeshAgent.velocity.magnitude);
        animator.SetBool("IsChasing", currentGuardState == GuardState.Chasing);
    }

    // Handles the following behavior.
    private void HandleFollowing()
    {
        // Return if the player is null.
        if (player == null) return;

        // Calculate the follow position and set the NavMeshAgent's destination.
        Vector3 followPosition = player.position - (player.position - transform.position).normalized * followDistance;
        navMeshAgent.SetDestination(followPosition);
        // Set the NavMeshAgent's speed to the walk speed if SecurityMovement is available.
        if (securityMovement != null)
        {
            navMeshAgent.speed = securityMovement.walkSpeed;
        }
    }

    // Handles the patrolling behavior.
    private void HandlePatrolling()
    {
        // Execute the patrol behavior using the SecurityMovement component if available.
        if (securityMovement != null)
        {
            securityMovement.PatrolBehavior();
        }
    }

    // Handles the dialogue behavior.
    private void HandleDialogue()
    {
        // Resume patrolling if the dialogue is completed.
        if (dialogueCompleted)
        {
            ResumePatrolling();
        }
    }

    // Stops the guard from chasing.
    public void DoNotChase()
    {
        // Set the chasing flag to false and resume patrolling.
        isChasingPlayer = false;
        ResumePatrolling();
        // Log a message.
        Debug.Log($"{gameObject.name} is no longer chasing the player.");
    }
}