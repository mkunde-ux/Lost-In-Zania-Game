using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LikedNPC : MonoBehaviour
{
    // Movement speed of the NPC.
    public float moveSpeed = 2f;
    // Duration for which the NPC moves away.
    public float moveAwayDuration = 5f;

    // Tags for identifying the player and disliked NPC.
    public string playerTag = "Player";
    public string dislikedNPCTag = "Nes";

    // Transforms for the disliked NPC and player.
    private Transform dislikedNPC;
    private Transform player;
    // NavMeshAgent component for navigation.
    private NavMeshAgent navMeshAgent;

    // Flag to track if the NPC is moving away.
    private bool isMovingAway = false;
    // Timer to track the move away duration.
    private float moveAwayTimer = 0f;

    // Called when the script instance is being loaded.
    private void Start()
    {
        // Get the NavMeshAgent component.
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Called once per frame.
    private void Update()
    {
        // Detect nearby entities.
        DetectNearbyEntities();
        // Handle the NPC behavior.
        HandleBehavior();
    }

    // Detects nearby entities within a radius.
    void DetectNearbyEntities()
    {
        // Find all colliders within a radius.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10f);

        // Reset detected entities.
        dislikedNPC = null;
        player = null;

        // Iterate through detected colliders.
        foreach (var hitCollider in hitColliders)
        {
            // Check if the collider has the disliked NPC tag.
            if (hitCollider.CompareTag(dislikedNPCTag))
            {
                dislikedNPC = hitCollider.transform;
            }
            // Check if the collider has the player tag.
            else if (hitCollider.CompareTag(playerTag))
            {
                player = hitCollider.transform;
            }
        }
    }

    // Handles the NPC behavior.
    void HandleBehavior()
    {
        // Move towards the disliked NPC if both player and disliked NPC are detected and not already moving away.
        if (player != null && dislikedNPC != null && !isMovingAway)
        {
            MoveTowards(dislikedNPC);
            StartCoroutine(DislikedNPCMoveAway());
        }

        // Track the move away timer.
        if (isMovingAway)
        {
            moveAwayTimer += Time.deltaTime;
            // Reset the move away flag and timer when the duration is reached.
            if (moveAwayTimer >= moveAwayDuration)
            {
                isMovingAway = false;
                moveAwayTimer = 0f;
            }
        }
    }

    // Moves the NPC towards a target.
    public void MoveTowards(Transform target)
    {
        // Set the NavMeshAgent destination.
        navMeshAgent.SetDestination(target.position);
    }

    // Coroutine to make the disliked NPC move away.
    public IEnumerator DislikedNPCMoveAway()
    {
        // Set the move away flag.
        isMovingAway = true;

        // Move the disliked NPC away if it exists.
        if (dislikedNPC != null)
        {
            // Get the NavMeshAgent component of the disliked NPC.
            NavMeshAgent dislikedAgent = dislikedNPC.GetComponent<NavMeshAgent>();

            // Move the disliked NPC away if it has a NavMeshAgent component.
            if (dislikedAgent != null)
            {
                // Calculate the direction to move away from.
                Vector3 awayDirection = (dislikedNPC.position - transform.position).normalized;
                // Calculate the target position.
                Vector3 targetPosition = dislikedNPC.position + awayDirection * 10f;

                // Set the NavMeshAgent destination for the disliked NPC.
                dislikedAgent.SetDestination(targetPosition);
            }
            // Log an error if the disliked NPC does not have a NavMeshAgent component.
            else
            {
                Debug.LogError("Disliked NPC does not have a NavMeshAgent component!");
            }
        }

        yield return null;
    }

    // Draws a wire sphere Gizmo in the editor to visualize the detection radius.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 10f);
    }
}