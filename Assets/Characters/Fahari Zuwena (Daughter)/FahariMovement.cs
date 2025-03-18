using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class FahariMovement : MonoBehaviour
{
    public enum NpcState { Patrolling, Interacting, Investigating, Alerted }

    [Header("Movement Settings")]
    // Speed at which Fahari patrols.
    [SerializeField] private float patrolSpeed = 2f;
    // Array of transforms representing patrol points.
    [SerializeField] private Transform[] patrolPoints;
    // Speed at which Fahari rotates.
    [SerializeField] private float rotationSpeed = 5f;
    // Time Fahari waits at each waypoint.
    [SerializeField] private float waypointWaitTime = 2f;

    [Header("Investigation System")]
    // Reference to the CharacterAI component for investigation logic.
    [SerializeField] private CharacterAI characterAI;

    [Header("Detection Settings")]
    // Radius within which Fahari can detect items or the player.
    [SerializeField] private float detectionRadius = 5f;
    // LayerMask for objects that obstruct Fahari's view.
    [SerializeField] private LayerMask obstructionLayer;
    // LayerMask for items that Fahari can investigate.
    [SerializeField] private LayerMask itemLayer;

    [Header("References")]
    // Reference to the Animator component for controlling animations.
    [SerializeField] private Animator animator;
    // Reference to the NavMeshAgent component for navigation.
    private NavMeshAgent agent;
    // Reference to the player's transform.
    private Transform player;
    // Current state of Fahari.
    private NpcState currentState = NpcState.Patrolling;
    // Index of the current patrol point.
    private int currentPatrolIndex = 0;

    // Animation Parameters
    // Hash for the "isWalking" animation parameter.
    private readonly int isWalkingHash = Animator.StringToHash("FisWalking");
    // Hash for the "isTalking" animation parameter.
    private readonly int isTalkingHash = Animator.StringToHash("FisTalking");
    // Hash for the "react" animation trigger.
    private readonly int reactHash = Animator.StringToHash("FReact");

    // Stagger
    // Flag indicating if Fahari is staggered.
    private bool isStaggered = false;
    // Duration of the stagger effect.
    [SerializeField] private float staggerDuration = 3f;

    // Flag indicating if Fahari is interacting with the player.
    private bool isInteracting = false;
    // Flag indicating if dialogue is active.
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

        // Only update state if dialogue is not active.
        if (!isDialogueActive)
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
        // If there are no patrol points or dialogue is active, exit the method.
        if (patrolPoints.Length == 0 || isDialogueActive)
            return;

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

        // If dialogue has started during wait, do not resume patrol.
        if (isDialogueActive)
            yield break;

        // Resume the NavMeshAgent's movement.
        agent.isStopped = false;
        // Calculate the index of the next patrol point, wrapping around to the start if necessary.
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        // Set the NavMeshAgent's destination to the next patrol point's position.
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }



    public void InteractWithPlayer(bool dialogueActive)
    {
        // Set the dialogue active flag.
        isDialogueActive = dialogueActive;
        // If Fahari is alerted or already interacting, exit the method.
        if (currentState == NpcState.Alerted || isInteracting) return;

        // Set the interacting flag to true.
        isInteracting = true;
        // Set the current state to interacting.
        currentState = NpcState.Interacting;
        // Stop the NavMeshAgent's movement.
        agent.isStopped = true;
        // Clear the NavMeshAgent's velocity.
        agent.velocity = Vector3.zero;
        // Disable NavMeshAgent's rotation updates.
        agent.updateRotation = false;

        // Reset the react animation trigger.
        animator.ResetTrigger(reactHash);
        // Set the talking animation to true.
        animator.SetBool(isTalkingHash, true);

        // Start the coroutine to smoothly look at the player.
        StartCoroutine(SmoothLookAt(player.position));
    }

    private void HandleInteraction()
    {
        // If Fahari is not interacting, exit the method.
        if (!isInteracting) return;

        // If the angle between Fahari's forward direction and the direction to the player is greater than 5 degrees,
        // start the coroutine to smoothly look at the player.
        if (Vector3.Angle(transform.forward, player.position - transform.position) > 5f)
        {
            StartCoroutine(SmoothLookAt(player.position));
        }
    }

    public void EndInteraction()
    {
        // Reset the dialogue active flag when the interaction ends.
        isDialogueActive = false;
        // If Fahari is not in the interacting state, exit the method.
        if (currentState != NpcState.Interacting) return;

        // Set the interacting flag to false.
        isInteracting = false;
        // Set the current state to patrolling.
        currentState = NpcState.Patrolling;
        // Resume the NavMeshAgent's movement.
        agent.isStopped = false;
        // Enable NavMeshAgent's rotation updates.
        agent.updateRotation = true;
        // Set the talking animation to false.
        animator.SetBool(isTalkingHash, false);

        // If there are patrol points, set the NavMeshAgent's destination to the current patrol point.
        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    public void Stagger()
    {
        // Prevent repeated staggers while already staggered.
        if (!isStaggered)
        {
            // Set the staggered flag to true.
            isStaggered = true;
            // Stop the NavMeshAgent's movement.
            agent.isStopped = true;
            // Clear the NavMeshAgent's velocity.
            agent.velocity = Vector3.zero;
            // Stop the walking animation.
            animator.SetBool(isWalkingHash, false);

            // Start the coroutine to recover from the stagger.
            StartCoroutine(RecoverFromStagger());
        }
    }

    private IEnumerator RecoverFromStagger()
    {
        // Wait for the stagger duration.
        yield return new WaitForSeconds(staggerDuration);
        // Resume the NavMeshAgent's movement.
        agent.isStopped = false;
        // Reset the staggered flag to false.
        isStaggered = false;
        // If Fahari is in the patrolling state and there are patrol points,
        // set the NavMeshAgent's destination to the current patrol point.
        if (currentState == NpcState.Patrolling && patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    private IEnumerator SmoothLookAt(Vector3 targetPosition)
    {
        // Calculate the normalized direction from Fahari to the target position.
        Vector3 direction = (targetPosition - transform.position).normalized;
        // Set the y-component of the direction to zero to prevent vertical rotation.
        direction.y = 0;
        // Create a rotation that looks in the direction of the calculated direction.
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Smoothly rotate Fahari towards the target rotation until the angle between the current rotation and the target rotation is less than 2 degrees.
        while (Quaternion.Angle(transform.rotation, targetRotation) > 2f)
        {
            // Smoothly interpolate Fahari's rotation towards the target rotation.
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
        // If Fahari is not in the patrolling state, exit the method.
        if (currentState != NpcState.Patrolling) return;
        // If the item is outside the detection radius, exit the method.
        if (Vector3.Distance(transform.position, itemPosition) > detectionRadius) return;

        // Trigger an investigation at the item's position.
        TriggerInvestigation(itemPosition);
    }

    private void TriggerInvestigation(Vector3 itemPosition)
    {
        // If Fahari is not in the patrolling state, exit the method.
        if (currentState != NpcState.Patrolling) return;
        // If the item position is not reachable, exit the method.
        if (!IsPositionReachable(itemPosition)) return;

        // Start the investigation routine.
        StartCoroutine(InvestigationRoutine(itemPosition));
    }

    private IEnumerator InvestigationRoutine(Vector3 itemPosition)
    {
        // Set Fahari's state to investigating.
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

        // If the CharacterAI component is alerted, alert Fahari.
        if (characterAI.IsAlerted)
        {
            Alert();
        }
        // Otherwise, return Fahari to patrol.
        else
        {
            ReturnToPatrol();
        }
    }

    public void Alert()
    {
        // If Fahari is already alerted, exit the method.
        if (currentState == NpcState.Alerted) return;

        // Set Fahari's state to alerted.
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
        // Set Fahari's state to patrolling.
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
        // Calculate the path from Fahari's current position to the target position.
        // Return true if the path is calculated successfully and the path status is PathComplete.
        return agent.CalculatePath(target, path) && path.status == NavMeshPathStatus.PathComplete;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the OnItemLanded event when Fahari is destroyed.
        DroppedItem.OnItemLanded -= HandleItemLanded;
    }
}