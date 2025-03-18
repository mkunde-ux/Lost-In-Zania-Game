using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AdilaMiziki : MonoBehaviour
{
    [Header("Trust Settings")]
    // Maximum trust level.
    public int maxTrust = 100;
    // Current trust level.
    public int currentTrust;
    // Trust threshold to alert the guard.
    public float trustThresholdToAlertGuard = 85f;
    // Interval for updating trust bar.
    public float updateInterval = 4f;

    [Header("References")]
    // Reference to the security guard AI.
    public SecurityAI securityGuard;
    // Reference to the trust bar UI.
    public AdilaTrustGuage trustBar;
    // Reference to the dialogue tree.
    public AdilaDialogueTree dialogueTree;

    [Header("Guard Settings")]
    // Detection radius for the guard.
    public float guardDetectionRadius = 10f;
    // Patrol points for the guard.
    public Transform[] guardPatrolPoints;

    // NavMeshAgent component.
    private NavMeshAgent navMeshAgent;
    // Last trust change amount.
    private int lastTrustChange = 0;

    // Called when the script starts.
    private void Start()
    {
        // Get the NavMeshAgent component.
        navMeshAgent = GetComponent<NavMeshAgent>();
        // Initialize current trust to maximum trust.
        currentTrust = maxTrust;

        // Set the maximum trust in the trust bar.
        if (trustBar != null)
        {
            trustBar.SetMaxTrust(maxTrust);
        }
    }

    // Adjusts the Miziki (trust) level.
    public void AdjustMiziki(int amount)
    {
        // Start coroutine to change trust.
        StartCoroutine(ChangeTrust(amount));
    }

    // Gets the last Miziki change amount.
    public int GetLastMizikiChange()
    {
        return lastTrustChange;
    }

    // Coroutine to change the trust level gradually.
    IEnumerator ChangeTrust(int changeAmount)
    {
        // Store the last trust change amount.
        lastTrustChange = changeAmount;

        // Calculate the number of steps and the direction of change.
        int steps = Mathf.Abs(changeAmount);
        int direction = changeAmount > 0 ? 1 : -1;

        // Change trust level in steps.
        for (int i = 0; i < steps; i++)
        {
            // Update current trust level.
            currentTrust += direction;
            currentTrust = Mathf.Clamp(currentTrust, 0, maxTrust);

            // Update the trust bar UI.
            if (trustBar != null)
            {
                trustBar.SetTrust(currentTrust);
            }

            // Check if trust level is below the threshold during dialogue.
            if (dialogueTree != null && dialogueTree.DialogueCanvas.gameObject.activeSelf)
            {
                if (currentTrust <= maxTrust * (trustThresholdToAlertGuard / 100f))
                {
                    // Call security and close dialogue if trust is too low.
                    CallSecurityAndCloseDialogue();
                    yield break;
                }
            }

            // Wait for a short duration before the next step.
            yield return new WaitForSeconds(0.1f);
        }
    }

    // Coroutine to update the trust bar periodically.
    IEnumerator TrustBarUpdate()
    {
        while (true)
        {
            // Wait for the update interval.
            yield return new WaitForSeconds(updateInterval);
        }
    }

    // Calls security and closes the dialogue.
    public void CallSecurityAndCloseDialogue()
    {
        // Display a message in the dialogue text.
        if (dialogueTree != null)
        {
            dialogueTree.dialogueText.text = "Adila: Okay, that’s it! You think you're funny? I’m calling Security!!";
            // Delay before calling the guard.
            StartCoroutine(DelayBeforeCallingGuard(2f));
        }
        else
        {
            Debug.LogError("DialogueTree reference is missing in Adila!");
        }
    }

    // Coroutine to delay before calling the guard.
    private IEnumerator DelayBeforeCallingGuard(float delayTime)
    {
        // Wait for the delay time.
        yield return new WaitForSeconds(delayTime);

        // End the dialogue.
        if (dialogueTree != null)
        {
            dialogueTree.EndDialogue();
        }

        // Call security.
        CallSecurity();
    }

    // Calls the security guard.
    public void CallSecurity()
    {
        // Check if the security guard reference is assigned.
        if (securityGuard != null)
        {
            // Get the player transform.
            Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

            // Check if player transform is found.
            if (playerTransform != null)
            {
                // Make the security guard follow the player.
                securityGuard.FollowPlayer(playerTransform);

                // Show the security guard's dialogue.
                MichaelSecurityDialogueScript michaelDialogue = securityGuard.GetComponent<MichaelSecurityDialogueScript>();
                if (michaelDialogue != null)
                {
                    michaelDialogue.ShowDialogue();
                }

                Debug.Log("Adila has alerted the security guard!");
            }
        }
    }

    // Checks the Miziki level at the end of the dialogue.
    public void CheckMizikiLevelAtEnd()
    {
        // Check if the trust level is below the threshold.
        if (currentTrust <= maxTrust * (trustThresholdToAlertGuard / 100f))
        {
            Debug.Log("Adila's trust is too low. She's not happy!");
        }
    }
}