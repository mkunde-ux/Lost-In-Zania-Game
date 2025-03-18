using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendlyNPC : MonoBehaviour
{
    // Settings for player interaction.
    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public KeyCode interactionKey = KeyCode.Q;

    // References to nearby guards and player memory tracker.
    [Header("References")]
    public List<Guard> nearbyGuards;

    public PlayerMemoryTracker memoryTracker;
    private Transform player;
    private bool isProtectingPlayer = false;
    private bool protectionUsed = false;

    // Called when the script instance is being loaded.
    void Start()
    {
        // Set the player transform reference.
        SetPlayerReference();
    }

    // Called once per frame.
    void Update()
    {
        // Check for player interaction.
        CheckPlayerInteraction();
    }

    // Sets the player transform reference.
    private void SetPlayerReference()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player object not found!");
        }
    }

    // Checks for player interaction within the interaction range.
    private void CheckPlayerInteraction()
    {
        if (player == null) return;

        if (Vector3.Distance(player.position, transform.position) <= interactionRange && !isProtectingPlayer)
        {
            if (Input.GetKeyDown(interactionKey))
            {
                InteractWithFriendlyNPC();
            }
        }
    }

    // Handles interaction with the friendly NPC.
    public void InteractWithFriendlyNPC()
    {
        // Check if protection has already been used.
        if (protectionUsed)
        {
            Debug.Log("Protection already used!");
            return;
        }

        // Check if protection should be ignored based on player memory.
        if (memoryTracker != null && memoryTracker.ShouldIgnoreFriendlyNPCProtection())
        {
            Debug.Log("Protection no longer works on guards! Guards will continue to chase the player.");
            return;
        }

        // Grant protection and reset guard memory.
        Debug.Log("Interacted with Friendly NPC, Protection Granted!");
        isProtectingPlayer = true;
        protectionUsed = true;

        ProtectPlayer();
        StartCoroutine(ResetProtection());
    }

    // Resets guard memory and stops chasing.
    private void ProtectPlayer()
    {
        foreach (var guard in nearbyGuards)
        {
            if (guard == null || guard.player != player) continue;

            guard.ResumePatrolling();
            Debug.Log("Guard memory reset by friendly NPC.");

            guard.StopChasingPlayer();
        }

        if (memoryTracker != null)
        {
            memoryTracker.RegisterChase();
        }
    }

    // Resets protection after a delay.
    private IEnumerator ResetProtection()
    {
        yield return new WaitForSeconds(30f);
        isProtectingPlayer = false;
        protectionUsed = false;
        Debug.Log("Protection reset, NPC ready for interaction again.");
    }

    // Draws a wire sphere Gizmo in the editor to visualize the interaction range.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}