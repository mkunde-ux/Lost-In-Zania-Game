using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ImaniTrust : MonoBehaviour
{
    [Header("Trust Settings")]
    [SerializeField] public float currentTrust = 100f;
    [SerializeField] public float maxTrust = 100f;
    [SerializeField] private float trustThresholdToAlertGuard = 85f; 
    [SerializeField] private int highTrustThreshold = 64; 
    [SerializeField] private int midTrustThresholdMin = 32; 
    [SerializeField] private int midTrustThresholdMax = 48; 

    [Header("References")]
    [SerializeField] private SecurityAI securityGuard; 
    [SerializeField] private ImaniTrustBar imaniTrustBar; 
    [SerializeField] private ImaniDialogue imaniDialogue; 
    [SerializeField] private ImaniHTDialogue imaniHTDialogue; 
    [SerializeField] private ImaniDialogue mainDialogue; 

    [Header("Behavior Settings")]
    [SerializeField] private float updateInterval = 4f; 
    [SerializeField] private float guardDetectionRadius = 10f; 
    [SerializeField] private Transform[] guardPatrolPoints; 
    [SerializeField] private float warningRadius = 15f; 

    private NavMeshAgent navMeshAgent;
    private bool isSearchingForGuard = false;
    private Transform playerTransform;
    private Transform moveAwayNPC;
    public List<MoveAwayNPC> warnedNPCs = new List<MoveAwayNPC>();
    private ImaniMovement imaniMovement;
    private int lastTrustChange = 0; 
    private float initialTrust; 

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        initialTrust = currentTrust; 

        if (midTrustThresholdMin >= midTrustThresholdMax)
        {
            Debug.LogError("midTrustThresholdMin should be less than midTrustThresholdMax!");
        }

        if (imaniTrustBar != null)
        {
            imaniTrustBar.SetMaxTrust(Mathf.RoundToInt(maxTrust));
            imaniTrustBar.SetTrust(Mathf.RoundToInt(currentTrust));
        }
        else
        {
            Debug.LogError("Trust bar reference not assigned in ImaniTrust!");
        }

        StartCoroutine(ImaniTrustBarUpdate());

        imaniMovement = GetComponent<ImaniMovement>();
        if (imaniMovement == null)
        {
            Debug.LogError("ImaniMovement component not found on ImaniTrust!");
        }
    }

    public void AdjustTrust(int amount)
    {
        if (amount == 0)
        {
            Debug.LogWarning("Attempted to adjust trust by 0. No change made.");
            return;
        }

        lastTrustChange = amount;
        StartCoroutine(ChangeTrust(amount));
    }

    private IEnumerator ChangeTrust(int changeAmount)
    {
        int steps = Mathf.Abs(changeAmount);
        int direction = changeAmount > 0 ? 1 : -1;

        for (int i = 0; i < steps; i++)
        {
            currentTrust += direction;
            currentTrust = Mathf.Clamp(currentTrust, 0f, maxTrust);

            if (imaniTrustBar != null)
            {
                imaniTrustBar.SetTrust(Mathf.RoundToInt(currentTrust));
            }
            Debug.Log($"Trust adjusted by {direction}. Current trust: {currentTrust}");
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator ImaniTrustBarUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);
        }
    }

    public void ResetTrust()
    {
        currentTrust = initialTrust; 
        if (imaniTrustBar != null)
        {
            imaniTrustBar.SetTrust(Mathf.RoundToInt(currentTrust));
        }
        Debug.Log("Imani's trust has been reset.");
    }

    public void CheckTrustLevelAtEnd()
    {
        if (currentTrust < midTrustThresholdMin)
        {
            Debug.Log("Low Trust Ending - Imani calls security!");
            CallSecurity();
        }
        else if (currentTrust < highTrustThreshold)
        {
            Debug.Log("Mid Trust Ending - Imani warns NPCs!");
            HandleMidTrustEnding();
        }
        else
        {
            Debug.Log("High Trust Ending - Imani helps!");
            HandleHighTrustEnding();
        }

        ReturnToPatrol();
    }

    private void ReturnToPatrol()
    {
        if (imaniMovement != null)
        {
            imaniMovement.EndInteraction();
        }
        else
        {
            Debug.LogError("ImaniMovement component not found on ImaniTrust!");
        }
    }

    private void HandleHighTrustEnding()
    {
        StartCoroutine(ActivateImaniHTDialogueAfterDelay(3f));
    }

    private IEnumerator ActivateImaniHTDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (imaniHTDialogue != null)
        {
            imaniHTDialogue.ActivateDialogue();
        }
        else
        {
            Debug.LogError("ImaniHTDialogue reference is missing!");
        }
    }

    private void HandleMidTrustEnding()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (playerTransform != null)
        {
            MoveAwayNPC[] moveAwayNPCs = FindObjectsOfType<MoveAwayNPC>();
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

    public void CallSecurityAndCloseDialogue()
    {
        if (imaniDialogue != null)
        {
            imaniDialogue.dialogueText.text = "Imani: \"Enough games! You’re toast—security’s coming for you!\"";
            StartCoroutine(DelayBeforeCallingGuard(2f));
        }
        else
        {
            Debug.LogError("ImaniDialogue reference is missing in ImaniTrust!");
        }
    }

    private IEnumerator DelayBeforeCallingGuard(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        if (imaniDialogue != null)
        {
            imaniDialogue.EndDialogue();
        }
        CallSecurity();
    }

    public bool IsLowTrustConditionMet()
    {
        return currentTrust < midTrustThresholdMin;
    }

    public void CallSecurity()
    {
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
        return currentTrust;
    }

    public int GetLastTrustChange()
    {
        return lastTrustChange;
    }
}