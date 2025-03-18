using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events;
using UnityEditor;

public class AdilaInteraction : MonoBehaviour
{
    [Header("References")]
    // Reference to the dialogue tree script.
    [SerializeField] private AdilaDialogueTree dialogueTree;
    // Canvas for interaction prompt.
    [SerializeField] private Canvas interactionCanvas;
    // Canvas for dialogue UI.
    [SerializeField] private Canvas dialogueCanvas;
    // Component to control Adila's head look.
    [SerializeField] private AdilaHeadLook npcHeadLookAt;
    // Transform of the player's head.
    [SerializeField] private Transform _playerHead;

    [Header("Settings")]
    // Radius within which player can interact.
    [SerializeField] private float interactionRadius = 5f;
    // Time to reset dialogue after player leaves.
    [SerializeField] private float timeToResetDialogue = 5f;
    // Height offset for head look.
    [SerializeField] private float lookAtHeightOffset = 1.7f;
    // Cooldown between interactions.
    [SerializeField] private float interactionCooldown = 1f;

    [Header("Audio Settings (Optional)")]
    // Audio source for playing sounds.
    [SerializeField] private AudioSource audioSource;
    // Audio clip for interaction sound.
    [SerializeField] private AudioClip interactionClip;

    // Transform of the player.
    private Transform _playerTransform;
    // Flag indicating if Adila is engaged in dialogue.
    private bool _isEngaged;
    // Flag indicating if the player is in range.
    private bool _playerInRange;
    // Time spent outside interaction radius.
    private float _timeOutsideRadius;
    // Time of last interaction.
    private float _lastInteractionTime;
    // Flag indicating if interaction is allowed.
    private bool _canInteract = true;

    // Reference to Adila's movement script.
    [SerializeField] private AdilaMovement adilaMovement;
    // Reference to the character AI.
    [SerializeField] private CharacterAI characterAI;

    private void Awake()
    {
        // Validate references.
        ValidateReferences();
        // Initialize UI elements.
        InitializeUI();
        // Find the player's transform.
        _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        // Add listener for dialogue end event.
        dialogueTree.onDialogueEnd.AddListener(OnDialogueEnd);
    }

    private void Update()
    {
        // Handle player detection.
        HandlePlayerDetection();
        // Update head tracking.
        UpdateHeadTracking();
        // Handle dialogue timeout.
        HandleDialogueTimeout();

        // Start interaction if player is in range and presses 'E'.
        if (_playerInRange && Input.GetKeyDown(KeyCode.E) && _canInteract && !_isEngaged && !characterAI.IsBusyWithItem)
        {
            StartInteraction();
        }
    }

    private void ValidateReferences()
    {
        // Get HeadLook component if not assigned.
        if (!npcHeadLookAt) npcHeadLookAt = GetComponent<AdilaHeadLook>();
        // Assert that all references are assigned.
        Debug.Assert(npcHeadLookAt != null, "HeadLook component is missing from NPC", this);
        Debug.Assert(dialogueTree != null, "DialogueTree reference is missing", this);
        Debug.Assert(interactionCanvas != null, "Interaction canvas reference is missing", this);
        Debug.Assert(dialogueCanvas != null, "Dialogue canvas reference is missing", this);
        if (adilaMovement == null) adilaMovement = GetComponent<AdilaMovement>();
        Debug.Assert(adilaMovement != null, "AdilaMovement reference is missing", this);
        Debug.Assert(characterAI != null, "CharacterAI reference is missing", this);
    }

    private void OnDialogueEnd()
    {
        // Reset dialogue system when dialogue ends.
        ResetDialogueSystem();
    }

    private void InitializeUI()
    {
        // Initially hide UI canvases.
        interactionCanvas.gameObject.SetActive(false);
        dialogueCanvas.gameObject.SetActive(false);
    }

