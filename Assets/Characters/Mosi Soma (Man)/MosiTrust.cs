using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MosiTrust : MonoBehaviour
{
    //Check Adila Miziki script for comments

    [Header("Trust Settings")]
    public int maxTrust = 100;
    public int currentTrust;
    public float trustThresholdToAlertGuard = 85f;      
    public float updateInterval = 4f;  

    [Header("References")]
    public SecurityAI securityGuard;      
    public MosiTrustGuage trustBar;      
    public MosiDialogue dialogueTree;  

    [Header("Guard Settings")]
    public float guardDetectionRadius = 10f;      
    public Transform[] guardPatrolPoints;  
    private NavMeshAgent navMeshAgent;
    private int lastTrustChange = 0;  

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        currentTrust = maxTrust;
        
        if (trustBar != null)
        {
            trustBar.SetMaxTrust(maxTrust);
        }
        else
        {
            Debug.LogError("Trust bar reference not assigned in Dalila!");
        }                
    }
    
    public void AdjustTrust(int amount)
    {
        AdjustMiziki(amount);
    }
     
    public void AdjustMiziki(int amount)
    {
        StartCoroutine(ChangeTrust(amount));
    }
         
    public int GetLastMizikiChange()
    {
        return lastTrustChange;
    }
     
    public int GetLastTrustChange()
    {
        return GetLastMizikiChange();
    }

         
    IEnumerator ChangeTrust(int changeAmount)
    {
        lastTrustChange = changeAmount;  
        int steps = Mathf.Abs(changeAmount);
        int direction = changeAmount > 0 ? 1 : -1;

        for (int i = 0; i < steps; i++)
        {
            currentTrust += direction;
            currentTrust = Mathf.Clamp(currentTrust, 0, maxTrust);
             
            if (trustBar != null)
            {
                trustBar.SetTrust(currentTrust);
            }
                                        
            if (dialogueTree != null && dialogueTree.DialogueCanvas.gameObject.activeSelf)
            {
                if (currentTrust <= maxTrust * (trustThresholdToAlertGuard / 100f))
                {
                    CallSecurityAndCloseDialogue();
                    yield break;                  
                }
            }

            yield return new WaitForSeconds(0.1f);          
        }
    }
   
    IEnumerator TrustBarUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);             
        }
    }
         
    public void CallSecurityAndCloseDialogue()
    {
        if (dialogueTree != null)
        {               
            dialogueTree.dialogueText.text = "Dalila: Okay, that’s it! You think you're funny? I’m calling Security!!";                    
            StartCoroutine(DelayBeforeCallingGuard(2f));          
        }
        else
        {
            Debug.LogError("DialogueTree reference is missing in Dalila!");
        }
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
       
    public void CallSecurity()
    {
        if (securityGuard != null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                Transform playerTransform = playerObject.transform;                      
                securityGuard.FollowPlayer(playerTransform);
                  
                MichaelSecurityDialogueScript michaelDialogue = securityGuard.GetComponent<MichaelSecurityDialogueScript>();
                if (michaelDialogue != null)
                {
                    michaelDialogue.ShowDialogue();
                }
                Debug.Log("Dalila has alerted the security guard!");
            }
            else
            {
                Debug.LogWarning("Player object not found!");
            }
        }
        else
        {
            Debug.LogWarning("Security guard reference not set for Dalila!");
        }
    }
      
    public void CheckMizikiLevelAtEnd()
    {
        if (currentTrust <= maxTrust * (trustThresholdToAlertGuard / 100f))
        {    
            Debug.Log("Dalila's trust is too low. She's not happy!");          
        }
        else
        {
            Debug.Log("Dalila's trust is acceptable. Dialogue ended peacefully.");
        }
    }
         
    public void CheckTrustLevelAtEnd()
    {
        CheckMizikiLevelAtEnd();
    }
}