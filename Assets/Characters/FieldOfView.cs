using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldOfViewNPC : MonoBehaviour
{
    // Radius of the NPC's view.
    public float viewRadius = 10f;
    // Angle of the NPC's view.
    [Range(0, 360)]
    public float viewAngle = 90f;

    // LayerMask for targets the NPC can see.
    public LayerMask targetMask;
    // LayerMask for obstacles that block the NPC's view.
    public LayerMask obstacleMask;

    // Transform of the player.
    public Transform player;

    // Flag indicating whether the NPC can see the player.
    public bool canSeePlayer;

    private void Start()
    {
        // Find the player GameObject using its tag.
        player = GameObject.FindGameObjectWithTag("Player").transform;
        // Start the coroutine to find targets with a delay.
        StartCoroutine(FindTargetsWithDelay(0.2f));
    }

    private IEnumerator FindTargetsWithDelay(float delay)
    {
        // Continuously find targets with the specified delay.
        while (true)
        {
            // Wait for the specified delay.
            yield return new WaitForSeconds(delay);
            // Find visible targets.
            FindVisibleTargets();
        }
    }

    private void FindVisibleTargets()
    {
        // Reset the canSeePlayer flag before checking.
        canSeePlayer = false;

        // Find all colliders within the view radius that are on the target layer.
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        // Iterate through the found targets.
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            // Get the transform of the current target.
            Transform target = targetsInViewRadius[i].transform;
            // Calculate the direction from the NPC to the target.
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            // Check if the target is within the view angle.
            if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2)
            {
                // Calculate the distance to the target.
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                // Check if there are any obstacles blocking the view to the target.
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    // Set the canSeePlayer flag to true if the player is visible.
                    canSeePlayer = true;
                    // Exit the loop once the player is found.
                    break;
                }
            }
        }
    }

    // Calculate the direction from an angle.
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        // Adjust the angle if it's not global.
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        // Calculate the direction vector.
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}