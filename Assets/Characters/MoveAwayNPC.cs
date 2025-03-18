using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MoveAwayNPC : MonoBehaviour
{
    // Distance the NPC moves away from the target.
    public float moveAwayDistance = 10f;
    // Duration the NPC moves away before stopping.
    public float moveAwayDuration = 5f;

    // Reference to the NavMeshAgent component.
    private NavMeshAgent navMeshAgent;
    // Flag indicating if the NPC is currently moving away.
    private bool isMovingAway = false;

    private void Start()
    {
        // Get the NavMeshAgent component attached to this GameObject.
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Method to make the NPC move away from a specified target.
    public void MoveAwayFrom(Transform target)
    {
        // If the NPC is already moving away, exit the method.
        if (isMovingAway) return;

        // Set the isMovingAway flag to true before starting the coroutine.
        isMovingAway = true;

        // Calculate the direction away from the target.
        Vector3 direction = (transform.position - target.position).normalized;
        // Calculate the target position to move to.
        Vector3 targetPosition = transform.position + direction * moveAwayDistance;
        // Set the NavMeshAgent's destination to the calculated target position.
        navMeshAgent.SetDestination(targetPosition);

        // Start the coroutine to handle the move away duration.
        StartCoroutine(DislikedNPCMoveAway());
    }

    // Coroutine to handle the move away duration.
    private IEnumerator DislikedNPCMoveAway()
    {
        // Wait for the specified move away duration.
        yield return new WaitForSeconds(moveAwayDuration);
        // Set the isMovingAway flag to false.
        isMovingAway = false;
        // Reset the NavMeshAgent's path to stop moving.
        navMeshAgent.ResetPath();
    }
}