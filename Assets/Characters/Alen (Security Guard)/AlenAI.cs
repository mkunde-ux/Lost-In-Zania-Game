using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class AlenAI : Guard
{
    //Check the SecurityAI script for Comments
    public enum GuardState { Patrolling, Following, Chasing, Dialogue }
    public GuardState currentGuardState = GuardState.Patrolling;

    [Header("Player Detection Settings")]
    public float followDistance = 3f;
    public float alertRange = 20f;
    public float memoryDuration = 60f;
    private bool playerCaught = false;

    public Transform handTarget;

    private float chaseTimer = 0f;

    private bool _isFollowingPlayer;
    private bool _isChasingPlayer;

    public AlenConDialogue dialogueScript;
    private bool dialogueCompleted = false;

    private HandReach handReach;

    [Header("References")]
    private Transform _player;
    public List<Transform> playersInMemory;

    [Header("Dialogue Trigger Settings")]
    public float dialogueTriggerRadius = 3f;

    [Header("UI Settings")]
    public Canvas caughtUICanvas;

    public SecurityMovement securityMovement;
    private FieldOfView fieldOfView;
    private PlayerMemoryTracker playerMemoryTracker;
    private Animator animator;

    [Header("Chase Conditions")]
    public bool canChasePlayer = true;

    private Transform chaseTarget;

    private bool originalNavMeshAgentState;
    private Quaternion originalRotation;

    public override Transform player
    {
        get => _player;
        set => _player = value;
    }

    public override bool isFollowingPlayer
    {
        get => _isFollowingPlayer;
        set => _isFollowingPlayer = value;
    }

    public override bool isChasingPlayer
    {
        get => _isChasingPlayer;
        set => _isChasingPlayer = value;
    }

    public override NavMeshAgent navMeshAgent { get; set; }

    public override float runSpeed { get; set; }

    void Start()
    {
        securityMovement = GetComponent<SecurityMovement>();
        handReach = GetComponent<HandReach>();
        fieldOfView = GetComponent<FieldOfView>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        SetPlayerReference();

        if (player != null)
        {
            playerMemoryTracker = player.GetComponent<PlayerMemoryTracker>();
        }

        if (caughtUICanvas != null)
        {
            caughtUICanvas.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Caught UI Canvas is not assigned in the inspector!");
        }
    }

    void Update()
    {
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

        UpdateAnimation();
        HandlePlayerDetection();

        if (Input.GetKeyDown(KeyCode.V)) 
        {
            securityMovement.ChaseTarget(player);
        }
    }

    private void SetPlayerReference()
    {
        if (_player != null) return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) _player = playerObject.transform;
        else Debug.LogError("Player object not found!");
    }

    private void HandlePlayerDetection()
    {
        if (currentGuardState == GuardState.Patrolling && fieldOfView != null && fieldOfView.playerRef != null && fieldOfView.canSeePlayer && canChasePlayer)
        {
            StartCoroutine(ChaseDelay(fieldOfView.playerRef));
        }
    }

    public override void StartChasingPlayer(Transform playerTransform)
    {
        if (playerTransform == null || !playerTransform.CompareTag("Player")) return;

        currentGuardState = GuardState.Chasing;
        isFollowingPlayer = false;
        isChasingPlayer = true;
        player = playerTransform;

        if (securityMovement != null)
        {
            securityMovement.StartChasing(playerTransform);
        }

        if (handReach != null)
        {
            handReach.StartReaching(handTarget);
        }
        else
        {
            Debug.LogWarning("HandReach component missing!");
        }

        if (!playersInMemory.Contains(player))
        {
            playersInMemory.Add(player);
            StartCoroutine(RememberPlayer(player));

            if (playerMemoryTracker != null)
            {
                playerMemoryTracker.RegisterChase();
            }
        }
    }

    public void DisableChasing()
    {
        canChasePlayer = false;
        if (isChasingPlayer)
        {
            StopChasingPlayer();
        }
    }

    public override void StopChasingPlayer()
    {
        currentGuardState = GuardState.Patrolling;
        isChasingPlayer = false;

        if (handReach != null)
        {
            handReach.StopReaching();
        }
        if (securityMovement != null)
        {
            securityMovement.StopChasing(); 
        }
    }

    private IEnumerator ChaseDelay(Transform playerTransform)
    {
        yield return new WaitForSeconds(1f);
        StartChasingPlayer(playerTransform);
    }

    private IEnumerator RememberPlayer(Transform detectedPlayer)
    {
        yield return new WaitForSeconds(memoryDuration);
        playersInMemory.Remove(detectedPlayer);
    }

    public override void FollowPlayer(Transform playerTransform)
    {
        if (playerTransform == null || !playerTransform.CompareTag("Player")) return;

        StartCoroutine(DetectionCooldown(2f));

        currentGuardState = GuardState.Following;
        isChasingPlayer = false;
        isFollowingPlayer = true;
        player = playerTransform;

        if (securityMovement != null)
        {
            securityMovement.StartFollowing(playerTransform); 
        }
    }

    public override void ResumePatrolling()
    {
        currentGuardState = GuardState.Patrolling;
        isFollowingPlayer = false;
        isChasingPlayer = false;

        if (securityMovement != null)
        {
            securityMovement.ResumePatrolling();
        }
    }

    private IEnumerator DetectionCooldown(float duration)
    {
        if (fieldOfView != null)
        {
            fieldOfView.enabled = false;
            yield return new WaitForSeconds(duration);
            fieldOfView.enabled = true;
        }
    }

    public void MoveToPlayerAndStartDialogue()
    {
        StartCoroutine(MoveGuardToPlayer());
    }

    private IEnumerator MoveGuardToPlayer()
    {
        if (player != null)
        {
            Vector3 targetPosition = player.position + (-player.forward * 1.5f);
            navMeshAgent.SetDestination(targetPosition);

            while (Vector3.Distance(transform.position, targetPosition) > 0.5f)
            {
                yield return null;
            }

            transform.LookAt(player);
            Vector3 newRotation = transform.rotation.eulerAngles;
            newRotation.y = 0;
            transform.rotation = Quaternion.Euler(newRotation);

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

    public void StoreOriginalState()
    {
        if (navMeshAgent != null)
        {
            originalNavMeshAgentState = navMeshAgent.isStopped;
        }
        originalRotation = transform.rotation;
    }

    public void StopAndLookAtPlayer(Transform playerTransform)
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
        }

        if (playerTransform != null)
        {
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            directionToPlayer.y = 0; 

            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);

            transform.rotation = lookRotation;
        }
    }

    public void RestoreOriginalState()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = originalNavMeshAgentState;
        }
        transform.rotation = originalRotation;
        currentGuardState = GuardState.Patrolling; 
    }

    private void HandleChasing()
    {
        if (player == null)
        {
            StopChasingPlayer();
            return;
        }

        if (fieldOfView != null && (fieldOfView.canSeePlayer || playersInMemory.Contains(player)))
        {
            currentGuardState = GuardState.Chasing;
            isChasingPlayer = true;

            if (securityMovement != null)
            {
                securityMovement.StartChasing(player);
            }

            if (handReach != null)
            {
                handReach.StartReaching(player);
            }

            StopCoroutine(ResetChaseTimer());
            StartCoroutine(ResetChaseTimer());
        }
        else
        {
            if (chaseTimer <= 0)
            {
                StopChasingPlayer();
            }
        }

        CheckPlayerCaught();
    }

    public void ChaseTarget(Transform target)
    {
        if (target == null)
        {
            Debug.LogError("Chase target is null!");
            ResumePatrolling();
            return;
        }

        currentGuardState = GuardState.Chasing; 
        chaseTarget = target;
        navMeshAgent.speed = runSpeed;
        navMeshAgent.isStopped = false;

        navMeshAgent.SetDestination(target.position);
    }


    private IEnumerator ResetChaseTimer()
    {
        chaseTimer = 5f;
        while (chaseTimer > 0)
        {
            chaseTimer -= Time.deltaTime;
            yield return null;
        }
    }

    public void DialogueFinished()
    {
        dialogueCompleted = true;
    }

    private void CheckPlayerCaught()
    {
        if (player == null || Vector3.Distance(transform.position, player.position) >= 1.5f) return;

        playerCaught = true;
        ActivateCaughtUI();
        KickPlayerOutOfGame();
    }

    private void ActivateCaughtUI()
    {
        if (caughtUICanvas == null)
        {
            Debug.LogError("Caught UI Canvas is not assigned in the inspector!");
            return;
        }

        caughtUICanvas.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    private void KickPlayerOutOfGame()
    {
        if (player == null || !player.CompareTag("Player")) return;

        Debug.Log("Player caught and kicked out of the game.");
        player.GetComponent<PlayerMovement>().enabled = false;
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        animator.SetFloat("Speed", navMeshAgent.velocity.magnitude);
        animator.SetBool("IsChasing", currentGuardState == GuardState.Chasing);
        animator.SetBool("IsFollowing", currentGuardState == GuardState.Following);
        animator.SetBool("IsPatrolling", currentGuardState == GuardState.Patrolling);
    }

    private void HandleFollowing()
    {
        if (player == null) return;

        Vector3 followPosition = player.position - (player.position - transform.position).normalized * followDistance;
        navMeshAgent.SetDestination(followPosition);
        if (securityMovement != null)
        {
            navMeshAgent.speed = securityMovement.walkSpeed;
        }
    }

    private void HandlePatrolling()
    {
        if (securityMovement != null)
        {
            securityMovement.PatrolBehavior();
        }
    }

    private void HandleDialogue()
    {
        if (dialogueCompleted)
        {
            ResumePatrolling();
        }
    }

    public void DoNotChase()
    {
        isChasingPlayer = false;
        ResumePatrolling();
        Debug.Log($"{gameObject.name} is no longer chasing the player.");
    }
}