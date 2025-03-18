using System.Collections;
using System.Collections.Generic; 
using UnityEngine;
using TMPro;

public class PlayerInteractions : MonoBehaviour
{
    [Header("Settings")]
    // Interaction range for NPCs.
    [SerializeField] private float interactRange = 5f;
    // Key to interact with NPCs.
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    // Key for friendly interactions.
    [SerializeField] private KeyCode friendlyInteractKey = KeyCode.Q;
    // Key to stagger NPCs.
    [SerializeField] private KeyCode staggerKey = KeyCode.F;
    // Key to give gifts to NPCs.
    [SerializeField] private KeyCode giveGiftKey = KeyCode.G;
    // Maximum number of staggers allowed.
    [SerializeField] private int maxStaggers = 5;
    // Current number of staggers.
    private int currentStaggers = 0;

    [Header("References")]
    // Reference to the PopupTrigger script for other functions.
    [SerializeField] public PopupTrigger popupTrigger;
    // Reference to the dialogue canvas.
    [SerializeField] private GameObject dialogueCanvas;
    // Reference to the PickUpDropScript.
    [SerializeField] private PickUpDropScript pickUpDropScript;
    // Reference to the NesInteractionController.
    [SerializeField] private NesInteractionController nesInteractionController;

    // Flag to track if Imani dialogue is active.
    private bool isImaniDialogueActive = false;
    // Flag to track if Nes dialogue is active.
    private bool isNesDialogueActive = false;

    [Header("Feedback Settings")]
    // Reference to the AudioSource component.
    [SerializeField] private AudioSource audioSource;
    // Audio clip played when a gift is given.
    [SerializeField] private AudioClip giftAudioClip;
    // Audio clip played when an item is returned and destroyed.
    [SerializeField] private AudioClip itemReturnAudioClip;

    // Reference to the PopupTriggerNPC script.
    [SerializeField] private PopupTriggerNPC popupTriggerNPC;

    // Dictionary to track interaction latches for each NPC GameObject.
    private Dictionary<GameObject, bool> interactionLatches = new Dictionary<GameObject, bool>();

    private void Start()
    {
        // Disable the dialogue canvas at the start.
        if (dialogueCanvas != null)
        {
            dialogueCanvas.SetActive(false);
        }
    }

    private void Update()
    {
        // If the player is holding an item, skip regular NPC dialogue interactions.
        if (pickUpDropScript != null && pickUpDropScript.IsHoldingObject)
        {
            // (You may still allow friendly interactions, staggers, or gift giving.)
        }
        else
        {
            // Handle regular NPC interactions.
            HandleNPCInteractions();
        }

        // Handle friendly interactions.
        HandleFriendlyInteractions();
        // Handle staggering.
        HandleStagger();
        // Handle gift giving.
        HandleGiftGiving();
    }

