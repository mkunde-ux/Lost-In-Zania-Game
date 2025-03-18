using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Room3HintItem : MonoBehaviour
{
    // Radius within which the player can interact with the hint item.
    public float radius = 5f;
    // Tag used to identify the player GameObject.
    public string playerTag = "Player";
    // GameObject representing the in-world popup to indicate interactability.
    public GameObject inWorldPopup;
    // GameObject representing the canvas menu UI for the hint item.
    public GameObject canvasMenuUI;
    // Button used to close the canvas menu UI.
    public Button closeButton;
    // Reference to the HintDialogue2 script for dialogue after closing the UI.
    public HintDialogue2 hintDialogue;

    // Reference to the QuestDirection script for advancing quest stages.
    public QuestDirection questDirection;

    // Transform of the player GameObject.
    private Transform playerTransform;
    // Flag to track if the canvas menu UI is active.
    private bool isMenuUIActive = false;

    // Reference to the Room3QuestTimerUI script.
    private Room3QuestTimerUI timerUI;
    // Reference to the Room3Quest script.
    private Room3Quest room3Quest;

    // Called when the script instance is being loaded.
    void Start()
    {
        // Find the player GameObject by tag and get its transform.
        playerTransform = GameObject.FindGameObjectWithTag(playerTag)?.transform;
        // Log an error if the player is not found.
        if (playerTransform == null)
            Debug.LogError("Player not found!");

        // Disable the in-world popup initially.
        inWorldPopup?.SetActive(false);
        // Disable the canvas menu UI initially.
        canvasMenuUI?.SetActive(false);
        // Add a listener to the close button to call the CloseMenuUI method.
        closeButton?.onClick.AddListener(CloseMenuUI);

        // Find the Room3QuestTimerUI component in the scene.
        timerUI = FindObjectOfType<Room3QuestTimerUI>();
        // Find the Room3Quest component in the scene.
        room3Quest = FindObjectOfType<Room3Quest>();

        // Log an error if the Room3QuestTimerUI component is not found.
        if (timerUI == null)
            Debug.LogError("Room3HintItem: TimerUI not found!");

        // Log an error if the Room3Quest component is not found.
        if (room3Quest == null)
            Debug.LogError("Room3HintItem: Room3Quest not found!");

        // Log a warning if the GameObject is not tagged as 'Room3item'.
        if (gameObject.tag != "Room3item")
            Debug.LogWarning("Room3HintItem: GameObject is not tagged as 'Room3item'.");
    }

    // Called once per frame.
    void Update()
    {
        // Return if the player transform is not assigned.
        if (playerTransform == null)
            return;

        // Calculate the distance between the player and the hint item.
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        // Determine if the player is within interaction range.
        bool inRange = distance <= radius;

        // Enable or disable the in-world popup based on player proximity and menu UI state.
        inWorldPopup?.SetActive(inRange && !isMenuUIActive);

        // Activate the menu UI if the player is in range and presses the 'R' key.
        if (inRange && Input.GetKeyDown(KeyCode.R) && !isMenuUIActive)
        {
            ActivateMenuUI();
        }
        // Pause or resume the game based on the menu UI state.
        Time.timeScale = isMenuUIActive ? 0f : 1f;
    }

    // Returns the current state of the menu UI.
    public bool IsMenuUIActive()
    {
        return isMenuUIActive;
    }

    // Activates the canvas menu UI.
    void ActivateMenuUI()
    {
        // Set the menu UI active flag and enable the canvas menu UI.
        isMenuUIActive = true;
        canvasMenuUI?.SetActive(true);
        // Show and unlock the cursor.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        // Disable the in-world popup.
        inWorldPopup?.SetActive(false);
    }

    // Closes the canvas menu UI.
    public void CloseMenuUI()
    {
        // Set the menu UI active flag to false and disable the canvas menu UI.
        isMenuUIActive = false;
        canvasMenuUI?.SetActive(false);
        // Hide and lock the cursor.
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Advance the quest stage if the QuestDirection script is assigned.
        if (questDirection != null)
        {
            questDirection.AdvanceQuestStage();
        }
        // Log an error if the QuestDirection script is missing.
        else
        {
            Debug.LogError("QuestDirection reference missing!");
        }

        // Start the hint dialogue after a delay.
        StartCoroutine(StartHintDialogueAfterDelay());
    }

    // Coroutine to start the hint dialogue after a delay.
    private IEnumerator StartHintDialogueAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        // Start the hint dialogue if the HintDialogue2 script is assigned.
        if (hintDialogue != null)
        {
            hintDialogue.StartDialogue();
        }
        // Log an error if the HintDialogue2 script is missing.
        else
        {
            Debug.LogError("HintDialogue reference missing!");
        }
        // Start the hint dialogue using the component on the same GameObject.
        if (GetComponent<HintDialogue2>() != null)
        {
            GetComponent<HintDialogue2>().StartDialogue();
        }
        // Log an error if the HintDialogue2 component is missing.
        else
        {
            Debug.LogError("HintDialogue2 missing!");
        }
    }

    // Called when another collider enters the trigger.
    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering collider is the player.
        if (other.CompareTag("Player"))
        {
            // Call the ItemFound method.
            ItemFound();
        }
    }

    // Stops the timer when the player finds the hint item.
    private void ItemFound()
    {
        // Stop the timer if the Room3QuestTimerUI script is assigned.
        if (timerUI != null)
        {
            timerUI.PublicStopTimer();
            Debug.Log("Room3HintItem: Player found the item! Timer stopped.");
        }
        // Log an error if the Room3QuestTimerUI script is missing.
        else
        {
            Debug.LogError("Room3HintItem: TimerUI is null, cannot stop timer.");
        }
    }

    // Draws a wire sphere Gizmo in the editor to visualize the interaction radius.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}