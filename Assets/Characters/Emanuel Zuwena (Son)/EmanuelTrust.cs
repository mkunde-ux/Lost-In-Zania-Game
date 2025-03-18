using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class EmanuelTrust : MonoBehaviour
{
    //Check the Fahari Trust Script for comments

    [Header("Trust Settings")]
    [SerializeField] public float currentTrust = 100f;
    [SerializeField] public float maxTrust = 100f;
    [SerializeField] private float trustThresholdToAlertGuard = 85f;
    [SerializeField] private int highTrustThreshold = 64;
    [SerializeField] private int midTrustThresholdMin = 32;
    [SerializeField] private int midTrustThresholdMax = 48;

    [Header("References")]
    [SerializeField] private SecurityAI securityGuard;
    [SerializeField] private EmanuelBar trustBar;
    [SerializeField] private EmanuelDT dialogueTree;
    [SerializeField] private EmanuelHTDialogue htDialogue;
    [SerializeField] private EmanuelDT mainDialogue;

    [Header("Behavior Settings")]
    [SerializeField] private float updateInterval = 4f;
    [SerializeField] private float guardDetectionRadius = 10f;
    [SerializeField] private Transform[] guardPatrolPoints;
    [SerializeField] private float warningRadius = 15f;

    private NavMeshAgent navMeshAgent;
    private bool isSearchingForGuard = false;
    private Transform playerTransform;
    public MoveAwayNPC[] moveAwayNPCs;
    private EmanuelMovement emanuelMovement;
    private int lastTrustChange = 0;
    private float initialTrust;

    public UnityEvent<float> OnTrustChanged = new UnityEvent<float>();

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        initialTrust = currentTrust;

        if (midTrustThresholdMin >= midTrustThresholdMax)
        {
            Debug.LogError("midTrustThresholdMin should be less than midTrustThresholdMax!");
        }

        if (trustBar != null)
        {
            trustBar.SetMaxTrust(Mathf.RoundToInt(maxTrust));
            trustBar.SetTrust(Mathf.RoundToInt(currentTrust));
        }
        else
        {
            Debug.LogError("Trust bar reference not assigned in EmanuelTrust!");
        }

        StartCoroutine(TrustBarUpdate());

        emanuelMovement = GetComponent<EmanuelMovement>();
        if (emanuelMovement == null)
        {
            Debug.LogError("EmanuelMovement component not found on EmanuelTrust!");
        }

        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
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

            if (trustBar != null)
            {
                trustBar.SetTrust(Mathf.RoundToInt(currentTrust));
            }

            Debug.Log($"Trust adjusted by {direction}. Current trust: {currentTrust}");
            OnTrustChanged?.Invoke(currentTrust);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public float GetCurrentTrust()
    {
        return currentTrust;
    }

    private IEnumerator TrustBarUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);
        }
    }

    public void ResetTrust()
    {
        currentTrust = initialTrust;
        if (trustBar != null)
        {
            trustBar.SetTrust(Mathf.RoundToInt(currentTrust));
        }
        Debug.Log("Emanuel's trust has been reset.");
    }

    public void CheckTrustLevelAtEnd()
    {
        if (currentTrust < midTrustThresholdMin)
        {
            Debug.Log("Low Trust Ending - Emanuel calls security!");
            CallSecurity();
        }
        else if (currentTrust < highTrustThreshold)
        {
            Debug.Log("Mid Trust Ending - Emanuel warns NPCs!");
            HandleMidTrustEnding();
        }
        else
        {
            Debug.Log("High Trust Ending - Emanuel helps!");
            HandleHighTrustEnding();
        }

        ReturnToPatrol();
    }

    private void ReturnToPatrol()
    {
        if (emanuelMovement != null)
        {
            emanuelMovement.EndInteraction();
        }
        else
        {
            Debug.LogError("EmanuelMovement component not found on EmanuelTrust!");
        }
    }

    private void HandleHighTrustEnding()
    {
        StartCoroutine(ActivateHTDialogueAfterDelay(3f));
    }

    private IEnumerator ActivateHTDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (htDialogue != null)
        {
            htDialogue.ActivateDialogue();
        }
        else
        {
            Debug.LogError("EmanuelHTDialogue reference is missing!");
        }
    }

    private void HandleMidTrustEnding()
    {
        Debug.Log("Mid Trust Ending: NPCs alerted to move away.");

        if (moveAwayNPCs != null && playerTransform != null)
        {
            foreach (var npc in moveAwayNPCs)
            {
                if (npc != null)
                {
                    float distanceToPlayer = Vector3.Distance(npc.transform.position, playerTransform.position);
                    if (distanceToPlayer <= warningRadius)
                    {
                        npc.MoveAwayFrom(playerTransform);
                    }
                }
                else
                {
                    Debug.LogError("MoveAwayNPC in array is null!");
                }
            }
        }
        else
        {
            Debug.LogError("MoveAwayNPCs array or Player transform is null!");
        }
    }

    public void CallSecurityAndCloseDialogue()
    {
        if (dialogueTree != null)
        {
            dialogueTree.dialogueText.text = "Emanuel: Okay, that’s it! You think you're funny? I’m calling Michael, our security guard!";
            StartCoroutine(DelayBeforeCallingGuard(2f));
        }
        else
        {
            Debug.LogError("EmanuelDT reference is missing in EmanuelTrust!");
        }
    }

    public void EndDialogue()
    {
        if (emanuelMovement != null)
        {
            emanuelMovement.EndInteraction();
        }
        CheckTrustLevelAtEnd();
    }

    private IEnumerator DelayBeforeCallingGuard(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        if (dialogueTree != null)
        {
            dialogueTree.EndDialogue();
        }
        CallSecurity();
    }

    public bool IsLowTrustConditionMet()
    {
        return currentTrust < midTrustThresholdMin;
    }

    public void CallSecurity()
    {
        if (securityGuard != null && !securityGuard.isChasingPlayer && playerTransform != null)
        {
            securityGuard.FollowPlayer(playerTransform);

            MichaelSecurityDialogueScript michaelDialogue = securityGuard.GetComponent<MichaelSecurityDialogueScript>();
            if (michaelDialogue != null)
            {
                michaelDialogue.ShowDialogue();
            }
        }
    }

    public int GetLastTrustChange()
    {
        return lastTrustChange;
    }
}