    private void HandleNPCInteractions()
    {
        // Check for interact key press.
        if (Input.GetKeyDown(interactKey))
        {
            // Array to store colliders within the interaction range.
            Collider[] colliderArray = new Collider[10];
            // Find colliders within the interaction range.
            int collidersFound = Physics.OverlapSphereNonAlloc(transform.position, interactRange, colliderArray);

            // Iterate through found colliders.
            for (int i = 0; i < collidersFound; i++)
            {
                Collider collider = colliderArray[i];
                // Get the GameObject of the collider.
                GameObject npcObject = collider.gameObject;

                // Check for Imani dialogue component.
                if (collider.TryGetComponent(out ImaniDialogue imaniDialogue))
                {
                    // Start Imani dialogue if conditions are met.
                    if (imaniDialogue != null && !isImaniDialogueActive && !isNesDialogueActive)
                    {
                        SetLatch(npcObject, true); // Set the latch
                        imaniDialogue.SetPlayerInRange(true);
                        imaniDialogue.StartDialogue();
                        isImaniDialogueActive = true;
                        return;
                    }
                }

                // Check for Adila dialogue component.
                if (collider.TryGetComponent(out AdilaDialogueTree adilaDialogue))
                {
                    // Start Adila dialogue if conditions are met and latch is true.
                    if (adilaDialogue != null && !isImaniDialogueActive && !isNesDialogueActive)
                    {
                        if (GetLatch(npcObject)) // Check the latch
                        {
                            adilaDialogue.StartDialogue();
                            return;
                        }
                    }
                }

                // Check for Fahari interaction component.
                if (collider.TryGetComponent(out FahariInteraction fahariInteraction))
                {
                    // Start Fahari interaction.
                    SetLatch(npcObject, true);
                    fahariInteraction.StartInteractionWithPlayer(transform);
                    return;
                }

                // Check for Nes interaction component.
                if (collider.TryGetComponent(out NesInteraction nesInteraction))
                {
                    // Start Nes interaction if conditions are met.
                    if (!isNesDialogueActive && CanInteractWithNes())
                    {
                        SetLatch(npcObject, true);
                        nesInteraction.StartInteraction();
                        isNesDialogueActive = true;
                    }
                    else
                    {
                        Debug.Log("You do not meet the trust conditions to interact with Nes.");
                    }
                    return;
                }

                // Check for Security guard tag.
                if (collider.CompareTag("SecurityGuard"))
                {
                    // Start Security dialogue if conditions are met.
                    if (popupTrigger != null && popupTrigger.IsMenuActivatedAndClosed())
                    {
                        if (collider.TryGetComponent(out SecurityInteractions security))
                        {
                            if (Vector3.Distance(transform.position, security.transform.position) <= security.interactionRadius)
                            {
                                SetLatch(npcObject, true);
                                security.StartDialogue();
                                dialogueCanvas.SetActive(true);
                            }
                            else
                            {
                                Debug.Log("Player is out of security guard's interaction radius.");
                            }
                        }
                    }
                    return;
                }
            }
        }
        else
        {
            // Reset dialogue states and latches when not interacting.
            Collider[] colliderArray = new Collider[10];
            int collidersFound = Physics.OverlapSphereNonAlloc(transform.position, interactRange, colliderArray);

            for (int i = 0; i < collidersFound; i++)
            {
                Collider collider = colliderArray[i];
                GameObject npcObject = collider.gameObject;

                if (collider.TryGetComponent(out ImaniDialogue imaniDialogue))
                {
                    if (imaniDialogue != null)
                    {
                        imaniDialogue.SetPlayerInRange(false);
                        SetLatch(npcObject, false); // Reset the latch
                    }
                }

                if (collider.TryGetComponent(out AdilaDialogueTree adilaDialogue))
                {
                    adilaDialogue.SetPlayerInRange(false);
                    SetLatch(npcObject, false);
                }

                if (collider.TryGetComponent(out NesInteraction nesInteraction))
                {
                    nesInteraction.SetPlayerInRange(false);
                    SetLatch(npcObject, false);
                }
            }
        }
    }

    // Sets the latch state for a given NPC GameObject.
    private void SetLatch(GameObject npc, bool state)
    {
        if (interactionLatches.ContainsKey(npc))
        {
            interactionLatches[npc] = state;
        }
        else
        {
            interactionLatches.Add(npc, state);
        }
    }

    // Gets the latch state for a given NPC GameObject.
    private bool GetLatch(GameObject npc)
    {
        if (interactionLatches.ContainsKey(npc))
        {
            return interactionLatches[npc];
        }
        return false;
    }

    // Handles friendly interactions with NPCs.
    private void HandleFriendlyInteractions()
    {
        // Check for friendly interaction key press.
        if (Input.GetKeyDown(friendlyInteractKey))
        {
            // Array to store colliders within the interaction range.
            Collider[] colliderArray = new Collider[10];
            // Find colliders within the interaction range.
            int collidersFound = Physics.OverlapSphereNonAlloc(transform.position, interactRange, colliderArray);

            // Iterate through found colliders.
            for (int i = 0; i < collidersFound; i++)
            {
                Collider collider = colliderArray[i];

                // Check for FriendlyNPC component.
                if (collider.TryGetComponent(out FriendlyNPC friendlyNPC))
                {
                    // Interact with the friendly NPC.
                    friendlyNPC.InteractWithFriendlyNPC();
                    return;
                }
            }
        }
    }

