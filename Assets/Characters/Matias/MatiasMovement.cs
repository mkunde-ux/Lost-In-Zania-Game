using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MatiasMovement : MonoBehaviour
{
    [Header("References")]
    // Reference to the player's transform.
    public Transform player;
    // Reference to the NavMeshAgent component.
    public NavMeshAgent agent;
    // Reference to the PlayerMovement script.
    public PlayerMovement playerMovement;

    [Header("Waypoint Settings")]
    // Array of waypoint transforms.
    public Transform[] waypoints;
    // Distance at which Matias considers a waypoint reached.
    public float waypointReachDistance = 0.5f;
    // Time Matias waits at each waypoint.
    public float waypointWaitTime = 1.5f;
    // Flag to determine if waypoints are visited in random order.
    public bool randomOrder = false;

    [Header("Movement Settings")]
    // Distance at which Matias starts following the player.
    public float followDistance = 6f;
    // Minimum distance Matias keeps from the player.
    public float minPlayerDistance = 2f;
    // Speed at which Matias patrols between waypoints.
    public float patrolSpeed = 3f;
    // Damping factor for matching player speed.
    public float speedMatchDamping = 0.5f;

    [Header("Rotation Settings")]
    // Speed at which Matias rotates.
    public float rotationSpeed = 5f;

    [Header("Player Stationary Detection")]
    // Time in seconds before Matias resumes patrolling if the player is stationary.
    public float playerStationaryThreshold = 10f;

    // Index of the current waypoint.
    private int currentWaypointIndex = 0;
    // Timer for waiting at waypoints.
    private float waitTimer = 0f;
    // Last recorded position of the player.
    private Vector3 lastPlayerPosition;
    // Flag indicating if Matias is currently patrolling.
    private bool isPatrolling = true;
    // Flag indicating if Matias is currently waiting at a waypoint.
    private bool isWaiting = false;
    // Current speed at which Matias follows the player.
    private float currentFollowSpeed;
    // Current direction of patrol.
    private Vector3 currentPatrolDirection;
    // Timer to track how long the player has been stationary.
    private float playerStationaryTimer = 0f;

    // Enum to track the current movement state.
    private enum MovementState { Idle, WaypointMovement, PlayerMovement }
    // Current movement state.
    private MovementState currentState = MovementState.WaypointMovement;

    void Start()
    {
        // Get NavMeshAgent component if not already assigned.
        if (!agent) agent = GetComponent<NavMeshAgent>();
        // Disable automatic rotation by the NavMeshAgent.
        agent.updateRotation = false;

        // Get PlayerMovement component if not already assigned.
        if (!playerMovement) playerMovement = player.GetComponent<PlayerMovement>();

        // Initialize last player position and movement speeds.
        lastPlayerPosition = player.position;
        agent.speed = patrolSpeed;
        currentFollowSpeed = playerMovement.walkSpeed;

        // Initialize waypoint index and start moving to the first waypoint.
        InitializeWaypointIndex();
        MoveToWaypoint();

        // Initialize patrol direction.
        if (waypoints.Length > 0)
        {
            currentPatrolDirection = (waypoints[currentWaypointIndex].position - transform.position).normalized;
        }
    }

    void Update()
    {
        // Calculate distance to player and check if player has moved.
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool playerMoved = Vector3.Distance(lastPlayerPosition, player.position) > 0.2f;

        // Update player stationary timer.
        if (playerMoved)
        {
            playerStationaryTimer = 0f; // Reset timer if player moves
            lastPlayerPosition = player.position;
        }
        else
        {
            playerStationaryTimer += Time.deltaTime; // Increment timer if player is stationary
        }

        // Determine movement state based on player activity and distance.
        if (playerStationaryTimer >= playerStationaryThreshold && distanceToPlayer > minPlayerDistance)
        {
            currentState = MovementState.WaypointMovement; // Prioritize waypoints if player is stationary
        }
        else if (distanceToPlayer > followDistance || playerMoved)
        {
            currentState = MovementState.PlayerMovement; // Prioritize player if they are moving or too far
        }

        // Handle movement and rotation based on current state.
        HandleMovement();
        HandleRotation();
    }

    // Initializes the waypoint index based on random order setting.
    void InitializeWaypointIndex()
    {
        if (waypoints.Length > 0 && randomOrder) currentWaypointIndex = Random.Range(0, waypoints.Length);
    }

    // Handles movement based on the current movement state.
    void HandleMovement()
    {
        switch (currentState)
        {
            case MovementState.WaypointMovement:
                HandlePatrolBehavior();
                break;

            case MovementState.PlayerMovement:
                FollowPlayer();
                break;
        }
    }

    // Handles rotation towards the target direction.
    void HandleRotation()
    {
        // If Matias is currently waiting at a waypoint, skip rotation.
        if (isWaiting) return;

        // Initialize targetDirection to zero.
        Vector3 targetDirection = Vector3.zero;

        // If the current state is PlayerMovement, calculate the direction to the player.
        if (currentState == MovementState.PlayerMovement)
        {
            // Calculate the normalized direction from Matias to the player.
            targetDirection = (player.position - transform.position).normalized;
        }
        // If the current state is WaypointMovement and there are waypoints, calculate the direction to the current waypoint.
        else if (currentState == MovementState.WaypointMovement && waypoints.Length > 0)
        {
            // Calculate the normalized direction from Matias to the current waypoint.
            targetDirection = (waypoints[currentWaypointIndex].position - transform.position).normalized;
        }

        // Set the y-component of targetDirection to zero to prevent vertical rotation.
        targetDirection.y = 0;

        // If targetDirection is not zero, rotate towards the target.
        if (targetDirection != Vector3.zero)
        {
            // Create a rotation that looks in the direction of targetDirection.
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            // Smoothly interpolate Matias's rotation towards targetRotation.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // Makes Matias follow the player.
    void FollowPlayer()
    {
        // If the player reference is null, exit the method.
        if (!player) return;

        // Calculate the follow position, which is the player's position plus a vector pointing away from the player, scaled by minPlayerDistance.
        Vector3 followPosition = player.position + (transform.position - player.position).normalized * minPlayerDistance;
        // Set the NavMeshAgent's destination to the calculated follow position.
        agent.SetDestination(followPosition);
        // Smoothly interpolate the agent's speed towards the player's speed, using speedMatchDamping.
        agent.speed = Mathf.Lerp(currentFollowSpeed, playerMovement.speed, speedMatchDamping * Time.deltaTime);
    }

    // Handles patrol behavior.
    void HandlePatrolBehavior()
    {
        // If Matias is currently waiting at a waypoint, exit the method.
        if (isWaiting) return;

        // If the agent's path is not pending and the remaining distance is within the stopping distance plus waypoint reach distance, start waiting at the waypoint.
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + waypointReachDistance)
        {
            StartWaitingAtWaypoint();
        }
        // Otherwise, move to the current waypoint.
        else
        {
            MoveToWaypoint();
        }
    }

    // Starts the waiting period at a waypoint.
    void StartWaitingAtWaypoint()
    {
        // Set the isWaiting flag to true.
        isWaiting = true;
        // Stop the NavMeshAgent.
        agent.isStopped = true;

        // Invoke the ResumePatrol method after waypointWaitTime seconds.
        Invoke(nameof(ResumePatrol), waypointWaitTime);
    }

    // Resumes patrolling after waiting at a waypoint.
    void ResumePatrol()
    {
        // Set the isWaiting flag to false.
        isWaiting = false;
        // Resume the NavMeshAgent.
        agent.isStopped = false;

        // Move to the next waypoint.
        MoveToNextWaypoint();
    }

    // Moves Matias to the current waypoint.
    void MoveToWaypoint()
    {
        // If there are no waypoints, exit the method.
        if (waypoints.Length == 0) return;

        // Set the NavMeshAgent's speed to patrolSpeed.
        agent.speed = patrolSpeed;
        // Set the NavMeshAgent's destination to the current waypoint's position.
        agent.SetDestination(waypoints[currentWaypointIndex].position);
        // Calculate and store the normalized direction from Matias to the current waypoint.
        currentPatrolDirection = (waypoints[currentWaypointIndex].position - transform.position).normalized;
    }

    // Moves Matias to the next waypoint.
    void MoveToNextWaypoint()
    {
        // If there are no waypoints, exit the method.
        if (waypoints.Length == 0) return;
        // Calculate the index of the next waypoint.
        // If randomOrder is true, select a random waypoint index.
        // Otherwise, increment the currentWaypointIndex and wrap around to 0 if it exceeds the number of waypoints.
        currentWaypointIndex = randomOrder ? Random.Range(0, waypoints.Length) : (currentWaypointIndex + 1) % waypoints.Length;

        // Set the NavMeshAgent's destination to the new waypoint's position.
        agent.SetDestination(waypoints[currentWaypointIndex].position);
        // Calculate and store the normalized direction from Matias to the new waypoint.
        currentPatrolDirection = (waypoints[currentWaypointIndex].position - transform.position).normalized;
    }

    // Resets the speed smoothing to the player's current speed.
    public void ResetSpeedSmoothing()
    {
        // Reset currentFollowSpeed to the player's current speed.
        currentFollowSpeed = playerMovement.speed; // Reset to the player's current speed
    }

    // Draws gizmos in the editor to visualize waypoints and movement.
    void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        Gizmos.color = Color.blue;

        foreach (Transform wp in waypoints)
        {
            if (wp != null)
                Gizmos.DrawSphere(wp.position, 0.5f);
        }

        // Draw red line to waypoint and blue line to player
        if (waypoints.Length > 0 && currentWaypointIndex < waypoints.Length)
        {
            Gizmos.color = Color.red; Gizmos.DrawLine(transform.position, waypoints[currentWaypointIndex].position);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, player.position);
    }
}