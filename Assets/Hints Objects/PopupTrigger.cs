using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PopupTrigger : MonoBehaviour
{
    public float radius = 5f;
    public string playerTag = "Player";
    public GameObject inWorldPopup;
    public GameObject canvasMenuUI;
    public Button closeButton;
    public HintDialogue1 hintDialogue;
    public MichaelDialogueQuest1 michaelDialogueQuest1;
    public Canvas interactionCanvas;
    public QuestDirection questDirection;

    private Transform playerTransform;
    private bool isMenuUIActive = false;
    private bool menuUIActivated = false;

    void Start()
    {
        // Find the player GameObject by tag and get its transform.
        playerTransform = GameObject.FindGameObjectWithTag(playerTag)?.transform;
        // Log an error if the player is not found.
        if (playerTransform == null) Debug.LogError("Player not found!");

        inWorldPopup?.SetActive(false);
        canvasMenuUI?.SetActive(false);
        // Add a listener to the close button to call the CloseMenuUI method.
        closeButton?.onClick.AddListener(CloseMenuUI);

        // Disable the Michael dialogue quest script initially.
        if (michaelDialogueQuest1 != null)
        {
            michaelDialogueQuest1.enabled = false;
        }

        // Disable the interaction canvas initially.
        if (interactionCanvas != null)
        {
            interactionCanvas.gameObject.SetActive(false);
        }
        // Initialize the menuUIActivated flag to false.
        menuUIActivated = false; //This is the added line.
    }

    void Update()
    {
        // Return if the player transform is null.
        if (playerTransform == null) return;

        // Calculate the distance between the player and the popup.
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        // Check if the player is within the interaction radius.
        bool inRange = distance <= radius;

        // Enable the in-world popup if the player is in range and the menu UI is not active.
        inWorldPopup?.SetActive(inRange && !isMenuUIActive);

        // Activate the menu UI if the player is in range, presses the 'R' key, and the menu UI is not active.
        if (inRange && Input.GetKeyDown(KeyCode.R) && !isMenuUIActive)
        {
            ActivateMenuUI();
        }

        //Time.timeScale = isMenuUIActive ? 0f : 1f;

        // Enable the Michael dialogue quest script if the menu UI has been activated and is not currently active.
        if (michaelDialogueQuest1 != null)
        {
            michaelDialogueQuest1.enabled = menuUIActivated && !isMenuUIActive;
        }

        // Enable the interaction canvas if the player is in range, the menu UI is not active, and the menu UI has been activated.
        if (interactionCanvas != null)
        {
            interactionCanvas.gameObject.SetActive(inRange && !isMenuUIActive && menuUIActivated);
        }
    }

    void ActivateMenuUI()
    {
        isMenuUIActive = true;
        menuUIActivated = true;
        canvasMenuUI?.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        // Disable the in-world popup.
        inWorldPopup?.SetActive(false);

        // Disable the interaction canvas.
        if (interactionCanvas != null)
        {
            interactionCanvas.gameObject.SetActive(false);
        }
    }

    public void CloseMenuUI()
    {
        // Set the menu UI active flag to false.
        isMenuUIActive = false;
        // Disable the canvas menu UI.
        canvasMenuUI?.SetActive(false);
        // Make the cursor invisible.
        Cursor.visible = false;
     
        Cursor.lockState = CursorLockMode.Locked;
        // Start a coroutine to start the hint dialogue after a delay.
        StartCoroutine(StartHintDialogueAfterDelay());

        // Enable the Michael dialogue quest script.
        if (michaelDialogueQuest1 != null)
        {
            michaelDialogueQuest1.enabled = true;
        }

        // Update the quest progress when the popup is closed.
        if (questDirection != null)
        {
            questDirection.AdvanceQuestStage();
        }
        else
        {
            Debug.LogError("QuestDirection reference missing!");
        }
    }

    private IEnumerator StartHintDialogueAfterDelay()
    {
        // Wait for 5 seconds.
        yield return new WaitForSeconds(5f);
        // Start the hint dialogue if the hint dialogue script is not null.
        if (hintDialogue != null)
        {
            hintDialogue.StartDialogue();
        }
        else
        {
            Debug.LogError("HintDialogue reference missing!");
        }
    }

    // Method to check if the menu has been activated and closed
    public bool IsMenuActivatedAndClosed()
    {
        // Return true if the menu UI has been activated and is not currently active.
        return menuUIActivated && !isMenuUIActive;
    }
    void OnDrawGizmosSelected()
    {
        // Set the gizmo color to yellow.
        Gizmos.color = Color.yellow;
        // Draw a wire sphere to visualize the interaction radius.
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}