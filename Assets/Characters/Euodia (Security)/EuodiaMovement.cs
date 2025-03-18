using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EuodiaMovement : MonoBehaviour
{
    [Header("Patrol Settings")]
    public List<PatrolRoute> patrolRoutes;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;
    public float walkSpeed = 5f;
    public float runSpeed = 15f;

    [Header("Waypoint Proximity Settings")]
    public float waypointProximityThreshold = 3f;
    public float speedAdjustmentDistance = 10f;

    [Header("Audio Settings (Optional)")]
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private AudioClip footstepClip;

    private int currentRouteIndex = 0;
    private int currentWaypointIndex = 0; 
    private Animator animator;
    public NavMeshAgent navMeshAgent { get; private set; }
    public bool isPatrolling = true;

    private Transform followTarget;
    private Transform chaseTarget;
    private bool isFollowing = false;
    private bool isChasing = false;

    [Header("Vision Settings")]
    public float viewDistance = 10f;
    public float viewAngle = 90f;
    public LayerMask targetLayer;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Animator component is missing from this object.");
        }
        ValidatePatrolRoutes();
        ChooseRandomRoute();

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.acceleration = 20;
        navMeshAgent.angularSpeed = 500;
    }

    void Update()
    {
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

        UpdateAnimation();
    }

    private void ValidatePatrolRoutes()
    {
        if (patrolRoutes == null || patrolRoutes.Count == 0)
        {
            Debug.LogWarning("No patrol routes assigned! Please add patrol routes in the inspector.");
        }
    }

    private bool CanSeeTarget(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        float distance = direction.magnitude;

        if (distance > viewDistance) return false;

        float angle = Vector3.Angle(transform.forward, direction.normalized);
        if (angle > viewAngle / 2) return false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction.normalized, out hit, viewDistance, targetLayer))
        {
            if (hit.transform == target)
                return true;
        }
        return false;
    }

    public void PatrolBehavior()
    {
        if (patrolRoutes == null || patrolRoutes.Count == 0 ||
            patrolRoutes[currentRouteIndex] == null ||
            patrolRoutes[currentRouteIndex].waypoints == null ||
            patrolRoutes[currentRouteIndex].waypoints.Count == 0)
        {
            Debug.LogError("Patrol route or waypoints are not properly set up!");
            return;
        }

        Transform currentWaypoint = patrolRoutes[currentRouteIndex].waypoints[currentWaypointIndex];
        float distanceToWaypoint = Vector3.Distance(currentWaypoint.position, transform.position);

        if (distanceToWaypoint <= waypointProximityThreshold)
        {
            StartCoroutine(WaitAndProceed());
        }

        navMeshAgent.speed = (distanceToWaypoint > speedAdjustmentDistance) ? runSpeed : walkSpeed;
        navMeshAgent.SetDestination(currentWaypoint.position);

        if (footstepAudioSource != null && footstepClip != null && navMeshAgent.velocity.magnitude > 0.1f)
        {
            if (!footstepAudioSource.isPlaying)
            {
                footstepAudioSource.PlayOneShot(footstepClip);
            }
        }
    }

    private IEnumerator WaitAndProceed()
    {
        isPatrolling = false;
        yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));

        currentWaypointIndex = (currentWaypointIndex + 1) % patrolRoutes[currentRouteIndex].waypoints.Count;
        if (currentWaypointIndex == 0)
        {
            ChooseRandomRoute();
        }
        isPatrolling = true;
    }

    public void ChooseRandomRoute()
    {
        if (patrolRoutes == null || patrolRoutes.Count == 0)
        {
            Debug.LogError("No patrol routes available!");
            return;
        }
        currentRouteIndex = Random.Range(0, patrolRoutes.Count);
        currentWaypointIndex = 0;
    }

    public void StopPatrolling()
    {
        isPatrolling = false;
        navMeshAgent.SetDestination(transform.position);
    }

    public void ResumePatrolling()
    {
        isPatrolling = true;
        isFollowing = false;
        isChasing = false;
        navMeshAgent.speed = walkSpeed;
        ChooseRandomRoute();
    }

    public void SetSpeed(float speed)
    {
        navMeshAgent.speed = speed;
    }

    public void FollowTarget(Transform target)
    {
        isPatrolling = false;
        isFollowing = true;
        isChasing = false;
        followTarget = target;
        navMeshAgent.speed = walkSpeed;
        navMeshAgent.isStopped = false;
    }

    public void ChaseTarget(Transform target)
    {
        isPatrolling = false;
        isFollowing = false;
        isChasing = true;
        chaseTarget = target;
        navMeshAgent.speed = runSpeed;
        navMeshAgent.isStopped = false;
    }

    public void StartChasing(Transform target)
    {
        isPatrolling = false;
        isFollowing = false;
        isChasing = true;
        chaseTarget = target;
        navMeshAgent.speed = runSpeed;
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(target.position);
        MoveToTarget(target, runSpeed, true);
    }

    public void StopChasing()
    {
        isChasing = false;
        chaseTarget = null;
        ResumePatrolling();
    }

    public void StartFollowing(Transform target)
    {
        isPatrolling = false;
        isFollowing = true;
        isChasing = false;
        followTarget = target;
        navMeshAgent.speed = walkSpeed;
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(target.position);
        MoveToTarget(target, walkSpeed, false);
    }

    public void StopFollowing()
    {
        isFollowing = false;
        followTarget = null;
        ResumePatrolling();
    }

    private void FollowBehavior()
    {
        if (followTarget == null)
        {
            ResumePatrolling();
            return;
        }

        navMeshAgent.SetDestination(followTarget.position);
        float distanceToTarget = Vector3.Distance(followTarget.position, transform.position);
        navMeshAgent.isStopped = (distanceToTarget <= waypointProximityThreshold);
    }

    private void ChaseBehavior()
    {
        if (chaseTarget == null)
        {
            Debug.LogError("Chase target is null!");
            ResumePatrolling();
            return;
        }

        navMeshAgent.SetDestination(chaseTarget.position);
        float distanceToTarget = Vector3.Distance(chaseTarget.position, transform.position);

        Vector3 predictedPosition = chaseTarget.position + chaseTarget.GetComponent<Rigidbody>().linearVelocity * 0.2f;

        Vector3 directionToTarget = predictedPosition - transform.position;
        directionToTarget.y = 0; 
        if (directionToTarget != Vector3.zero) 
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f); 
        }

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


    public void MoveToTarget(Transform target, float speed, bool isChase)
    {
        isPatrolling = false;
        isFollowing = !isChase;
        isChasing = isChase;
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

        navMeshAgent.speed = speed;
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(target.position);
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        float speed = navMeshAgent.velocity.magnitude;
        animator.SetFloat("EuSpeed", speed);

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
        animator.SetBool("IsChasing", isChasing);
    }
}