using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LikedNPCBar : MonoBehaviour
{
    // Maximum trust value for the NPC.
    public int maxTrust = 100;
    // Current trust value of the NPC.
    public int currentTrust;

    // Reference to the LikedNPCSliderBar script for UI updates.
    public LikedNPCSliderBar trustBar;

    // Interval for trust updates.
    public float updateInterval = 4f;
    // Amount of trust to change per update.
    public int trustChangeAmount = 3;

    // Called when the script instance is being loaded.
    private void Start()
    {
        // Initialize current trust to max trust.
        currentTrust = maxTrust;
        // Set the maximum trust value in the UI.
        trustBar.SetMaxTrust(maxTrust);
        // Start the coroutine for periodic trust updates.
        StartCoroutine(TrustBarUpdate());
    }

    // Called once per frame.
    private void Update()
    {
        // Debugging input to gain trust.
        if (Input.GetKeyDown(KeyCode.T))
        {
            GainTrust(trustChangeAmount);
        }
    }

    // Reduces trust by a specified amount.
    public void LoseTrust(int amount)
    {
        StartCoroutine(ChangeTrust(-amount));
    }

    // Increases trust by a specified amount.
    public void GainTrust(int amount)
    {
        StartCoroutine(ChangeTrust(amount));
    }

    // Coroutine to smoothly change the trust value.
    IEnumerator ChangeTrust(int changeAmount)
    {
        // Calculate the number of steps and direction of change.
        int steps = Mathf.Abs(changeAmount);
        int direction = changeAmount > 0 ? 1 : -1;

        // Iterate through the steps and update trust value.
        for (int i = 0; i < steps; i++)
        {
            currentTrust += direction;
            currentTrust = Mathf.Clamp(currentTrust, 0, maxTrust);
            trustBar.SetTrust(currentTrust);
            yield return new WaitForSeconds(0.1f);
        }
    }

    // Coroutine for periodic trust updates.
    IEnumerator TrustBarUpdate()
    {
        while (true)
        {
            // Wait for the update interval.
            yield return new WaitForSeconds(updateInterval);
        }
    }
}