using UnityEngine;
using UnityEngine.Events;

public class NesInteractionController : MonoBehaviour
{
    [Header("Trust and Dialogue References")]
    // References to the trust scripts for Imani, Emanuel, and Fahari.
    [SerializeField] private ImaniTrust imaniTrust;
    [SerializeField] private EmanuelTrust emanuelTrust;
    [SerializeField] private FahariTrust fahariTrust;

    // References to the interaction scripts for Imani, Emanuel, and Fahari.
    [SerializeField] private ImaniInteractions imaniInteractions;
    [SerializeField] private EmanuelInteraction emanuelInteractions;
    [SerializeField] private FahariInteraction fahariInteractions;

    [Header("Object and Dialogue References")]
    // Reference to the Nes GameObject.
    [SerializeField] private GameObject nesObject;
    // Reference to the MatiasDialogueQuest3 script.
    [SerializeField] private MatiasDialogueQuest3 matiasDialogue;
    // Reference to the NesInteraction script.
    [SerializeField] private NesInteraction nesInteraction;

    [Header("Settings")]
    // Required trust level for each character.
    [SerializeField] private int requiredTrustLevel = 85;
    // Required number of characters with high trust levels.
    [SerializeField] private int requiredHighTrustCount = 2;

    // Flags to track if Imani, Emanuel, and Fahari's dialogues are completed.
    private bool imaniDialogueCompleted = false;
    private bool emanuelDialogueCompleted = false;
    private bool fahariDialogueCompleted = false;
    // Flag to track if the Nes GameObject is active.
    private bool nesObjectActive = false;

    private void Start()
    {
        // Add listeners to the dialogue completion events for Imani, Emanuel, and Fahari.
        if (imaniInteractions != null) imaniInteractions.OnDialogueCompleted.AddListener(OnImaniDialogueEnd);
        if (emanuelInteractions != null) emanuelInteractions.OnDialogueCompleted.AddListener(OnEmanuelDialogueEnd);
        if (fahariInteractions != null) fahariInteractions.OnDialogueCompleted.AddListener(OnFahariDialogueEnd);

        // Disable nesObject only at runtime if it was initially enabled.
        if (nesObject != null && nesObject.activeInHierarchy)
        {
            nesObject.SetActive(false);
        }
    }

    public void OnImaniDialogueEnd()
    {
        // Check if Imani's dialogue is complete.
        if (imaniInteractions != null && imaniInteractions.IsDialogueComplete())
        {
            // Set the Imani dialogue completed flag to true.
            imaniDialogueCompleted = true;
            // Evaluate trust levels and activate Nes if necessary.
            EvaluateAndActivateNes();
        }
        else
        {
            // Log an error message if the dialogue completion event is fired but the dialogue is not complete.
            Debug.LogError("Imani dialogue completion event fired, but dialogue is not complete.");
        }
    }

    public void OnEmanuelDialogueEnd()
    {
        // Check if Emanuel's dialogue is complete.
        if (emanuelInteractions != null && emanuelInteractions.IsDialogueComplete())
        {
            // Set the Emanuel dialogue completed flag to true.
            emanuelDialogueCompleted = true;
            // Evaluate trust levels and activate Nes if necessary.
            EvaluateAndActivateNes();
        }
        else
        {
            // Log an error message if the dialogue completion event is fired but the dialogue is not complete.
            Debug.LogError("Emanuel dialogue completion event fired, but dialogue is not complete.");
        }
    }

    public void OnFahariDialogueEnd()
    {
        // Check if Fahari's dialogue is complete.
        if (fahariInteractions != null && fahariInteractions.IsDialogueComplete())
        {
            // Set the Fahari dialogue completed flag to true.
            fahariDialogueCompleted = true;
            // Evaluate trust levels and activate Nes if necessary.
            EvaluateAndActivateNes();
        }
        else
        {
            // Log an error message if the dialogue completion event is fired but the dialogue is not complete.
            Debug.LogError("Fahari dialogue completion event fired, but dialogue is not complete.");
        }
    }

    private void EvaluateAndActivateNes()
    {
        // Evaluate trust levels.
        if (EvaluateTrustLevels())
        {
            // Activate Nes GameObject if trust levels are sufficient.
            SetNesObjectActive();
        }
        else
        {
            // Start Matias's dialogue if trust levels are not sufficient.
            if (matiasDialogue != null)
            {
                matiasDialogue.StartDialogue();
            }
            else
            {
                // Log an error message if the MatiasDialogue reference is missing.
                Debug.LogError("MatiasDialogue reference is missing.");
            }
        }
    }


    public bool EvaluateTrustLevels()
    {
        // Initialize a counter for NPCs with high trust.
        int npcsWithHighTrust = 0;

        // Check Imani's trust level.
        if (imaniTrust != null)
        {
            // Increment the counter if Imani's trust is high enough.
            if (imaniTrust.GetCurrentTrust() >= requiredTrustLevel) npcsWithHighTrust++;
            // Log Imani's current trust level.
            Debug.Log("Imani Trust: " + imaniTrust.GetCurrentTrust());
        }
        else
        {
            // Log an error if the ImaniTrust reference is missing.
            Debug.LogError("ImaniTrust reference is missing!");
        }

        // Check Emanuel's trust level.
        if (emanuelTrust != null)
        {
            // Increment the counter if Emanuel's trust is high enough.
            if (emanuelTrust.GetCurrentTrust() >= requiredTrustLevel) npcsWithHighTrust++;
            // Log Emanuel's current trust level.
            Debug.Log("Emanuel Trust: " + emanuelTrust.GetCurrentTrust());
        }
        else
        {
            // Log an error if the EmanuelTrust reference is missing.
            Debug.LogError("EmanuelTrust reference is missing!");
        }

        // Check Fahari's trust level.
        if (fahariTrust != null)
        {
            // Increment the counter if Fahari's trust is high enough.
            if (fahariTrust.GetCurrentTrust() >= requiredTrustLevel) npcsWithHighTrust++;
            // Log Fahari's current trust level.
            Debug.Log("Fahari Trust: " + fahariTrust.GetCurrentTrust());
        }
        else
        {
            // Log an error if the FahariTrust reference is missing.
            Debug.LogError("FahariTrust reference is missing!");
        }

        // Log the total number of NPCs with high trust.
        Debug.Log("NPCs with high trust: " + npcsWithHighTrust);
        // Return true if the number of NPCs with high trust meets the requirement.
        return npcsWithHighTrust >= requiredHighTrustCount;
    }

    private void SetNesObjectActive()
    {
        // Check if the Nes GameObject reference is valid.
        if (nesObject != null)
        {
            // Activate the Nes GameObject.
            nesObject.SetActive(true);
            // Set the Nes GameObject active flag to true.
            nesObjectActive = true;
            // Start the Nes interaction if the NesInteraction reference is valid.
            if (nesInteraction != null) nesInteraction.StartInteraction();
        }
        else
        {
            // Log an error if the Nes GameObject reference is missing.
            Debug.LogError("NesObject reference is missing!");
        }
    }
}