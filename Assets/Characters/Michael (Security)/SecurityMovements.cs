using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class SecurityMovement : MonoBehaviour
{
    [Header("Patrol Settings")]
    // List of patrol routes the guard can follow.
    public List<PatrolRoute> patrolRoutes;
    // Minimum wait time at each waypoint.
    public float minWaitTime = 2f;
    // Maximum wait time at each waypoint.
    public float maxWaitTime = 5f;
    // Walking speed of the guard.
    public float walkSpeed = 5f;
    // Running speed of the guard.
    public float runSpeed = 15f;

    [Header("Waypoint Proximity Settings")]
    // Distance threshold to consider the guard has reached a waypoint.
    public float waypointProximityThreshold = 3f;
    // Distance at which the guard transitions from walking to running.
    public float speedAdjustmentDistance = 10f;

    [Header("Audio Settings (Optional)")]
    // Audio source for footstep sounds.
    [SerializeField] private AudioSource footstepAudioSource;
    // Audio clip for footstep sounds.
    [SerializeField] private AudioClip footstepClip;

    // Index of the current patrol route.
    private int currentRouteIndex = 0;
    // Index of the current waypoint within the route.
    private int currentWaypointIndex = 0;
    // Animator component for animations.
    private Animator animator;
    // NavMeshAgent component for navigation.
    public NavMeshAgent navMeshAgent { get; private set; }
    // Flag to indicate if the guard is patrolling.
    public bool isPatrolling = true;

    // Transform of the target to follow.
    private Transform followTarget;
    // Transform of the target to chase.
    private Transform chaseTarget;
    // Flag to indicate if the guard is following a target.
    private bool isFollowing = false;
    // Flag to indicate if the guard is chasing a target.
    private bool isChasing = false;

    [Header("Vision Settings")]
    // Distance the guard can see.
    public float viewDistance = 10f;
    // Angle of the guard's field of view.
    public float viewAngle = 90f;
    // Layer mask for target detection.
    public LayerMask targetLayer;

    // Start method called when the script is initialized.
    void Start()
    {
        // Get the NavMeshAgent and Animator components.
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        // Log a warning if the Animator component is missing.
        if (animator == null)
        {
            Debug.LogWarning("Animator component is missing from this object.");
        }
        // Validate the patrol routes.
        ValidatePatrolRoutes();
        // Choose a random patrol route.
        ChooseRandomRoute();

        // Initialize NavMeshAgent settings.
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.acceleration = 20;
        navMeshAgent.angularSpeed = 500;
    }

    // Update method called every frame.
    void Update()
    {
        // Handle chasing, following, or patrolling behavior based on current state.
        if (isChasing && chaseTarget != null)
        {
            ChaseBehavior();
        }
        else if (isFollowing && followTarget != null)
        {
            FollowBehavior();
        }
        else if (isPatrolling)
        {
            PatrolBehavior();
        }

        // Update animations.
        UpdateAnimation();
    }

    // Validates the patrol routes.
    private void ValidatePatrolRoutes()
    {
        // Log a warning if no patrol routes are assigned.
        if (patrolRoutes == null || patrolRoutes.Count == 0)
        {
            Debug.LogWarning("No patrol routes assigned! Please add patrol routes in the inspector.");
        }
    }

    // Checks if the guard can see a target.
    private bool CanSeeTarget(Transform target)
    {
        // Calculate the direction and distance to the target.
        Vector3 direction = target.position - transform.position;
        float distance = direction.magnitude;

        // Return false if the target is out of view distance.
        if (distance > viewDistance) return false;

        // Calculate the angle to the target.
        float angle = Vector3.Angle(transform.forward, direction.normalized);
        // Return false if the target is out of view angle.
        if (angle > viewAngle / 2) return false;

        // Perform a raycast to check for obstacles.
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction.normalized, out hit, viewDistance, targetLayer))
        {
            // Return true if the raycast hits the target.
            if (hit.transform == target)
                return true;
        }
        // Return false if the raycast does not hit the target.
        return false;
    }

    // Handles the patrolling behavior.
    public void PatrolBehavior()
    {
        // Log an error and return if patrol routes or waypoints are not set up properly.
        if (patrolRoutes == null || patrolRoutes.Count == 0 ||
            patrolRoutes[currentRouteIndex] == null ||
            patrolRoutes[currentRouteIndex].waypoints == null ||
            patrolRoutes[currentRouteIndex].waypoints.Count == 0)
        {
            Debug.LogError("Patrol route or waypoints are not properly set up!");
            return;
        }

        // Get the current waypoint.
        Transform currentWaypoint = patrolRoutes[currentRouteIndex].waypoints[currentWaypointIndex];
        // Calculate the distance to the waypoint.
        float distanceToWaypoint = Vector3.Distance(currentWaypoint.position, transform.position);

        // Start waiting and proceeding to the next waypoint if the guard has reached the current waypoint.
        if (distanceToWaypoint <= waypointProximityThreshold)
        {
            StartCoroutine(WaitAndProceed());
        }

        // Set the NavMeshAgent's speed and destination based on the distance to the waypoint.
        navMeshAgent.speed = (distanceToWaypoint > speedAdjustmentDistance) ? runSpeed : walkSpeed;
        navMeshAgent.SetDestination(currentWaypoint.position);

        // Play footstep sounds if the guard is moving.
        if (footstepAudioSource != null && footstepClip != null && navMeshAgent.velocity.magnitude > 0.1f)
        {
            if (!footstepAudioSource.isPlaying)
            {
                footstepAudioSource.PlayOneShot(footstepClip);
            }
        }
    }


    // Coroutine to wait at a waypoint and proceed to the next one.
    private IEnumerator WaitAndProceed()
    {
        // Stop patrolling temporarily.
        isPatrolling = false;
        // Wait for a random duration between minWaitTime and maxWaitTime.
        yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));

        // Move to the next waypoint in the current route.
        currentWaypointIndex = (currentWaypointIndex + 1) % patrolRoutes[currentRouteIndex].waypoints.Count;
        // If the end of the route is reached, choose a new random route.
        if (currentWaypointIndex == 0)
        {
            ChooseRandomRoute();
        }
        // Resume patrolling.
        isPatrolling = true;
    }

    // Chooses a random patrol route.
    public void ChooseRandomRoute()
    {
        // Log an error and return if no patrol routes are available.
        if (patrolRoutes == null || patrolRoutes.Count == 0)
        {
            Debug.LogError("No patrol routes available!");
            return;
        }
        // Choose a random route index.
        currentRouteIndex = Random.Range(0, patrolRoutes.Count);
        // Reset the waypoint index to the start of the route.
        currentWaypointIndex = 0;
    }

    // Stops the guard from patrolling.
    public void StopPatrolling()
    {
        // Set isPatrolling to false.
        isPatrolling = false;
        // Stop the NavMeshAgent's movement.
        navMeshAgent.SetDestination(transform.position);
    }

    // Resumes the guard's patrolling behavior.
    public void ResumePatrolling()
    {
        // Set isPatrolling to true and other states to false.
        isPatrolling = true;
        isFollowing = false;
        isChasing = false;
        // Set the NavMeshAgent's speed to walkSpeed.
        navMeshAgent.speed = walkSpeed;
        // Choose a new random route.
        ChooseRandomRoute();
    }

    // Sets the NavMeshAgent's speed.
    public void SetSpeed(float speed)
    {
        // Set the NavMeshAgent's speed to the specified value.
        navMeshAgent.speed = speed;
    }

    // Sets the guard to follow a target.
    public void FollowTarget(Transform target)
    {
        // Stop patrolling and set isFollowing to true.
        isPatrolling = false;
        isFollowing = true;
        isChasing = false;
        // Set the follow target and NavMeshAgent's speed and destination.
        followTarget = target;
        navMeshAgent.speed = walkSpeed;
        navMeshAgent.isStopped = false;
    }

    // Sets the guard to chase a target.
    public void ChaseTarget(Transform target)
    {
        // Stop patrolling and set isChasing to true.
        isPatrolling = false;
        isFollowing = false;
        isChasing = true;
        // Set the chase target and NavMeshAgent's speed and destination.
        chaseTarget = target;
        navMeshAgent.speed = runSpeed;
        navMeshAgent.isStopped = false;
    }

    // Starts chasing a target.
    public void StartChasing(Transform target)
    {
        // Stop patrolling and set isChasing to true.
        isPatrolling = false;
        isFollowing = false;
        isChasing = true;
        // Set the chase target and NavMeshAgent's speed and destination.
        chaseTarget = target;
        navMeshAgent.speed = runSpeed;
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(target.position);
        // Move to the target with run speed.
        MoveToTarget(target, runSpeed, true);
    }

    // Stops the guard from chasing.
    public void StopChasing()
    {
        // Set isChasing to false and chaseTarget to null.
        isChasing = false;
        chaseTarget = null;
        // Resume patrolling.
        ResumePatrolling();
    }

    // Starts following a target.
    public void StartFollowing(Transform target)
    {
        // Stop patrolling and set isFollowing to true.
        isPatrolling = false;
        isFollowing = true;
        isChasing = false;
        // Set the follow target and NavMeshAgent's speed and destination.
        followTarget = target;
        navMeshAgent.speed = walkSpeed;
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(target.position);
        // Move to the target with walk speed.
        MoveToTarget(target, walkSpeed, false);
    }

    // Stops the guard from following.
    public void StopFollowing()
    {
        // Set isFollowing to false and followTarget to null.
        isFollowing = false;
        followTarget = null;
        // Resume patrolling.
        ResumePatrolling();
    }

    // Handles the following behavior.
    private void FollowBehavior()
    {
        // Resume patrolling if the follow target is null.
        if (followTarget == null)
        {
            ResumePatrolling();
            return;
        }

        // Set the NavMeshAgent's destination to the follow target.
        navMeshAgent.SetDestination(followTarget.position);
        // Calculate the distance to the follow target.
        float distanceToTarget = Vector3.Distance(followTarget.position, transform.position);
        // Stop the NavMeshAgent if the guard is close enough to the target.
        navMeshAgent.isStopped = (distanceToTarget <= waypointProximityThreshold);
    }

    // Handles the chasing behavior.
    private void ChaseBehavior()
    {
        // Log an error and resume patrolling if the chase target is null.
        if (chaseTarget == null)
        {
            Debug.LogError("Chase target is null!");
            ResumePatrolling();
            return;
        }

        // Set the NavMeshAgent's destination to the chase target.
        navMeshAgent.SetDestination(chaseTarget.position);
        // Calculate the distance to the chase target.
        float distanceToTarget = Vector3.Distance(chaseTarget.position, transform.position);

        // Predict the target's position based on its velocity.
        Vector3 predictedPosition = chaseTarget.position + chaseTarget.GetComponent<Rigidbody>().linearVelocity * 0.2f;

        // Calculate the direction to the predicted target position.
        Vector3 directionToTarget = predictedPosition - transform.position;
        directionToTarget.y = 0;
        // Rotate the guard to face the predicted target position.
        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Stop the NavMeshAgent if the guard is close enough to the target.
        if (distanceToTarget <= waypointProximityThreshold)
        {
            Debug.Log("Target caught!");
            navMeshAgent.isStopped = true;
        }
        else
        {
            navMeshAgent.isStopped = false;
        }
    }

    // Moves the guard to a target with a specified speed.
    public void MoveToTarget(Transform target, float speed, bool isChase)
    {
        // Stop patrolling and set the appropriate state flags.
        isPatrolling = false;
        isFollowing = !isChase;
        isChasing = isChase;
        // Set the chase or follow target based on the chase flag.
        if (isChase)
        {
            chaseTarget = target;
            followTarget = null;
        }
        else
        {
            followTarget = target;
            chaseTarget = null;
        }

        // Set the NavMeshAgent's speed and destination.
        navMeshAgent.speed = speed;
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(target.position);
    }

    // Updates the guard's animations.
    private void UpdateAnimation()
    {
        // Return if the animator is null.
        if (animator == null) return;

        // Get the NavMeshAgent's speed.
        float speed = navMeshAgent.velocity.magnitude;
        // Set the animator's speed parameter.
        animator.SetFloat("Speed", speed);

        // Set the animator's running and walking parameters based on the speed.
        if (speed > 10f)
        {
            animator.SetBool("IsRunning", true);
            animator.SetBool("IsWalking", false);
        }
        else if (speed > 0.01f)
        {
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsWalking", true);
        }
        else
        {
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsWalking", false);
        }

        // Set the animator's chasing parameter.
        animator.SetBool("IsChasing", isChasing);
    }
}
