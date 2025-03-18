using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ImaniMovement : MonoBehaviour
{
    public enum NpcState { Patrolling, Interacting, Investigating, Alerted }

    [Header("Movement Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float waypointWaitTime = 2f;

    [Header("Investigation System")]
    [SerializeField] private CharacterAI characterAI;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask obstructionLayer;
    [SerializeField] private LayerMask itemLayer;

    [Header("References")]
    [SerializeField] private Animator animator;
    private NavMeshAgent agent;
    private Transform player;
    private NpcState currentState = NpcState.Patrolling;
    private int currentPatrolIndex = 0;

    private readonly int isWalkingHash = Animator.StringToHash("IMisWalking");
    private readonly int isTalkingHash = Animator.StringToHash("IMisTalking");
    private readonly int reactHash = Animator.StringToHash("IMisReact");

    private bool isStaggered = false;
    [SerializeField] private float staggerDuration = 3f;

    private bool isInteracting = false; 
    private bool isDialogueActive = false; 

    [Header("Audio Settings (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private float footstepInterval = 0.5f;
    private float footstepTimer = 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        DroppedItem.OnItemLanded += HandleItemLanded;
        characterAI?.Initialize(agent, player);
    }

    private void Start()
    {
        agent.speed = patrolSpeed;
        InitializeAnimations();
    }

    private void InitializeAnimations()
    {
        animator.SetBool(isWalkingHash, false);
        animator.SetBool(isTalkingHash, false);
        animator.ResetTrigger(reactHash);
    }

    private void Update()
    {
        UpdateMovementAnimation();

        if (!isInteracting && !isDialogueActive)
        {
            switch (currentState)
            {
                case NpcState.Patrolling:
                    Patrol();
                    CheckForItems();
                    break;
                case NpcState.Interacting:
                    HandleInteraction();
                    break;
                case NpcState.Investigating:
                    break;
                case NpcState.Alerted:
                    break;
            }
        }
    }

    private void UpdateMovementAnimation()
    {
        bool shouldWalk = agent.enabled && !agent.isStopped && agent.remainingDistance > agent.stoppingDistance;
        animator.SetBool(isWalkingHash, shouldWalk);

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
            footstepTimer = 0f;
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0 || isDialogueActive)
            return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(WaypointWait());
        }
    }


    private IEnumerator WaypointWait()
    {
        agent.isStopped = true;
        animator.SetBool(isWalkingHash, false);
        yield return new WaitForSeconds(waypointWaitTime);

        if (isDialogueActive)
            yield break;
        agent.isStopped = false;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    public void InteractWithPlayer(bool dialogueActive)
    {
        isDialogueActive = dialogueActive; 
        isInteracting = true; 
        currentState = NpcState.Interacting;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.updateRotation = false;
        animator.ResetTrigger(reactHash);
        animator.SetBool(isTalkingHash, true);
        StartCoroutine(SmoothLookAt(player.position));
    }

    private void HandleInteraction()
    {
        if (!isInteracting) return;
        if (Vector3.Angle(transform.forward, player.position - transform.position) > 5f)
        {
            StartCoroutine(SmoothLookAt(player.position));
        }
    }

    public void EndInteraction()
    {
        isDialogueActive = false;
        isInteracting = false;

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
        if (!isStaggered)
        {
            isStaggered = true;
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            animator.SetBool(isWalkingHash, false);
            StartCoroutine(RecoverFromStagger());
        }
    }

    private IEnumerator RecoverFromStagger()
    {
        yield return new WaitForSeconds(staggerDuration);
        agent.isStopped = false;
        isStaggered = false;
        if (currentState == NpcState.Patrolling && patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    private IEnumerator SmoothLookAt(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 2f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void CheckForItems()
    {
        Collider[] items = Physics.OverlapSphere(transform.position, detectionRadius, itemLayer);
        foreach (Collider item in items)
        {
            DroppedItem droppedItem = item.GetComponent<DroppedItem>();
            if (droppedItem != null && droppedItem.isDropped && !droppedItem.isInvestigated)
            {
                TriggerInvestigation(item.transform.position);
                droppedItem.MarkAsInvestigated();
                break;
            }
        }
    }

    private void HandleItemLanded(Vector3 itemPosition)
    {
        if (currentState != NpcState.Patrolling) return;
        if (Vector3.Distance(transform.position, itemPosition) > detectionRadius) return;
        TriggerInvestigation(itemPosition);
    }

    private void TriggerInvestigation(Vector3 itemPosition)
    {
        if (currentState != NpcState.Patrolling) return;
        if (!IsPositionReachable(itemPosition)) return;
        StartCoroutine(InvestigationRoutine(itemPosition));
    }

    private IEnumerator InvestigationRoutine(Vector3 itemPosition)
    {
        currentState = NpcState.Investigating;
        characterAI.StartInvestigation(itemPosition);

        while (characterAI.IsSearching)
        {
            animator.SetBool(isWalkingHash, agent.velocity.magnitude > 0.1f);
            yield return null;
        }

        if (characterAI.IsAlerted)
        {
            Alert();
        }
        else
        {
            ReturnToPatrol();
        }
    }

    public void Alert()
    {
        if (currentState == NpcState.Alerted) return;

        currentState = NpcState.Alerted;
        agent.isStopped = true;
        animator.SetTrigger(reactHash);
    }

    private void ReturnToPatrol()
    {
        animator.ResetTrigger(reactHash);
        currentState = NpcState.Patrolling;
        agent.isStopped = false;

        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    private bool IsPositionReachable(Vector3 target)
    {
        NavMeshPath path = new NavMeshPath();
        return agent.CalculatePath(target, path) && path.status == NavMeshPathStatus.PathComplete;
    }

    private void OnDestroy()
    {
        DroppedItem.OnItemLanded -= HandleItemLanded;
    }
}
