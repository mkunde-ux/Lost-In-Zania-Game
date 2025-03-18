using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events;

public class FahariInteraction : MonoBehaviour
{
    [Header("References")]
    // Reference to the FahariDialogue script.
    [SerializeField] private FahariDialogue fahariDialogue;
    // References to the interaction and dialogue canvases.
    [SerializeField] private Canvas interactionCanvas;
    [SerializeField] private Canvas dialogueCanvas;
    // Reference to the HeadLook script for NPC head tracking.
    [SerializeField] private HeadLook npcHeadLookAt;
    // Reference to the player's head transform.
    [SerializeField] private Transform _playerHead;

    [Header("Settings")]
    // Radius within which the player can interact with Fahari.
    [SerializeField] private float interactionRadius = 5f;
    // Cooldown duration between interactions.
    [SerializeField] private float interactionCooldown = 1f;

    // Reference to the player's transform.
    private Transform _playerTransform;
    // Flag indicating whether Fahari is currently engaged in dialogue.
    private bool _isEngaged;
    // Flag indicating whether the player is within interaction range.
    private bool _playerInRange;
    // Flag indicating whether Fahari can currently be interacted with.
    private bool _canInteract = true;

    // Event triggered when the dialogue is completed.
    public UnityEvent OnDialogueCompleted = new UnityEvent();

    // Reference to the FahariMovement script.
    [SerializeField] private FahariMovement fahariMovement;

    private void Awake()
    {
        // Validate and initialize references.
        ValidateReferences();
        // Initialize the UI.
        InitializeUI();
        // Get the FahariMovement component.
        fahariMovement = GetComponent<FahariMovement>();

        // Log an error if the FahariMovement component is missing.
        if (fahariMovement == null)
        {
            Debug.LogError("fahariMovement component not found on NPC!");
        }
    }

    private void ValidateReferences()
    {
        // Get the HeadLook component if it's missing.
        if (!npcHeadLookAt) npcHeadLookAt = GetComponent<HeadLook>();
        // Assert that the HeadLook component is present.
        Debug.Assert(npcHeadLookAt != null, "HeadLook component is missing from NPC", this);
        // Assert that all other references are present.
        Debug.Assert(fahariDialogue != null, "fahariDialogue reference is missing", this);
        Debug.Assert(interactionCanvas != null, "Interaction canvas reference is missing", this);
        Debug.Assert(dialogueCanvas != null, "Dialogue canvas reference is missing", this);
    }

    private void InitializeUI()
    {
        // Hide the interaction and dialogue canvases.
        interactionCanvas.gameObject.SetActive(false);
        dialogueCanvas.gameObject.SetActive(false);
    }

    // This method is triggered when the player first interacts with Fahari.
    public void StartInteractionWithPlayer(Transform playerTransform)
    {
        // Ensure Fahari stops moving immediately during the interaction.
        if (fahariMovement != null)
        {
            fahariMovement.InteractWithPlayer(true);
        }

        // Log the start of the interaction.
        Debug.Log("Starting interaction with player: " + playerTransform.name);

        // Start the dialogue using the FahariDialogue script.
        if (fahariDialogue != null)
        {
            fahariDialogue.StartDialogue();
        }

        // Make Fahari look at the player's head.
        if (npcHeadLookAt != null && playerTransform != null)
        {
            npcHeadLookAt.LookAtPosition(playerTransform.position + Vector3.up * 1.7f); // Look at the player's head
        }

        // Set the engaged flag to true, toggle the UI, and unlock the cursor.
        _isEngaged = true;
        ToggleUI(true);
        LockCursor(false);
    }

    private void Update()
    {
        // Handle player detection.
        HandlePlayerDetection();
        // Update head tracking.
        UpdateHeadTracking();

        // Check for player interaction input.
        if (_playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // Check if Fahari can be interacted with and is not engaged.
            if (_canInteract && !_isEngaged)
            {
                // Start the interaction.
                StartInteraction();
            }
        }
    }

    private void HandlePlayerDetection()
    {
        // Store the previous player in range state.
        bool wasInRange = _playerInRange;
        // Check if the player is within interaction range.
        _playerInRange = Physics.CheckSphere(transform.position, interactionRadius, LayerMask.GetMask("Player"));

        // If the player is in range.
        if (_playerInRange)
        {
            // Update the player's transform reference.
            UpdatePlayerReference();
            // Handle behavior when the player enters the range.
            HandleInRangeBehavior(!wasInRange);

            // If the player just entered the range.
            if (!wasInRange)
            {
                // Set the player in range flag in FahariDialogue.
                if (fahariDialogue != null)
                {
                    fahariDialogue.SetPlayerInRange(true);
                    Debug.Log("Player entered range. SetPlayerInRange(true) called.");
                }
            }
        }
        // If the player is out of range.
        else if (wasInRange)
        {
            // Handle behavior when the player exits the range.
            HandleOutOfRangeBehavior();

            // Set the player out of range flag in FahariDialogue.
            if (fahariDialogue != null)
            {
                fahariDialogue.SetPlayerInRange(false);
                Debug.Log("Player exited range. SetPlayerInRange(false) called.");
            }
        }
    }


