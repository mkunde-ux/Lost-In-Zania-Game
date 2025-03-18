using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NesMovement : MonoBehaviour
{
    public enum NpcState { Patrolling, Interacting, Investigating, Alerted }

    [Header("Movement Settings")]
    // Speed at which the NPC patrols.
    [SerializeField] private float patrolSpeed = 2f;
    // Array of transforms representing patrol points.
    [SerializeField] private Transform[] patrolPoints;
    // Speed at which the NPC rotates.
    [SerializeField] private float rotationSpeed = 5f;
    // Time the NPC waits at each patrol point.
    [SerializeField] private float waypointWaitTime = 2f;

    [Header("Investigation System")]
    // Reference to the CharacterAI component for investigation logic.
    [SerializeField] private CharacterAI characterAI;

    [Header("Detection Settings")]
    // Radius within which the NPC can detect the player or items.
    [SerializeField] private float detectionRadius = 5f;
    // LayerMask for objects that obstruct the NPC's view.
    [SerializeField] private LayerMask obstructionLayer;
    // LayerMask for items that the NPC can investigate.
    [SerializeField] private LayerMask itemLayer;

    [Header("References")]
    // Reference to the Animator component for controlling animations.
    [SerializeField] private Animator animator;
    // Reference to the NavMeshAgent component for navigation.
    private NavMeshAgent agent;
    // Reference to the player's transform.
    private Transform player;
    // Current state of the NPC.
    private NpcState currentState = NpcState.Patrolling;
    // Index of the current patrol point.
    private int currentPatrolIndex = 0;

    // Animation Parameters
    // Hash for the "isWalking" animation parameter.
    private readonly int isWalkingHash = Animator.StringToHash("NEisWalking");
    // Hash for the "isTalking" animation parameter.
    private readonly int isTalkingHash = Animator.StringToHash("NEisTalking");
    // Hash for the "react" animation trigger.
    private readonly int reactHash = Animator.StringToHash("NEisReact");

    // Stagger
    // Flag indicating if the NPC is staggered.
    private bool isStaggered = false;
    // Duration of the stagger effect.
    [SerializeField] private float staggerDuration = 3f;

    // Flag to track if the NPC is interacting with the player.
    private bool isInteracting = false;
    // Flag to track if dialogue is active.
    private bool isDialogueActive = false;

    private void Awake()
    {
        // Get the NavMeshAgent component.
        agent = GetComponent<NavMeshAgent>();
        // Find the player's transform using its tag.
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Subscribe to the OnItemLanded event of DroppedItem.
        DroppedItem.OnItemLanded += HandleItemLanded;
        // Initialize the CharacterAI component.
        characterAI?.Initialize(agent, player);
    }

    private void Start()
    {
        // Set the NavMeshAgent's speed to the patrol speed.
        agent.speed = patrolSpeed;
        // Initialize animations.
        InitializeAnimations();
    }

    private void InitializeAnimations()
    {
        // Set the "isWalking" animation parameter to false.
        animator.SetBool(isWalkingHash, false);
        // Set the "isTalking" animation parameter to false.
        animator.SetBool(isTalkingHash, false);
        // Reset the "react" animation trigger.
        animator.ResetTrigger(reactHash);
    }

    private void Update()
    {
        // Update the movement animation based on the NavMeshAgent's state.
        UpdateMovementAnimation();

        // Only update state if not interacting or in dialogue
        if (!isInteracting && !isDialogueActive)
        {
            switch (currentState)
            {
                case NpcState.Patrolling:
                    // Perform patrol behavior.
                    Patrol();
                    // Check for items within detection radius.
                    CheckForItems();
                    break;
                case NpcState.Interacting:
                    // Handle interaction with the player.
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
        // Check if the NavMeshAgent is enabled, not stopped, and has a remaining distance greater than its stopping distance.
        bool shouldWalk = agent.enabled && !agent.isStopped && agent.remainingDistance > agent.stoppingDistance;
        // Set the "isWalking" animation parameter based on the shouldWalk flag.
        animator.SetBool(isWalkingHash, shouldWalk);
    }

    private void Patrol()
    {
        // If there are no patrol points, exit the method.
        if (patrolPoints.Length == 0) return;

        // Check if the NavMeshAgent's path is not pending and the remaining distance is less than or equal to the stopping distance.
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // Start the WaypointWait coroutine.
            StartCoroutine(WaypointWait());
        }
    }

    private IEnumerator WaypointWait()
    {
        // Stop the NavMeshAgent's movement.
        agent.isStopped = true;
        // Set the "isWalking" animation parameter to false to stop walking animation.
        animator.SetBool(isWalkingHash, false);
        // Wait for the specified waypoint wait time.
        yield return new WaitForSeconds(waypointWaitTime);

        // Resume the NavMeshAgent's movement.
        agent.isStopped = false;
        // Calculate the index of the next patrol point, wrapping around to the start if necessary.
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        // Set the NavMeshAgent's destination to the next patrol point's position.
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    public void InteractWithPlayer(bool dialogueActive)
    {
        isDialogueActive = dialogueActive; // Set the dialogue flag
        isInteracting = true; // Set the interaction flag
        currentState = NpcState.Interacting;

        // Stop the agent completely
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.updateRotation = false;

        // Set animation states
        animator.ResetTrigger(reactHash);
        animator.SetBool(isTalkingHash, true);

        // Face the player
        StartCoroutine(SmoothLookAt(player.position));
    }

    private void HandleInteraction()
    {
        if (!isInteracting) return; // Exit if not interacting

        // Smoothly look at the player during interaction
        if (Vector3.Angle(transform.forward, player.position - transform.position) > 5f)
        {
            StartCoroutine(SmoothLookAt(player.position));
        }
    }

    public void EndInteraction()
    {
        isDialogueActive = false; // Reset dialogue flag
        isInteracting = false; // Reset interaction flag

        // Resume patrolling if not alerted
        if (currentState != NpcState.Alerted)
        {
            currentState = NpcState.Patrolling;
            agent.isStopped = false;
            agent.updateRotation = true;
            animator.SetBool(isTalkingHash, false);

            if (patrolPoints.Length > 0)
            {
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
        }
    }

    public void Stagger()
    {
        if (!isStaggered) // Prevent repeated staggers while already staggered
        {
            isStaggered = true;
            agent.isStopped = true;  // Stop movement
            agent.velocity = Vector3.zero; // Ensure it comes to a complete stop
            animator.SetBool(isWalkingHash, false); // Stop walking animation

            StartCoroutine(RecoverFromStagger());
        }
    }

    private IEnumerator RecoverFromStagger()
    {
        // Wait for the specified stagger duration.
        yield return new WaitForSeconds(staggerDuration);
        // Resume the NavMeshAgent's movement.
        agent.isStopped = false;
        // Reset the isStaggered flag.
        isStaggered = false;
        // If the NPC is in patrolling state and there are patrol points, set the destination to the current patrol point.
        if (currentState == NpcState.Patrolling && patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    private IEnumerator SmoothLookAt(Vector3 targetPosition)
    {
        // Calculate the normalized direction from the NPC to the target position.
        Vector3 direction = (targetPosition - transform.position).normalized;
        // Set the y-component of the direction to zero to prevent vertical rotation.
        direction.y = 0;
        // Create a rotation that looks in the direction of the calculated direction.
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Smoothly rotate the NPC towards the target rotation until the angle between the current rotation and the target rotation is less than 2 degrees.
        while (Quaternion.Angle(transform.rotation, targetRotation) > 2f)
        {
            // Smoothly interpolate the NPC's rotation towards the target rotation.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            // Wait for the next frame.
            yield return null;
        }
    }

    private void CheckForItems()
    {
        // Find all colliders within the detection radius that are on the item layer.
        Collider[] items = Physics.OverlapSphere(transform.position, detectionRadius, itemLayer);
        // Iterate through the found items.
        foreach (Collider item in items)
        {
            // Get the DroppedItem component from the item.
            DroppedItem droppedItem = item.GetComponent<DroppedItem>();
            // If the item is a dropped item, is dropped, and has not been investigated, trigger an investigation.
            if (droppedItem != null && droppedItem.isDropped && !droppedItem.isInvestigated)
            {
                // Trigger an investigation at the item's position.
                TriggerInvestigation(item.transform.position);
                // Mark the item as investigated.
                droppedItem.MarkAsInvestigated();
                // Break out of the loop since only one item should be investigated at a time.
                break;
            }
        }
    }

    private void HandleItemLanded(Vector3 itemPosition)
    {
        // If the NPC is not in patrolling state, exit the method.
        if (currentState != NpcState.Patrolling) return;
        // If the item is outside the detection radius, exit the method.
        if (Vector3.Distance(transform.position, itemPosition) > detectionRadius) return;

        // Trigger an investigation at the item's position.
        TriggerInvestigation(itemPosition);
    }

    private void TriggerInvestigation(Vector3 itemPosition)
    {
        // If the NPC is not in patrolling state, exit the method.
        if (currentState != NpcState.Patrolling) return;
        // If the item position is not reachable, exit the method.
        if (!IsPositionReachable(itemPosition)) return;

        // Start the investigation routine.
        StartCoroutine(InvestigationRoutine(itemPosition));
    }

    private IEnumerator InvestigationRoutine(Vector3 itemPosition)
    {
        // Set the NPC's state to investigating.
        currentState = NpcState.Investigating;
        // Start the investigation using the CharacterAI component.
        characterAI.StartInvestigation(itemPosition);

        // Continue the investigation routine until the CharacterAI component is no longer searching.
        while (characterAI.IsSearching)
        {
            // Set the "isWalking" animation parameter based on the NavMeshAgent's velocity.
            animator.SetBool(isWalkingHash, agent.velocity.magnitude > 0.1f);
            // Wait for the next frame.
            yield return null;
        }

        // If the CharacterAI component is alerted, alert the NPC.
        if (characterAI.IsAlerted)
        {
            Alert();
        }
        // Otherwise, return the NPC to patrol.
        else
        {
            ReturnToPatrol();
        }
    }

    public void Alert()
    {
        // If the NPC is already alerted, exit the method.
        if (currentState == NpcState.Alerted) return;

        // Set the NPC's state to alerted.
        currentState = NpcState.Alerted;
        // Stop the NavMeshAgent's movement.
        agent.isStopped = true;
        // Trigger the "react" animation.
        animator.SetTrigger(reactHash);
    }

    private void ReturnToPatrol()
    {
        // Reset the "react" animation trigger.
        animator.ResetTrigger(reactHash);
        // Set the NPC's state to patrolling.
        currentState = NpcState.Patrolling;
        // Resume the NavMeshAgent's movement.
        agent.isStopped = false;

        // If there are patrol points, set the destination to the current patrol point.
        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    private bool IsPositionReachable(Vector3 target)
    {
        // Create a new NavMeshPath.
        NavMeshPath path = new NavMeshPath();
        // Calculate the path from the NPC's current position to the target position.
        // Return true if the path is calculated successfully and the path status is PathComplete.
        return agent.CalculatePath(target, path) && path.status == NavMeshPathStatus.PathComplete;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the OnItemLanded event when the NPC is destroyed.
        DroppedItem.OnItemLanded -= HandleItemLanded;
    }
}