    private void HandlePlayerDetection()
    {
        // Store previous in range state.
        bool wasInRange = _playerInRange;
        // Check if player is in range.
        _playerInRange = Physics.CheckSphere(transform.position, interactionRadius, LayerMask.GetMask("Player"));

        // Log player enter/exit events.
        if (_playerInRange && !wasInRange)
        {
            Debug.Log("Player entered Adila's range!");
        }
        else if (!_playerInRange && wasInRange)
        {
            Debug.Log("Player exited Adila's range!");
        }

        // Show interaction canvas if player is in range and not engaged.
        interactionCanvas.gameObject.SetActive(_playerInRange && !_isEngaged);

        // Find player transform if not already found.
        if (_playerInRange && _playerTransform == null)
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
    }

    private void ResetDialogueSystem()
    {
        // Reset dialogue state.
        _isEngaged = false;
        _timeOutsideRadius = 0f;
        // Hide UI.
        ToggleUI(false);
        // Lock cursor.
        LockCursor(true);

        // Stop head look.
        npcHeadLookAt.StopLooking();

        // End Adila's interaction.
        adilaMovement.EndInteraction();
    }

    private void UpdateHeadTracking()
    {
        // Make Adila look at player's head if in range and not engaged.
        if (_playerInRange && _playerTransform && !_isEngaged)
        {
            Vector3 lookPosition = GetPlayerHeadPosition();
            npcHeadLookAt.LookAtPosition(lookPosition, 3f);
        }
        else
        {
            // Stop head look if not in range or engaged.
            npcHeadLookAt.StopLooking();
        }
    }

    private void HandleDialogueTimeout()
    {
        // Check if Adila is engaged in dialogue and the player is out of range.
        if (_isEngaged && !_playerInRange)
        {
            // Increment the time spent outside the interaction radius.
            _timeOutsideRadius += Time.deltaTime;
            // If the time exceeds the reset dialogue time, reset the dialogue system.
            if (_timeOutsideRadius >= timeToResetDialogue)
            {
                ResetDialogueSystem();
            }
        }
    }

    private void StartInteraction()
    {
        // If AdilaMovement component exists, trigger interaction.
        if (adilaMovement != null)
        {
            adilaMovement.InteractWithPlayer(true);
        }

        // Set engaged flag to true.
        _isEngaged = true;
        // Toggle UI to show dialogue canvas.
        ToggleUI(true);
        // Hide interaction canvas.
        interactionCanvas.gameObject.SetActive(false);

        // Unlock cursor.
        LockCursor(false);

        // Play interaction sound if audio source and clip are assigned.
        if (audioSource != null && interactionClip != null)
        {
            audioSource.PlayOneShot(interactionClip);
        }

        // Enable dialogue tree and reset dialogue.
        dialogueTree.enabled = true;
        dialogueTree.ResetDialogue();
        // Make Adila look at player's head.
        npcHeadLookAt.LookAtPosition(GetPlayerHeadPosition());

        // Start interaction cooldown coroutine.
        StartCoroutine(InteractionCooldown());
    }

    public void StartInteractionWithPlayer(Transform interactorTransform)
    {
        // Start interaction with player.
        StartInteraction();
    }

    private Vector3 GetPlayerHeadPosition()
    {
        // Return player head position if available, otherwise use player transform with height offset.
        return _playerHead ? _playerHead.position : _playerTransform.position + Vector3.up * lookAtHeightOffset;
    }

    private void ToggleUI(bool dialogueActive)
    {
        // Hide interaction canvas.
        interactionCanvas.gameObject.SetActive(false);
        // Show dialogue canvas if dialogue is active.
        dialogueCanvas.gameObject.SetActive(dialogueActive);
    }

    private void LockCursor(bool locked)
    {
        // Show or hide cursor based on locked flag.
        Cursor.visible = !locked;
        // Lock or unlock cursor based on locked flag.
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private IEnumerator InteractionCooldown()
    {
        // Set canInteract flag to false.
        _canInteract = false;
        // Wait for interaction cooldown duration.
        yield return new WaitForSeconds(interactionCooldown);
        // Set canInteract flag to true only if player is in range.
        _canInteract = _playerInRange;
    }

    private void OnDrawGizmosSelected()
    {
        // Set Gizmos color to cyan.
        Gizmos.color = Color.cyan;
        // Draw wire sphere to visualize interaction radius.
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
