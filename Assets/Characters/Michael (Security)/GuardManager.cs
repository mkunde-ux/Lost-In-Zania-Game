using UnityEngine;
using System.Collections.Generic;

public class GuardManager : MonoBehaviour
{
    // Static variable to track if the player is being chased globally.
    public static bool playerIsBeingChased = false;
    // List to store all Guard objects managed by this manager.
    public List<Guard> guards;

    [Header("Flank Settings")]
    // Distance from the player at which guards try to position themselves when flanking.
    [Tooltip("Distance from the player at which guards try to position themselves when flanking.")]
    public float flankDistance = 5f;
    // Angle offset (in degrees) from the direct line to the player for flanking positions.
    [Tooltip("Angle offset (in degrees) from the direct line to the player for flanking positions.")]
    public float flankAngleOffset = 45f;

    [Header("Separation Settings")]
    // Minimum distance guards must maintain from each other.
    [Tooltip("Minimum distance guards must maintain from each other.")]
    public float minGuardDistance = 2f;

    [Header("Slow Effect Settings")]
    // Radius within which the player gets slowed down.
    [Tooltip("Radius within which the player gets slowed down.")]
    public float slowRadius = 10f;
    // Multiplier for reducing the player's speed (e.g., 0.5 for 50% speed).
    [Tooltip("Multiplier for reducing the player's speed (e.g., 0.5 for 50% speed).")]
    public float slowFactor = 0.5f;

    // Reference to the PlayerMovement script to apply slow effects.
    public PlayerMovement playerMovement;

    // Static method to start the global chase state.
    public static void StartGlobalChase()
    {
        playerIsBeingChased = true;
    }

    // Static method to stop the global chase state.
    public static void StopGlobalChase()
    {
        playerIsBeingChased = false;
    }

    // Checks the chase status of individual guards and updates the global chase state.
    public void CheckChaseStatus()
    {
        // Flag to track if any guard is currently chasing the player.
        bool anyGuardChasing = false;
        // Iterate through each guard in the list.
        foreach (Guard guard in guards)
        {
            // Check if the current guard is chasing the player.
            if (guard.isChasingPlayer)
            {
                // If any guard is chasing, set the flag to true and break the loop.
                anyGuardChasing = true;
                break;
            }
        }
        // If no guard is chasing the player, stop the global chase.
        if (!anyGuardChasing)
        {
            StopGlobalChase();
        }
    }

    // Update method called every frame.
    void Update()
    {
        // Check and update the global chase status.
        CheckChaseStatus();

        // If the player is being chased globally, execute flanking and slow effects.
        if (playerIsBeingChased)
        {
            FlankAndChaseGuards();
            ApplySlowEffectToPlayer();
        }
    }

    // Method to handle flanking and chasing behavior for guards.
    public void FlankAndChaseGuards()
    {
        // Iterate through each guard in the list.
        foreach (Guard guard in guards)
        {
            // Check if the guard is not already chasing and has a player target.
            if (!guard.isChasingPlayer && guard.player != null)
            {
                // Calculate the direction from the guard to the player.
                Vector3 toGuard = (guard.transform.position - guard.player.position).normalized;
                // Calculate the flanking direction using the angle offset.
                Vector3 flankDirection = Quaternion.Euler(0, flankAngleOffset, 0) * toGuard;
                // Calculate the target flanking position.
                Vector3 flankTarget = guard.player.position + flankDirection * flankDistance;

                // Iterate through other guards to apply separation logic.
                foreach (Guard otherGuard in guards)
                {
                    // Skip the current guard.
                    if (otherGuard == guard)
                        continue;

                    // Calculate the distance between the current guard and other guards.
                    float distanceBetweenGuards = Vector3.Distance(guard.transform.position, otherGuard.transform.position);
                    // If the distance is less than the minimum guard distance, apply repulsion.
                    if (distanceBetweenGuards < minGuardDistance)
                    {
                        // Calculate the repulsion direction.
                        Vector3 repulsionDir = (guard.transform.position - otherGuard.transform.position).normalized;
                        // Calculate the adjustment factor based on the distance difference.
                        float adjustmentFactor = (minGuardDistance - distanceBetweenGuards);
                        // Adjust the flanking target to maintain separation.
                        flankTarget += repulsionDir * adjustmentFactor;
                    }
                }

                // Alert the guard to the flanking target and start chasing.
                guard.SharedAlert(flankTarget);
                guard.StartChasingPlayer(guard.player);
            }
        }
    }

    // Method to apply a slow effect to the player based on nearby guards.
    public void ApplySlowEffectToPlayer()
    {
        // Check if the player movement script is assigned.
        if (playerMovement == null)
            return;

        // Count the number of guards within the slow radius.
        int nearbyGuards = 0;
        // Iterate through each guard in the list.
        foreach (Guard guard in guards)
        {
            // Check if the guard has a player target.
            if (guard.player != null)
            {
                // Calculate the distance between the guard and the player.
                float distance = Vector3.Distance(guard.transform.position, guard.player.position);
                // If the distance is within the slow radius, increment the count.
                if (distance <= slowRadius)
                {
                    nearbyGuards++;
                }
            }
        }

        // If there are guards within the slow radius, apply the slow effect.
        if (nearbyGuards > 0)
        {
            playerMovement.ApplySlow(slowFactor);
        }
        // Otherwise, remove the slow effect.
        else
        {
            playerMovement.RemoveSlow();
        }
    }

    // Method to make all guards start chasing the player.
    public void ChaseAllGuards()
    {
        // Iterate through each guard in the list.
        foreach (Guard guard in guards)
        {
            // Check if the guard is not already chasing and has a player target.
            if (!guard.isChasingPlayer && guard.player != null)
            {
                // Start chasing the player.
                guard.StartChasingPlayer(guard.player);
            }
        }
    }
}