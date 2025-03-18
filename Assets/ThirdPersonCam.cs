using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;

    [Header("Lock-On Settings")]
    public LayerMask targetLayer; // The layer that the NPCs are on
    public float lockOnRadius = 10f; // Adjustable radius for lock-on
    public float rotationSpeed = 5f; // Speed for rotating towards the target
    public List<Transform> lockOnTargets; // List of potential NPC targets for lock-on

    public Transform currentTarget { get; private set; } // The current locked-on target
    public bool isLockedOn { get; private set; } // To check if the lock-on is active

    // Add this flag to detect if player is interacting with UI
    public static bool isInteractingWithUI = false; // Set this flag from dialogue scripts

    private void Start()
    {
        // Locks the cursor to the center of the screen and hides it.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Prevent camera controls if interacting with UI
        if (isInteractingWithUI)
        {
            // Ensure the cursor is unlocked for UI interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return; // Do nothing else while interacting with the UI
        }

        if (isLockedOn && currentTarget != null)
        {
            // Look at the target smoothly
            Vector3 directionToTarget = currentTarget.position - playerObj.position;
            directionToTarget.y = 0; // Keep the y-axis level
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            playerObj.rotation = Quaternion.Slerp(playerObj.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // Regular camera and player rotation
            Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
            orientation.forward = viewDir.normalized;

            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if (inputDir != Vector3.zero)
            {
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
            }
        }
    }


    private Transform GetClosestTarget()
    {
        // Finds and returns the closest target within the lock-on radius and in line of sight.
        Transform closestTarget = null; // Initialize the closest target to null.
        float shortestDistance = Mathf.Infinity; // Initialize the shortest distance to infinity.

        foreach (Transform target in lockOnTargets) // Iterate through each potential target in the list.
        {
            float distanceToTarget = Vector3.Distance(player.position, target.position); // Calculate the distance between the player and the current target.

            if (distanceToTarget <= lockOnRadius && IsTargetInLineOfSight(target)) // Check if the target is within the lock-on radius and in line of sight.
            {
                if (distanceToTarget < shortestDistance) // Check if the current target is closer than the current shortest distance.
                {
                    closestTarget = target; // If it's closer, update the closest target.
                    shortestDistance = distanceToTarget; // Update the shortest distance.
                }
            }
        }

        return closestTarget; // Return the closest target found.
    }

    private bool IsTargetInLineOfSight(Transform target)
    {
        // Checks if the target is in line of sight by performing a raycast.
        Vector3 directionToTarget = target.position - player.position;
        Ray ray = new Ray(player.position, directionToTarget);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, lockOnRadius))
        {
            if (hit.transform == target)
                return true; // Target is in line of sight
        }

        return false;
    }

    public void LockOnTarget(Transform target)
    {
        // Locks onto the specified target.
        currentTarget = target;
        isLockedOn = true;
    }

    public void UnlockTarget()
    {
        // Unlocks the current target.
        currentTarget = null;
        isLockedOn = false;
    }
    private void OnDrawGizmosSelected()
    {
        // Draws gizmos in the editor to visualize the lock-on radius and line of sight.
        // Draw the lock-on radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.position, lockOnRadius);

        // Draw line of sight to current target
        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(player.position, currentTarget.position);
        }
    }
}