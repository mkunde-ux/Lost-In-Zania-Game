using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class NesInteraction : MonoBehaviour
{
    [Header("References")]
    // Reference to the NesDialogue script.
    public NesDialogue nesDialogue;
    // References to the trust scripts for different characters.
    public ImaniTrust imaniTrust;
    public EmanuelTrust emanuelTrust;
    public FahariTrust fahariTrust;

    // References to the interaction and dialogue canvases.
    [SerializeField] private Canvas interactionCanvas;
    [SerializeField] private Canvas dialogueCanvas;

    [Header("Settings")]
    // Radius within which the player can interact with Nes.
    [SerializeField] private float interactionRadius = 5f;
    // Cooldown duration between interactions.
    [SerializeField] private float interactionCooldown = 1f;

    // Required trust level to interact with Nes.
    [SerializeField] private int requiredTrustLevel = 85;
    // Layer mask for the player.
    [SerializeField] private LayerMask playerLayerMask;

    // Reference to the player's transform.
    private Transform _playerTransform;
    // Flag indicating whether Nes is currently engaged in dialogue.
    private bool _isEngaged;
    // Flag indicating whether the player is within interaction range.
    private bool _playerInRange;
    // Flag indicating whether Nes can currently be interacted with.
    private bool _canInteract = true;

    // Event triggered when the dialogue is completed.
    public UnityEvent OnDialogueCompleted = new UnityEvent();

    private void Awake()
    {
        // Initialize the UI.
        InitializeUI();
    }

    private void InitializeUI()
    {
        // Hide the interaction and dialogue canvases.
        interactionCanvas.gameObject.SetActive(false);
        dialogueCanvas.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Handle player detection.
        HandlePlayerDetection();

        // Check for player interaction input.
        if (_playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // Check if Nes can be interacted with, is not engaged, and the trust level is sufficient.
            if (_canInteract && !_isEngaged && CanInteractWithNes())
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
        _playerInRange = Physics.CheckSphere(transform.position, interactionRadius, playerLayerMask);

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
                // Set the player in range flag in NesDialogue.
                if (nesDialogue != null)
                {
                    nesDialogue.SetPlayerInRange(true);
                    Debug.Log("Player entered range. SetPlayerInRange(true) called.");
                }
            }
        }
        // If the player is out of range.
        else if (wasInRange)
        {
            // Handle behavior when the player exits the range.
            HandleOutOfRangeBehavior();

            // Set the player out of range flag in NesDialogue.
            if (nesDialogue != null)
            {
                nesDialogue.SetPlayerInRange(false);
                Debug.Log("Player exited range. SetPlayerInRange(false) called.");
            }
        }
    }

    public void SetPlayerInRange(bool inRange)
    {
        // Set the player in range flag.
        _playerInRange = inRange;
        // If the player is out of range and the dialogue canvas is active, reset the dialogue system.
        if (!inRange && dialogueCanvas.gameObject.activeSelf)
        {
            ResetDialogueSystem();
        }
    }

    private void OnPlayerEnterRange()
    {
        // If Nes is not engaged and can be interacted with, show the interaction canvas.
        if (!_isEngaged && CanInteractWithNes())
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
        // Show the interaction canvas if Nes is not engaged in dialogue.
        interactionCanvas.gameObject.SetActive(!_isEngaged);
    }

    private void HandleOutOfRangeBehavior()
    {
        // Hide the interaction canvas.
        interactionCanvas.gameObject.SetActive(false);
        // If Nes is engaged in dialogue, reset the dialogue system.
        if (_isEngaged) ResetDialogueSystem();
    }

    private bool CanInteractWithNes()
    {
        // Check if any of the trust scripts are missing.
        if (imaniTrust == null || emanuelTrust == null || fahariTrust == null)
        {
            // Log an error message if any trust script is missing.
            Debug.LogError("One or more trust scripts are missing!");
            // Return false to indicate that interaction is not possible.
            return false;
        }

        // Count the number of characters with high trust levels.
        int highTrustCount = 0;
        if (imaniTrust.currentTrust >= requiredTrustLevel) highTrustCount++;
        if (emanuelTrust.currentTrust >= requiredTrustLevel) highTrustCount++;
        if (fahariTrust.currentTrust >= requiredTrustLevel) highTrustCount++;

        // Return true if at least two characters have high trust levels.
        return highTrustCount >= 2;
    }

    public void StartInteraction()
    {
        // Set the engaged flag to true.
        _isEngaged = true;
        // Toggle the UI to show the dialogue canvas.
        ToggleUI(true);
        // Unlock the cursor.
        LockCursor(false);

        // Start the dialogue using the NesDialogue script.
        nesDialogue.StartDialogue();
        // Start the interaction cooldown coroutine.
        StartCoroutine(InteractionCooldown());
    }

    private void ToggleUI(bool dialogueActive)
    {
        // Hide the interaction canvas and show the dialogue canvas based on the dialogueActive flag.
        interactionCanvas.gameObject.SetActive(false);
        dialogueCanvas.gameObject.SetActive(dialogueActive);
    }

    private void LockCursor(bool locked)
    {
        // Show or hide the cursor and lock or unlock it based on the locked flag.
        Cursor.visible = !locked;
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

        // If the NesDialogue script exists and the dialogue is not complete, reset the dialogue.
        if (nesDialogue != null && !nesDialogue.IsDialogueComplete())
        {
            nesDialogue.ResetDialogue();
            // Invoke the OnDialogueCompleted event.
            OnDialogueCompleted?.Invoke();
        }
    }

    private IEnumerator InteractionCooldown()
    {
        // Set the canInteract flag to false.
        _canInteract = false;
        // Wait for the interaction cooldown duration.
        yield return new WaitForSeconds(interactionCooldown);
        // Set the canInteract flag to true.
        _canInteract = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}