    // Handles staggering of NPCs.
    private void HandleStagger()
    {
        // Check for stagger key press.
        if (Input.GetKeyDown(staggerKey))
        {
            // Check if there are staggers remaining.
            if (currentStaggers < maxStaggers)
            {
                // Array to store colliders within the interaction range.
                Collider[] colliderArray = new Collider[10];
                // Find colliders within the interaction range.
                int collidersFound = Physics.OverlapSphereNonAlloc(transform.position, interactRange, colliderArray);

                // Iterate through found colliders.
                for (int i = 0; i < collidersFound; i++)
                {
                    Collider collider = colliderArray[i];

                    // Check for AdilaMovement component.
                    if (collider.TryGetComponent(out AdilaMovement adila))
                    {
                        // Stagger the Adila NPC.
                        adila.Stagger();
                        currentStaggers++;
                        Debug.Log("Staggered NPC. Staggers remaining: " + (maxStaggers - currentStaggers));
                        return;
                    }
                }
            }
            else
            {
                // Log a message if no staggers are remaining.
                Debug.Log("No staggers remaining!");
            }
        }
    }

    // Handles giving gifts to NPCs.
    private void HandleGiftGiving()
    {
        // Check if the player is holding an item and the gift key is pressed.
        if (Input.GetKeyDown(giveGiftKey) && pickUpDropScript != null && pickUpDropScript.IsHoldingObject)
        {
            // Array to store colliders within the interaction range.
            Collider[] colliderArray = new Collider[10];
            // Find colliders within the interaction range.
            int collidersFound = Physics.OverlapSphereNonAlloc(transform.position, interactRange, colliderArray);
            // Iterate through found colliders.
            for (int i = 0; i < collidersFound; i++)
            {
                Collider collider = colliderArray[i];
                // Check for CharacterAI component.
                if (collider.TryGetComponent(out CharacterAI npc))
                {
                    // Give the gift to the NPC.
                    npc.ReceiveGift();

                    // Optionally, provide feedback.
                    if (popupTriggerNPC != null)
                    {
                        popupTriggerNPC.ShowPopup("Gift Delivered!");
                    }
                    if (audioSource != null && giftAudioClip != null)
                    {
                        audioSource.PlayOneShot(giftAudioClip);
                    }

                    // Retrieve the held gift object from the PickUpDropScript.
                    GameObject giftToDestroy = pickUpDropScript.HeldItem;

                    // Drop the gift (which also resets holding state).
                    pickUpDropScript.DropObject();

                    // Destroy the gift object.
                    if (giftToDestroy != null)
                    {
                        Destroy(giftToDestroy);
                    }

                    Debug.Log("Gift given to NPC!");
                    return;
                }
            }
            // Optionally, if no NPC is in range, you can display a message.
            Debug.Log("No NPC in range to receive the gift.");
        }
    }


    // Gives a gift to the specified NPC.
    public void GiveGiftToNPC(CharacterAI npc)
    {
        npc?.ReceiveGift();
    }

    // Draws a red wire sphere in the editor to visualize the interaction range.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }

    // Resets the Imani dialogue active flag.
    public void ResetImaniDialogueFlag()
    {
        isImaniDialogueActive = false;
    }

    // Resets the Nes dialogue active flag.
    public void ResetNesDialogueFlag()
    {
        isNesDialogueActive = false;
    }

    // Checks if the player can interact with Nes based on trust levels.
    private bool CanInteractWithNes()
    {
        // Check if the NesInteractionController reference is missing.
        if (nesInteractionController == null)
        {
            Debug.LogError("NesInteractionController reference is missing!");
            return false;
        }

        // Evaluate trust levels using the NesInteractionController.
        return nesInteractionController.EvaluateTrustLevels();
    }
}
