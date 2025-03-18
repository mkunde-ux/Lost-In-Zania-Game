using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FahariTrust : MonoBehaviour
{
    [Header("Trust Settings")]
    // Public variable to track Fahari's current trust level.
    [SerializeField] public float currentTrust = 100f;
    // Maximum possible trust level.
    [SerializeField] public float maxTrust = 100f;
    // Trust threshold to alert the security guard.
    [SerializeField] public float trustThresholdToAlertGuard = 85f;
    // Threshold for high trust interactions.
    [SerializeField] private int highTrustThreshold = 64;
    // Minimum threshold for mid trust interactions.
    [SerializeField] private int midTrustThresholdMin = 32;
    // Maximum threshold for mid trust interactions.
    [SerializeField] private int midTrustThresholdMax = 48;

    [Header("References")]
    // Reference to the security guard's AI.
    [SerializeField] private SecurityAI securityGuard;
    // Reference to the trust bar UI.
    [SerializeField] private WealthyGirldTrustBar fahariTrustBar;
    // Reference to the main Fahari dialogue.
    [SerializeField] private FahariDialogue fahariDialogue;
    // Reference to the high trust Fahari dialogue.
    [SerializeField] private FahariHTDialogue fahariHTDialogue;
    // Reference to another main Fahari dialogue.
    [SerializeField] private FahariDialogue mainDialogue;

    [Header("Behavior Settings")]
    // Interval for updating Fahari's behavior.
    [SerializeField] private float updateInterval = 4f;
    // Detection radius for the security guard.
    [SerializeField] private float guardDetectionRadius = 10f;
    // Patrol points for the security guard.
    [SerializeField] private Transform[] guardPatrolPoints;
    // Warning radius around Fahari.
    [SerializeField] private float warningRadius = 15f;

    // NavMeshAgent component for navigation.
    private NavMeshAgent navMeshAgent;
    // Flag indicating if Fahari is searching for the guard.
    private bool isSearchingForGuard = false;
    // Transform of the player.
    private Transform playerTransform;
    // Reference to Fahari's movement script.
    private FahariMovement fahariMovement;
    // Last trust change amount.
    private int lastTrustChange = 0;
    // Initial trust level.
    private float initialTrust;

    private void Start()
    {
        // Get the NavMeshAgent component.
        navMeshAgent = GetComponent<NavMeshAgent>();
        // Store the initial trust level.
        initialTrust = currentTrust;

        // Log an error if the mid trust thresholds are invalid.
        if (midTrustThresholdMin >= midTrustThresholdMax)
        {
            Debug.LogError("midTrustThresholdMin should be less than midTrustThresholdMax!");
        }

        // Initialize the trust bar UI.
        if (fahariTrustBar != null)
        {
            fahariTrustBar.SetMaxTrust(Mathf.RoundToInt(maxTrust));
            fahariTrustBar.SetTrust(Mathf.RoundToInt(currentTrust));
        }
        else
        {
            Debug.LogError("Trust bar reference not assigned in FahariTrust!");
        }

        // Start the trust bar update coroutine.
        StartCoroutine(FahariTrustBarUpdate());

        // Get the FahariMovement component.
        fahariMovement = GetComponent<FahariMovement>();
        // Log an error if the FahariMovement component is missing.
        if (fahariMovement == null)
        {
            Debug.LogError("FahariMovement component not found on FahariTrust!");
        }
    }

    public void AdjustTrust(int amount)
    {
        // Log a warning if attempting to adjust trust by 0.
        if (amount == 0)
        {
            Debug.LogWarning("Attempted to adjust trust by 0. No change made.");
            return;
        }

        // Store the last trust change amount.
        lastTrustChange = amount;
        // Start the trust change coroutine.
        StartCoroutine(ChangeTrust(amount));
    }

    private IEnumerator ChangeTrust(int changeAmount)
    {
        // Calculate the number of steps and the direction of the trust change.
        int steps = Mathf.Abs(changeAmount);
        int direction = changeAmount > 0 ? 1 : -1;

        // Gradually change the trust level.
        for (int i = 0; i < steps; i++)
        {
            currentTrust += direction;
            currentTrust = Mathf.Clamp(currentTrust, 0f, maxTrust);

            // Update the trust bar UI.
            if (fahariTrustBar != null)
            {
                fahariTrustBar.SetTrust(Mathf.RoundToInt(currentTrust));
            }

            // Log the trust adjustment.
            Debug.Log($"Trust adjusted by {direction}. Current trust: {currentTrust}");
            // Wait for a short duration.
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator FahariTrustBarUpdate()
    {
        // Continuously update Fahari's behavior.
        while (true)
        {
            // Wait for the specified update interval.
            yield return new WaitForSeconds(updateInterval);
        }
    }


    public void ResetTrust()
    {
        // Reset Fahari's trust to the initial trust level.
        currentTrust = initialTrust;
        // Update the trust bar UI.
        if (fahariTrustBar != null)
        {
            fahariTrustBar.SetTrust(Mathf.RoundToInt(currentTrust));
        }
        // Log the trust reset.
        Debug.Log("Fahari's trust has been reset.");
    }

    public void CheckTrustLevelAtEnd()
    {
        // Check Fahari's trust level and trigger the appropriate ending.
        if (currentTrust < midTrustThresholdMin)
        {
            // Low trust ending: Fahari calls security.
            Debug.Log("Low Trust Ending - Fahari calls security!");
            CallSecurity();
        }
        else if (currentTrust < highTrustThreshold)
        {
            // Mid trust ending: Fahari warns NPCs.
            Debug.Log("Mid Trust Ending - Fahari warns NPCs!");
            HandleMidTrustEnding();
        }
        else
        {
            // High trust ending: Fahari helps.
            Debug.Log("High Trust Ending - Fahari helps!");
            HandleHighTrustEnding();
        }

        // Return Fahari to patrol.
        ReturnToPatrol();
    }

    private void ReturnToPatrol()
    {
        // Return Fahari to her patrol route.
        if (fahariMovement != null)
        {
            fahariMovement.EndInteraction();
        }
        else
        {
            Debug.LogError("FahariMovement component not found on FahariTrust!");
        }
    }

    private void HandleHighTrustEnding()
    {
        // Activate the high trust dialogue after a delay.
        StartCoroutine(ActivateFahariHTDialogueAfterDelay(3f));
    }

    private IEnumerator ActivateFahariHTDialogueAfterDelay(float delay)
    {
        // Wait for the specified delay.
        yield return new WaitForSeconds(delay);
        // Activate the high trust dialogue.
        if (fahariHTDialogue != null)
        {
            fahariHTDialogue.ActivateDialogue();
        }
        else
        {
            Debug.LogError("FahariHTDialogue reference is missing!");
        }
    }

    private void HandleMidTrustEnding()
    {
        // Find the player's transform.
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Check if the player was found.
        if (playerTransform != null)
        {
            // Find all NPCs that move away from the player.
            MoveAwayNPC[] moveAwayNPCs = FindObjectsOfType<MoveAwayNPC>();
            // Iterate through the NPCs and warn them if they are within the warning radius.
            foreach (MoveAwayNPC npc in moveAwayNPCs)
            {
                float distanceToPlayer = Vector3.Distance(npc.transform.position, playerTransform.position);
                if (distanceToPlayer <= warningRadius)
                {
                    npc.MoveAwayFrom(playerTransform);
                }
            }
        }
        else
        {
            Debug.LogError("Player not found for mid trust ending!");
        }
    }

    public bool IsLowTrustConditionMet()
    {
        // Check if Fahari's trust is below the low trust threshold.
        return currentTrust < trustThresholdToAlertGuard;
    }

    public void CallSecurity()
    {
        // Call the security guard to chase the player.
        if (securityGuard != null && !securityGuard.isChasingPlayer)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform != null)
            {
                securityGuard.FollowPlayer(playerTransform);
            }
        }
        else
        {
            Debug.LogError("SecurityGuard reference is missing or already chasing player!");
        }
    }

    public float GetCurrentTrust()
    {
        // Return Fahari's current trust level.
        return currentTrust;
    }

    public int GetLastTrustChange()
    {
        // Return the last trust change amount.
        return lastTrustChange;
    }
}