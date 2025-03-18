using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class AIInvestigationManager : MonoBehaviour
{
/*
    public FieldOfViewNPC fieldOfView; // Corrected: Public field for FieldOfViewNPC

    public float detectionRadius = 5f;
    public float fovAngle = 90f;
    public LayerMask obstructionLayer;
    public LayerMask itemLayer;
    public float searchRadius = 8f;
    public int searchPoints = 4;
    public float pointSpacing = 2f;
    public float searchDuration = 25f;
    public float viewDistance = 10f; // Corrected: Added declaration for viewDistance


    private NavMeshAgent agent;
    private Transform player;
    private Coroutine searchRoutine;
    private Vector3 investigationOrigin;
    public bool IsSearching { get; private set; }
    public bool IsAlerted { get; private set; }

    public float investigationTime = 10f;
    private float investigationTimer;
    private bool isInvestigating = false;
    private Vector3 lastKnownPosition; // Corrected: Changed to Vector3
    public SecurityAI securityGuard;

    void Start()
    {
        fieldOfView = GetComponent<FieldOfViewNPC>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (fieldOfView != null)
        {
            if (isInvestigating)
            {
                investigationTimer -= Time.deltaTime;
                if (investigationTimer <= 0f)
                {
                    ReturnToPatrol();
                }
                else if (fieldOfView.canSeePlayer)
                {
                    AlertSecurity();
                }
            }
        }
        else
        {
            Debug.LogError("FieldOfView component not assigned in AIInvestigationManager on " + gameObject.name);
        }
    }


    public void Initialize(NavMeshAgent navAgent, Transform playerTransform)
    {
        agent = navAgent;
        player = playerTransform;
    }

    public void StartInvestigation(Vector3 position)
    {
        isInvestigating = true;
        investigationTimer = investigationTime;
        lastKnownPosition = position;
        agent.SetDestination(lastKnownPosition); // Move to initial point

        // Start the search coroutine AFTER arriving at the initial point.
        StartCoroutine(StartSearchAfterArrival(position));
    }

    private IEnumerator StartSearchAfterArrival(Vector3 position)
    {
        // Wait until the agent reaches the dropped item position
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        investigationOrigin = position; // Set investigation origin AFTER arrival
        searchRoutine = StartCoroutine(SearchBehavior()); // Start the search
    }

    private void ReturnToPatrol()
    {
        isInvestigating = false;
        if (searchRoutine != null) // Stop the search if interrupted
        {
            StopCoroutine(searchRoutine);
            searchRoutine = null;
        }
        Debug.Log($"{gameObject.name} is returning to patrol.");
    }

    private void AlertSecurity()
    {
        Debug.Log("Player detected within investigation time! Alerting security!");
        if (securityGuard != null)
        {
            securityGuard.StartChasingPlayer(fieldOfView.player);
        }
    }

    private IEnumerator SearchBehavior()
    {
        IsAlerted = false;
        IsSearching = true;
        float searchTimer = searchDuration;
        bool investigationComplete = false; // Track if the full investigation is complete

        List<Vector3> searchPattern = GenerateSpiralPattern();

        foreach (Vector3 point in searchPattern)
        {
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                yield return MoveToInvestigationPoint(hit.position);
                yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

                // Check for player during search
                if (IsPlayerInFOV() && player != null)
                {
                    IsAlerted = true;
                    IsSearching = false;
                    yield break; // Stop searching and alert security
                }
            }

            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0) break;
        }

        investigationComplete = true;
        IsSearching = false;

        if (investigationComplete)
        {
            ReturnToPatrol(); // Ensure the NPC returns only after full investigation
        }
    }


    private List<Vector3> GenerateSpiralPattern()
    {
        List<Vector3> points = new List<Vector3>();
        float angleIncrement = 360f / searchPoints;
        float currentRadius = 0;

        for (int i = 0; i < searchPoints; i++)
        {
            float angle = i * angleIncrement;
            currentRadius += pointSpacing;
            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * currentRadius,
                0,
                Mathf.Sin(angle * Mathf.Deg2Rad) * currentRadius
            );
            points.Add(investigationOrigin + offset);
        }

        return points;
    }

    private IEnumerator MoveToInvestigationPoint(Vector3 target)
    {
        agent.isStopped = false;
        agent.SetDestination(target);

        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        agent.isStopped = true;
    }

    private bool IsPlayerInFOV()
    {
        if (player == null || fieldOfView == null) return false;

        Vector3 directionToPlayer = player.position - agent.transform.position;
        float angle = Vector3.Angle(directionToPlayer, agent.transform.forward);

        if (angle < fieldOfView.viewAngle / 2 && directionToPlayer.magnitude < detectionRadius)
        {
            if (!Physics.Linecast(agent.transform.position, player.position, obstructionLayer))
            {
                return true;
            }
        }

        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (fieldOfView != null)
        {
            Vector3 fovLine1 = Quaternion.AngleAxis(fieldOfView.viewAngle / 2, transform.up) * transform.forward * viewDistance;
            Vector3 fovLine2 = Quaternion.AngleAxis(-fieldOfView.viewAngle / 2, transform.up) * transform.forward * viewDistance;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + fovLine1);
            Gizmos.DrawLine(transform.position, transform.position + fovLine2);
        }
    }

    */
}
