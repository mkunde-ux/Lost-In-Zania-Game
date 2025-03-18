using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AdilaMovement : MonoBehaviour
{
    // Enumeration to define the NPC's current state.
    public enum NpcState { Patrolling, Interacting, Investigating, Alerted }

    [Header("Movement Settings")]
    // Speed at which the NPC patrols.
    [SerializeField] private float patrolSpeed = 2f;
    // Array of transforms representing patrol points.
    [SerializeField] private Transform[] patrolPoints;
    // Speed at which the NPC rotates.
    [SerializeField] private float rotationSpeed = 5f;
    // Time the NPC waits at each waypoint.
    [SerializeField] private float waypointWaitTime = 2f;

    [Header("Investigation System")]
    // Reference to the CharacterAI script for investigation behaviors.
    [SerializeField] private CharacterAI characterAI;

    [Header("Detection Settings")]
    // Radius within which the NPC detects items or player.
    [SerializeField] private float detectionRadius = 5f;
    // Layer mask for obstructions that block detection.
    [SerializeField] private LayerMask obstructionLayer;
    // Layer mask for items that can be detected.
    [SerializeField] private LayerMask itemLayer;

    [Header("References")]
    // Reference to the Animator component.
    [SerializeField] private Animator animator;
    // NavMeshAgent component for navigation.
    private NavMeshAgent agent;
    // Transform of the player.
    private Transform player;
    // Current state of the NPC.
    private NpcState currentState = NpcState.Patrolling;
    // Index of the current patrol point.
    private int currentPatrolIndex = 0;

    // Animator hash for the "IsWalking" parameter.
    private readonly int isWalkingHash = Animator.StringToHash("IsWalking");
    // Animator hash for the "IsTalking" parameter.
    private readonly int isTalkingHash = Animator.StringToHash("IsTalking");
    // Animator hash for the "React" trigger.
    private readonly int reactHash = Animator.StringToHash("React");

    // Flag indicating if the NPC is staggered.
    private bool isStaggered = false;
    // Duration of the stagger effect.
    [SerializeField] private float staggerDuration = 3f;

    // Flag indicating if the NPC is interacting.
    private bool isInteracting = false;
    // Flag indicating if a dialogue is active.
    private bool isDialogueActive = false;

    [Header("Audio Settings (Optional)")]
    // AudioSource component for playing sounds.
    [SerializeField] private AudioSource audioSource;
    // AudioClip for footstep sounds.
    [SerializeField] private AudioClip footstepClip;
    // Interval between footstep sounds.
    [SerializeField] private float footstepInterval = 0.5f;
    // Timer for footstep sounds.
    private float footstepTimer = 0f;

    private void Awake()
    {
        // Get the NavMeshAgent component.
        agent = GetComponent<NavMeshAgent>();
        // Find the player GameObject using its tag.
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        // Initialize CharacterAI if it exists.
        characterAI?.Initialize(agent, player);
    }

    private void Start()
    {
        // Set the agent speed to the patrol speed.
        agent.speed = patrolSpeed;
        // Initialize animations.
        InitializeAnimations();
    }

    private void InitializeAnimations()
    {
        // Reset animation parameters.
        animator.SetBool(isWalkingHash, false);
        animator.SetBool(isTalkingHash, false);
        animator.ResetTrigger(reactHash);
    }

    private void Update()
    {
        // Update movement animations based on agent state.
        UpdateMovementAnimation();

        // If CharacterAI is busy with an item, let it handle movement.
        if (characterAI != null && characterAI.IsBusyWithItem)
        {
            if (agent.isStopped)
            {
                agent.isStopped = false;
            }
            return;
        }

        // Only process movement if no dialogue is active.
        if (!isDialogueActive)
        {
            switch (currentState)
            {
                case NpcState.Patrolling:
                    // Perform patrol behavior.
                    Patrol();
                    // Check for nearby items.
                    CheckForItems();
                    break;
                case NpcState.Interacting:
                    // Handle interaction behavior.
                    HandleInteraction();
                    break;
                case NpcState.Investigating:
                    // Handle investigation behavior.
                    break;
                case NpcState.Alerted:
                    // Handle alerted behavior.
                    break;
            }
        }
    }

    private void UpdateMovementAnimation()
    {
        // Determine if the NPC should be walking.
        bool shouldWalk = agent.enabled && !agent.isStopped && agent.remainingDistance > agent.stoppingDistance;
        // Set the "IsWalking" animation parameter.
        animator.SetBool(isWalkingHash, shouldWalk);

        // Play footstep sounds if walking and audio settings are configured.
        if (shouldWalk && audioSource != null && footstepClip != null)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                audioSource.PlayOneShot(footstepClip);
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            // Reset footstep timer when not walking.
            footstepTimer = 0f;
        }
    }

    private void Patrol()
    {
        // If there are no patrol points, exit the function.
        if (patrolPoints.Length == 0) return;

        // Check if the agent has reached the current waypoint.
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // Start the coroutine for waiting at the waypoint.
            StartCoroutine(WaypointWait());
        }
    }

    private IEnumerator WaypointWait()
    {
        // Stop the agent's movement.
        agent.isStopped = true;
        // Set the "IsWalking" animation parameter to false.
        animator.SetBool(isWalkingHash, false);
        // Wait for the specified waypoint wait time.
        yield return new WaitForSeconds(waypointWaitTime);

        // Resume the agent's movement.
        agent.isStopped = false;
        // Increment the patrol index and wrap around if necessary.
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        // Set the agent's destination to the next patrol point.
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    public void InteractWithPlayer(bool dialogueActive)
    {
        // Set the dialogue active flag.
        isDialogueActive = dialogueActive;
        // If the NPC is alerted or already interacting, exit the function.
        if (currentState == NpcState.Alerted || isInteracting) return;

        // Set the interacting flag to true.
        isInteracting = true;
        // Set the NPC's state to interacting.
        currentState = NpcState.Interacting;
        // Stop the agent's movement and velocity.
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        // Disable agent rotation.
        agent.updateRotation = false;

        // Reset the "React" trigger and set the "IsTalking" animation parameter to true.
        animator.ResetTrigger(reactHash);
        animator.SetBool(isTalkingHash, true);

        // Start the coroutine for smoothly looking at the player.
        StartCoroutine(SmoothLookAt(player.position));
    }

    private void HandleInteraction()
    {
        // If the NPC is not interacting, exit the function.
        if (!isInteracting) return;

        // Check if the NPC needs to rotate to face the player.
        if (Vector3.Angle(transform.forward, player.position - transform.position) > 5f)
        {
            // Start the coroutine for smoothly looking at the player.
            StartCoroutine(SmoothLookAt(player.position));
        }
    }

    public void EndInteraction()
    {
        // Set the dialogue active flag to false.
        isDialogueActive = false;
        // If the NPC is not in the interacting state, exit the function.
        if (currentState != NpcState.Interacting) return;

        // Set the interacting flag to false.
        isInteracting = false;
        // Set the NPC's state to patrolling.
        currentState = NpcState.Patrolling;
        // Resume the agent's movement and enable rotation.
        agent.isStopped = false;
        agent.updateRotation = true;
        // Set the "IsTalking" animation parameter to false.
        animator.SetBool(isTalkingHash, false);

        // If there are patrol points, set the agent's destination.
        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    public void Stagger()
    {
        // If the NPC is not already staggered, start the stagger effect.
        if (!isStaggered)
        {
            // Set the staggered flag to true.
            isStaggered = true;
            // Stop the agent's movement and velocity.
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            // Set the "IsWalking" animation parameter to false.
            animator.SetBool(isWalkingHash, false);

            // Start the coroutine for recovering from stagger.
            StartCoroutine(RecoverFromStagger());
        }
    }

    private IEnumerator RecoverFromStagger()
    {
        // Wait for the stagger duration.
        yield return new WaitForSeconds(staggerDuration);
        // Resume the agent's movement.
        agent.isStopped = false;
        // Set the staggered flag to false.
        isStaggered = false;
        // If the NPC is patrolling and there are patrol points, set the agent's destination.
        if (currentState == NpcState.Patrolling && patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    private IEnumerator SmoothLookAt(Vector3 targetPosition)
    {
        // Calculate the direction to the target position.
        Vector3 direction = (targetPosition - transform.position).normalized;
        // Set the y-component of the direction to 0.
        direction.y = 0;
        // Calculate the target rotation.
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Smoothly rotate the NPC towards the target rotation.
        while (Quaternion.Angle(transform.rotation, targetRotation) > 2f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void CheckForItems()
    {
        // Find all colliders within the detection radius that are on the item layer.
        Collider[] items = Physics.OverlapSphere(transform.position, detectionRadius, itemLayer);
        // Iterate through the found colliders.
        foreach (Collider item in items)
        {
            // Check if the item has the "PickableItem" tag.
            if (item.CompareTag("PickableItem"))
            {
                // Start the coroutine in CharacterAI to pick up the item after a delay.
                characterAI.StartCoroutine(characterAI.PickUpItemAfterDelay(item.gameObject));
                // Break the loop after finding and processing the first pickable item.
                break;
            }
        }
    }

    public void Alert()
    {
        // If the NPC is already alerted, exit the function.
        if (currentState == NpcState.Alerted) return;

        // Set the NPC's state to alerted.
        currentState = NpcState.Alerted;
        // Stop the agent's movement.
        agent.isStopped = true;
        // Trigger the "React" animation.
        animator.SetTrigger(reactHash);
    }

    public void ReturnToPatrol()
    {
        // If there are patrol points, find the nearest one and resume patrol.
        if (patrolPoints.Length > 0)
        {
            // Initialize variables to track the nearest patrol point.
            int nearestIndex = 0;
            float nearestDistance = Mathf.Infinity;
            // Iterate through all patrol points.
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                // Calculate the distance to the current patrol point.
                float distance = Vector3.Distance(transform.position, patrolPoints[i].position);
                // Check if the current patrol point is closer than the nearest one found so far.
                if (distance < nearestDistance)
                {
                    // Update the nearest distance and index.
                    nearestDistance = distance;
                    nearestIndex = i;
                }
            }
            // Set the current patrol index to the nearest one.
            currentPatrolIndex = nearestIndex;
            // Resume the agent's movement.
            agent.isStopped = false;
            // Set the agent's destination to the nearest patrol point.
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            // Log a message indicating the resumption of patrol.
            Debug.Log("Resuming patrol at waypoint " + currentPatrolIndex);
        }
    }

    private bool IsPositionReachable(Vector3 target)
    {
        // Create a new NavMeshPath.
        NavMeshPath path = new NavMeshPath();
        // Calculate the path to the target position.
        // Return true if the path is successfully calculated and complete, otherwise return false.
        return agent.CalculatePath(target, path) && path.status == NavMeshPathStatus.PathComplete;
    }

    private void OnDrawGizmosSelected()
    {
        // Set the Gizmos color to yellow.
        Gizmos.color = Color.yellow;
        // Draw a wire sphere to visualize the detection radius.
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}