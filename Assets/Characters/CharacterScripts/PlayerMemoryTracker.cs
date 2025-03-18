using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlayerMemoryTracker : MonoBehaviour
{
    // Counter for the number of chase events.
    public int chaseCount = 0;
    // Maximum number of chase events before ignoring friendly NPC protection.
    public int maxChaseCount = 4;
    // Time in seconds before resetting the chase count.
    public float resetTimer = 300f;

    // Flag to determine if friendly NPC protection should be ignored.
    private bool ignoreFriendlyNPCProtection = false;
    // Timer to track reset time.
    private float timer = 0f;

    // This method is called whenever a chase is initiated.
    public void RegisterChase()
    {
        // Increment the chase count.
        chaseCount++;
        // Log the current chase count.
        Debug.Log("Player has been chased " + chaseCount + " times.");

        // Check if the chase count has reached the limit.
        if (chaseCount >= maxChaseCount)
        {
            // Set the flag to ignore friendly NPC protection.
            ignoreFriendlyNPCProtection = true;
            // Log that friendly NPC protection is now ignored.
            Debug.Log("Guards will now ignore friendly NPC protection.");
        }

        // Reset the timer.
        timer = resetTimer;
    }

    // This method returns whether to ignore friendly NPC protection.
    public bool ShouldIgnoreFriendlyNPCProtection()
    {
        // Return the current state of the ignoreFriendlyNPCProtection flag.
        return ignoreFriendlyNPCProtection;
    }

    void Update()
    {
        // Check if friendly NPC protection is being ignored.
        if (ignoreFriendlyNPCProtection)
        {
            // Decrement the timer.
            timer -= Time.deltaTime;
            // Check if the timer has reached zero.
            if (timer <= 0f)
            {
                // Reset the chase count.
                ResetChaseCount();
            }
        }
    }

    // Method to reset the chase count and protection status.
    public void ResetChaseCount()
    {
        // Reset the chase count to zero.
        chaseCount = 0;
        // Restore friendly NPC protection.
        ignoreFriendlyNPCProtection = false;
        // Log that the chase count has been reset and protection restored.
        Debug.Log("Chase count reset. Friendly NPC protection restored.");
    }
}