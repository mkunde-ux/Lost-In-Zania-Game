using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MichaelMovement : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private Animator animator;

    private bool isPatrolling = true;
    private bool isFollowingPlayer = false;
    private bool isChasingPlayer = false;

    private Transform player;
    public float walkSpeed = 5f;
    public float runSpeed = 15f;

    public List<PatrolRoute> patrolRoutes;
    private int currentRouteIndex = 0;
    private int currentWayPointIndex = 0;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;
    public float followDistance = 3f;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        ChooseRandomRoute();
    }

    void Update()
    {
        if (isPatrolling)
        {
            PatrolBehavior();
        }
        else if (isFollowingPlayer)
        {
            FollowPlayerBehavior();
        }
        else if (isChasingPlayer)
        {
            ChasePlayerBehavior();
        }
    }

    public void StartPatrolling()
    {
        isPatrolling = true;
        isFollowingPlayer = false;
        isChasingPlayer = false;
        navMeshAgent.speed = walkSpeed;
        ChooseRandomRoute();
    }

    public void FollowPlayer(Transform playerTransform)
    {
        if (playerTransform == null) return;
        isFollowingPlayer = true;
        isPatrolling = false;
        isChasingPlayer = false;
        player = playerTransform;
        navMeshAgent.speed = walkSpeed;
    }

    public void StartChasingPlayer(Transform playerTransform)
    {
        if (playerTransform == null) return;
        isChasingPlayer = true;
        isFollowingPlayer = false;
        isPatrolling = false;
        player = playerTransform;
        navMeshAgent.speed = runSpeed;
    }

    public void StopChasingPlayer()
    {
        isChasingPlayer = false;
        StartPatrolling();
    }

    private void FollowPlayerBehavior()
    {
        if (player == null) return;
        Vector3 followPosition = player.position - (player.position - transform.position).normalized * followDistance;
        navMeshAgent.SetDestination(followPosition);
    }

    private void ChasePlayerBehavior()
    {
        if (player == null) return;
        navMeshAgent.SetDestination(player.position);
    }

    private void PatrolBehavior()
    {
        if (patrolRoutes == null || patrolRoutes.Count == 0 || patrolRoutes[currentRouteIndex] == null ||
            patrolRoutes[currentRouteIndex].waypoints == null || patrolRoutes[currentRouteIndex].waypoints.Count == 0)
        {
            Debug.LogError("Patrol route or waypoints are not properly set up!");
            return;
        }

        float distanceToWayPoint = Vector3.Distance(patrolRoutes[currentRouteIndex].waypoints[currentWayPointIndex].position, transform.position);

        if (distanceToWayPoint <= 3f)
        {
            StartCoroutine(WaitAndProceed());
        }
        navMeshAgent.speed = (distanceToWayPoint > 10f) ? runSpeed : walkSpeed;
        navMeshAgent.SetDestination(patrolRoutes[currentRouteIndex].waypoints[currentWayPointIndex].position);
    }

    private IEnumerator WaitAndProceed()
    {
        isPatrolling = false;
        yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
        currentWayPointIndex = (currentWayPointIndex + 1) % patrolRoutes[currentRouteIndex].waypoints.Count;
        isPatrolling = true;
    }

    private void ChooseRandomRoute()
    {
        if (patrolRoutes == null || patrolRoutes.Count == 0) return;
        currentRouteIndex = Random.Range(0, patrolRoutes.Count);
        currentWayPointIndex = 0;
    }
}
