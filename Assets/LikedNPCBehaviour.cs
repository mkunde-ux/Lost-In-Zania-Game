using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LikedNPCBehaviour : MonoBehaviour
{
  
    public float moveSpeed = 2f; // Speed for moving towards/away
    public float moveAwayDuration = 5f; // Duration for moving away

    public string playerTag = "Player";
    public string dislikedNPCTag = "Nes";

    private Transform dislikedNPC;
    private Transform player;
    private NavMeshAgent navMeshAgent; // Use NavMeshAgent for movement

    private bool isMovingAway = false;
    private float moveAwayTimer = 0f;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>(); // Initialize NavMeshAgent
    }

    private void Update()
    {
        DetectNearbyEntities();
        HandleBehavior();
    }

    void DetectNearbyEntities()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10f); // Detection radius

        dislikedNPC = null;
        player = null;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(dislikedNPCTag))
            {
                dislikedNPC = hitCollider.transform;
            }
            else if (hitCollider.CompareTag(playerTag))
            {
                player = hitCollider.transform;
            }
        }
    }

    void HandleBehavior()
    {
        if (player != null && dislikedNPC != null && !isMovingAway) // Mid trust ending condition
        {
            MoveTowards(dislikedNPC);
            StartCoroutine(DislikedNPCMoveAway());
        }

        if (isMovingAway)
        {
            moveAwayTimer += Time.deltaTime;
            if (moveAwayTimer >= moveAwayDuration)
            {
                isMovingAway = false;
                moveAwayTimer = 0f;
                // You might want to reset the disliked NPC's position/behavior here.
            }
        }
    }

    public void MoveTowards(Transform target) // Change from private to public
    {
        navMeshAgent.SetDestination(target.position);
    }

    public IEnumerator DislikedNPCMoveAway() // Change from private to public
    {
        isMovingAway = true;

        if (dislikedNPC != null)
        {
            NavMeshAgent dislikedAgent = dislikedNPC.GetComponent<NavMeshAgent>();

            if (dislikedAgent != null)
            {
                Vector3 awayDirection = (dislikedNPC.position - transform.position).normalized;
                Vector3 targetPosition = dislikedNPC.position + awayDirection * 10f; // Move 10 units away

                dislikedAgent.SetDestination(targetPosition);
            }
            else
            {
                Debug.LogError("Disliked NPC does not have a NavMeshAgent component!");
            }
        }

        yield return null; // Let the Update loop handle the moveAwayTimer
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 10f); // Detection radius
    }
    
}