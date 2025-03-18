using UnityEngine;
using System.Collections;

public class CallNPC : MonoBehaviour
{
    public ThirdPersonCam thirdPersonCam; // Reference to the ThirdPersonCam script
    public float stopDistance = 2f; // Distance at which the NPC should stop from the player
    public float moveSpeed = 3f; // Speed at which the NPC moves toward the player

    private bool isCallingNPC = false; // Flag to check if NPC is being called
    private Transform calledNPC; // The NPC that is currently called

    private void Update()
    {
        HandleCallNPC();
    }

    private void HandleCallNPC()
    {
        // Check if the "C" key is pressed and lock-on is active with a valid target
        if (Input.GetKeyDown(KeyCode.C) && thirdPersonCam.isLockedOn && thirdPersonCam.currentTarget != null)
        {
            calledNPC = thirdPersonCam.currentTarget;
            isCallingNPC = true;
        }

        // If an NPC is being called, move it towards the player
        if (isCallingNPC && calledNPC != null)
        {
            MoveNPCTowardsPlayer();
        }
    }

    private void MoveNPCTowardsPlayer()
    {
        // Get the direction from NPC to the player
        Vector3 directionToPlayer = (thirdPersonCam.player.position - calledNPC.position).normalized;

        // Calculate the new position for the NPC, stopping at the specified distance
        float distanceToPlayer = Vector3.Distance(thirdPersonCam.player.position, calledNPC.position);
        if (distanceToPlayer > stopDistance)
        {
            // Move the NPC towards the player
            calledNPC.position += directionToPlayer * moveSpeed * Time.deltaTime;
        }
        else
        {
            // NPC has reached the desired distance
            isCallingNPC = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a sphere around the player to indicate the stopping distance for the called NPC
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(thirdPersonCam.player.position, stopDistance);
    }
}