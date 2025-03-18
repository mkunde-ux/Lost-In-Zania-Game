using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SecurityInteractions : MonoBehaviour
{
    [Header("References")]
    // Reference to the security UI GameObject.
    [SerializeField] private GameObject securityUI;
    // Reference to the dialogue script for quest 1.
    [SerializeField] private MichaelDialogueQuest1 michaelDialogueQuest1;
    // Reference to the Canvas component for interaction prompts.
    [SerializeField] private Canvas interactionCanvas;
    // Reference to the PlayerInteractions script for player-related interactions.
    [SerializeField] private PlayerInteractions playerInteractions;

    [Header("Settings")]
    // Radius within which the player can interact with the security object.
    public float interactionRadius = 5f;
    // Layer mask to identify the player GameObject.
    [SerializeField] private LayerMask playerLayer;

    // Cached Transform component of the player GameObject.
    private Transform _playerTransform;
    // Flag to track if the player is within interaction range.
    private bool _playerInRange;

    // Called when the script instance is being loaded.
    private void Start()
    {
        // Ensure the security UI is initially hidden.
        if (securityUI != null)
        {
            securityUI.SetActive(false);
        }

        // Ensure the interaction canvas is initially hidden.
        if (interactionCanvas != null)
        {
            interactionCanvas.gameObject.SetActive(false);
        }
    }

    // Called once per frame.
    private void Update()
    {
        // Check if the player is within interaction range and update UI accordingly.
        HandlePlayerDetection();

        // Start dialogue when the player is in range and presses the interaction key ('E').
        if (_playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            StartDialogue();
        }
    }

    // Handles player detection and updates interaction UI based on player proximity.
    private void HandlePlayerDetection()
    {
        // Store the previous player range state for comparison.
        bool wasInRange = _playerInRange;

        // Check if the player is within the interaction radius using a physics sphere check.
        _playerInRange = Physics.CheckSphere(transform.position, interactionRadius, playerLayer);

        // If the player is now in range.
        if (_playerInRange)
        {
            // Cache the player's transform if not already cached.
            UpdatePlayerReference();
            // Handle behavior when the player enters the interaction range.
            HandleInRangeBehavior(!wasInRange); // Pass true if this is the first frame the player is in range.
        }
        // If the player just left the interaction range.
        else if (wasInRange)
        {
            // Handle behavior when the player exits the interaction range.
            HandleOutOfRangeBehavior();
        }
    }

    // Caches the player's transform for efficient access.
    private void UpdatePlayerReference()
    {
        // Find and cache the player's transform if it hasn't been cached yet.
        if (_playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player)
                _playerTransform = player.transform;
        }
    }

    // Handles behavior when the player is within the interaction range.
    private void HandleInRangeBehavior(bool firstEnter)
    {
        // Execute logic only on the first frame the player enters the range.
        if (firstEnter)
        {
            OnPlayerEnterRange();
        }

        // Enable the interaction canvas to show interaction prompts.
        if (interactionCanvas != null)
        {
            interactionCanvas.gameObject.SetActive(true);
        }
    }

    // Handles behavior when the player is out of the interaction range.
    private void HandleOutOfRangeBehavior()
    {
        // Disable the interaction canvas to hide interaction prompts.
        if (interactionCanvas != null)
        {
            interactionCanvas.gameObject.SetActive(false);
        }
    }

    // Executes logic when the player first enters the interaction range.
    private void OnPlayerEnterRange()
    {
        // Reset any necessary states when the player enters interaction range.
    }

    // Starts the dialogue interaction.
    public void StartDialogue()
    {
        // Log that the StartDialogue function has been called.
        Debug.Log("SecurityInteractions: StartDialogue called");

        // Start dialogue only if the player's popup trigger is activated and then closed.
        if (playerInteractions != null &&
            playerInteractions.popupTrigger != null &&
            playerInteractions.popupTrigger.IsMenuActivatedAndClosed())
        {
            // Start the dialogue if the dialogue script is assigned.
            if (michaelDialogueQuest1 != null)
            {
                // Start the dialogue using the dialogue script.
                michaelDialogueQuest1.StartDialogue();
                // Show the security UI.
                if (securityUI != null)
                {
                    securityUI.SetActive(true);
                }

                // Hide the interaction prompt.
                if (interactionCanvas != null)
                {
                    interactionCanvas.gameObject.SetActive(false);
                }

                // Notify the SecurityAI to stop and look at the player.
                if (TryGetComponent(out SecurityAI securityAI))
                {
                    securityAI.StoreOriginalState(); // Save current state.
                    securityAI.StopAndLookAtPlayer(playerInteractions.transform); // Pass player's transform.
                    securityAI.currentGuardState = SecurityAI.GuardState.Dialogue; // Set state to dialogue.
                }
            }
        }
    }

    // Restores the guard's original state after dialogue ends.
    public void RestoreGuardState()
    {
        // Restore the guard's original behavior once dialogue ends.
        if (TryGetComponent(out SecurityAI securityAI))
        {
            securityAI.RestoreOriginalState();
        }
        // Remove the listener to prevent multiple calls.
        michaelDialogueQuest1.onDialogueEnd.RemoveListener(RestoreGuardState);
    }

    // Draws a wire sphere in the editor to visualize the interaction radius.
    private void OnDrawGizmosSelected()
    {
        // Set the gizmo color to cyan.
        Gizmos.color = Color.cyan;
        // Draw a wire sphere at the object's position with the interaction radius.
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}