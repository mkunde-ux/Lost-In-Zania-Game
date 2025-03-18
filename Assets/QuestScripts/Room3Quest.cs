using UnityEngine;

public class Room3Quest : MonoBehaviour
{
    // Reference to the dialogue system script.
    [Header("Dialogue System")]
    public MichaelDialogueQuest1 dialogueSystem;

    // Reference to the Room 3 door GameObject.
    [Header("Door Interaction")]
    public GameObject door3;

    // Collider that triggers when the player enters Room 3.
    [Header("Room 3 Trigger")]
    public Collider room3Trigger;

    // Reference to the timer UI script.
    [Header("Timer UI")]
    public Room3QuestTimerUI timerUI;

    // Reference to the security guard AI script.
    [Header("Security Guard")]
    public SecurityAI securityGuard;

    // Flag to track if the dialogue has been completed.
    private bool dialogueCompleted = false;
    // Stores the player's dialogue choice (1 or 2).
    private int dialogueChoice = 0;
    // Flag to track if the player is currently in Room 3.
    private bool isPlayerInRoom3 = false;

    // Called when the script instance is being loaded.
    void Start()
    {
        // Initially disable interaction with the Room 3 door.
        if (door3 != null)
        {
            door3.GetComponent<InteractableDoor>().SetInteractable(false);
        }

        // Add a listener to the dialogue system's dialogue end event.
        if (dialogueSystem != null)
        {
            dialogueSystem.onDialogueEnd.AddListener(OnDialogueCompleted);
        }
    }

    // Called when the dialogue is completed.
    private void OnDialogueCompleted()
    {
        // Set the dialogue completed flag and store the player's choice.
        dialogueCompleted = true;
        dialogueChoice = dialogueSystem.GetSelectedOption();

        // Enable interaction with the Room 3 door.
        if (door3 != null)
        {
            door3.GetComponent<InteractableDoor>().SetInteractable(true);
        }
    }

    // Called when another collider enters the trigger.
    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering collider is the player and the dialogue is completed.
        if (other.CompareTag("Player") && dialogueCompleted)
        {
            // Set the player in Room 3 flag.
            isPlayerInRoom3 = true;

            // Determine the timer duration based on the player's dialogue choice.
            float timerDuration = dialogueChoice == 1 ? 240f : 120f;

            // Start the timer with the calculated duration.
            if (timerUI != null)
            {
                timerUI.StartTimer(timerDuration);
            }
            // Log an error if the timer UI is not assigned.
            else
            {
                Debug.LogError("Timer UI is not assigned!");
            }
        }
    }

    // Called when another collider exits the trigger.
    private void OnTriggerExit(Collider other)
    {
        // Check if the exiting collider is the player.
        if (other.CompareTag("Player"))
        {
            // Reset the player in Room 3 flag.
            isPlayerInRoom3 = false;
        }
    }

    // Called when the timer completes.
    public void OnTimerComplete()
    {
        // Check if the player is in Room 3 when the timer completes.
        if (isPlayerInRoom3)
        {
            // Alert the security guard if the player is in Room 3.
            if (securityGuard != null)
            {
                securityGuard.StartChasingPlayer(GameObject.FindGameObjectWithTag("Player").transform);
                Debug.Log("Security guard alerted! Player is in Room 3.");
            }
        }
        // Log a message if the player is not in Room 3 when the timer completes.
        else
        {
            Debug.Log("Timer completed, but player is not in Room 3.");
        }
    }
}