    public bool IsDialogueComplete()
    {
        // Check if the FahariDialogue reference is missing.
        if (fahariDialogue == null)
        {
            // Log an error message if the reference is missing.
            Debug.LogError("FahariDialogue reference is missing!");
            // Return false to indicate that the dialogue is not complete.
            return false;
        }
        // Return the dialogue completion status from the FahariDialogue script.
        return fahariDialogue.IsDialogueComplete();
    }

    private void OnPlayerEnterRange()
    {
        // If Fahari is not engaged in dialogue, show the interaction canvas.
        if (!_isEngaged)
        {
            interactionCanvas.gameObject.SetActive(true);
        }
    }

    private void UpdatePlayerReference()
    {
        // Check if the player transform reference is null.
        if (_playerTransform == null)
        {
            // Find the player GameObject using its tag.
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            // If the player GameObject is found, store its transform.
            if (player != null)
            {
                _playerTransform = player.transform;
            }
        }
    }

    private void HandleInRangeBehavior(bool firstEnter)
    {
        // If it's the first time the player enters the range, trigger the OnPlayerEnterRange event.
        if (firstEnter) OnPlayerEnterRange();
        // Show the interaction canvas if Fahari is not engaged in dialogue.
        interactionCanvas.gameObject.SetActive(!_isEngaged);
    }

    private void HandleOutOfRangeBehavior()
    {
        // Hide the interaction canvas.
        interactionCanvas.gameObject.SetActive(false);
        // If Fahari is engaged in dialogue, reset the dialogue system.
        if (_isEngaged) ResetDialogueSystem();
    }

    private void UpdateHeadTracking()
    {
        // If the player is in range, the player transform is not null, and Fahari is not engaged in dialogue,
        // make Fahari look at the player's head.
        if (_playerInRange && _playerTransform != null && !_isEngaged)
        {
            Vector3 lookPosition = GetPlayerHeadPosition();
            npcHeadLookAt.LookAtPosition(lookPosition);
        }
        // If the player is out of range, stop Fahari from looking at the player.
        else if (!_playerInRange)
        {
            npcHeadLookAt.StopLooking();
        }
    }

    // Called when the player presses the interact key from within FahariInteraction's own context.
    private void StartInteraction()
    {
        // Set the engaged flag to true.
        _isEngaged = true;
        // Toggle the UI to show the dialogue canvas.
        ToggleUI(true);
        // Unlock the cursor.
        LockCursor(false);

        // Start the dialogue using the FahariDialogue script.
        fahariDialogue.StartDialogue();
        // Make Fahari look at the player's head.
        npcHeadLookAt.LookAtPosition(GetPlayerHeadPosition());
        // Start the interaction cooldown coroutine.
        StartCoroutine(InteractionCooldown());

        // If the FahariMovement script exists, trigger the interaction.
        if (fahariMovement != null)
        {
            fahariMovement.InteractWithPlayer(true);
        }
    }

    private Vector3 GetPlayerHeadPosition()
    {
        // If the player head transform is not null, return its position.
        if (_playerHead != null)
        {
            return _playerHead.position;
        }
        // If the player head transform is null, log a warning and return the player's position with a default head height.
        else
        {
            Debug.LogWarning("Player head reference is missing. Using default head height.");
            return _playerTransform.position + Vector3.up * 1.7f;
        }
    }

    private void ToggleUI(bool dialogueActive)
    {
        // Show the interaction canvas if dialogue is not active, and hide it otherwise.
        interactionCanvas.gameObject.SetActive(!dialogueActive);
        // Show the dialogue canvas if dialogue is active, and hide it otherwise.
        dialogueCanvas.gameObject.SetActive(dialogueActive);
    }

    private void LockCursor(bool locked)
    {
        // Show or hide the cursor based on the locked flag.
        Cursor.visible = !locked;
        // Lock or unlock the cursor based on the locked flag.
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private void ResetDialogueSystem()
    {
        // Set the engaged flag to false.
        _isEngaged = false;
        // Toggle the UI to hide the dialogue canvas.
        ToggleUI(false);
        // Lock the cursor.
        LockCursor(true);
        // Stop Fahari from looking at the player.
        npcHeadLookAt.StopLooking();

        // If the FahariMovement script exists, trigger the end of the interaction.
        if (fahariMovement != null)
        {
            fahariMovement.InteractWithPlayer(false);
            fahariMovement.EndInteraction();
        }

        // If the dialogue is complete, invoke the OnDialogueCompleted event.
        if (fahariDialogue.IsDialogueComplete())
        {
            OnDialogueCompleted?.Invoke();
        }
        // If the dialogue is not complete, reset it.
        else
        {
            fahariDialogue.ResetDialogue();
        }
    }

    private IEnumerator InteractionCooldown()
    {
        // Set the canInteract flag to false.
        _canInteract = false;
        // Wait for the interaction cooldown duration.
        yield return new WaitForSeconds(interactionCooldown);
        // Set the canInteract flag to true only if the player is still in range.
        _canInteract = _playerInRange;
    }

    private void OnDrawGizmosSelected()
    {
        // Set the Gizmos color to cyan.
        Gizmos.color = Color.cyan;
        // Draw a wire sphere to visualize the interaction radius.
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}