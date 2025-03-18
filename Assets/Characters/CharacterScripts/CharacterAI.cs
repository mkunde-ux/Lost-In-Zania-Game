using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

// Serializable class to hold NPC trust UI elements.
[System.Serializable]
public class NPCTrustUI
{
    // Name of the NPC.
    public string npcName;
    // Slider representing the NPC's trust level.
    public Slider trustBar;
}

// Serializable class to hold NPC hold positions.
[System.Serializable]
public class NPCHoldPosition
{
    // Name of the NPC.
    public string npcName;
    // Transform representing the hold position.
    public Transform holdPosition;
}

public class CharacterAI : MonoBehaviour
{
    [Header("Trust System")]
    // Current trust level of the NPC.
    public float currentTrust = 50f;
    // Maximum trust level.
    public float maxTrust = 100f;
    // Minimum trust level.
    public float minTrust = 0f;
    // List of NPC trust UI elements.
    public List<NPCTrustUI> npcTrustUIList;
    // Name of the NPC.
    public string npcName;

    [Header("Detection Settings")]
    // Radius within which the NPC detects the player.
    public float detectionRadius = 10f;
    // Velocity threshold for determining if the player is running.
    public float runningVelocityThreshold = 5f;
    // Rate at which trust is lost.
    public float trustLossRate = 1f;
    // Rate at which trust is gained.
    public float trustGainRate = 5f;

    [Header("Item Interaction")]
    // List of NPC hold positions.
    public List<NPCHoldPosition> npcHoldPositions;
    // Delay before the NPC picks up an item.
    public float itemPickupDelay = 6f;
    // Tag of pickable items.
    public string pickableItemTag = "PickableItem";
    // Tag of return triggers.
    public string returnTriggerTag = "ReturnTrigger";
    // Tag of gift items.
    public string giftTag = "Gift";
    // Reference to the PickUpDropScript.
    private PickUpDropScript pickUpDropScript;

    [Header("Behavior Settings")]
    // Distance the NPC avoids the player at low trust.
    public float lowTrustAvoidDistance = 5f;
    // Distance the NPC follows the player at high trust.
    public float highTrustFollowDistance = 3f;
    // Trust threshold for calling security.
    public float securityCallThreshold = 20f;

    [Header("Gift Settings")]
    // Amount of trust gained from a gift.
    public float giftTrustIncrease = 10f;
    // Cooldown time for gifts.
    public float giftCooldown = 10f;
    // Time of the last gift given.
    private float lastGiftTime = -10f;
    // Flag indicating if a gift has been given.
    private bool giftGiven = false;

    [Header("Chase Settings")]
    // Speed at which the NPC runs when chasing.
    public float runSpeed = 7f;

    [Header("Feedback Settings")]
    // Reference to the PopupTriggerNPC script.
    public PopupTriggerNPC popupTrigger;
    // Audio source for playing sounds.
    public AudioSource audioSource;
    // Audio clip for item return.
    public AudioClip itemReturnAudioClip;
    // Audio clip for gift giving.
    public AudioClip giftAudioClip;

    // Reference to the NavMeshAgent component.
    private NavMeshAgent agent;
    // Transform of the player.
    private Transform player;
    // Rigidbody of the player.
    private Rigidbody playerRb;
    // Currently held item.
    private GameObject currentItem = null;
    // Flag indicating if the NPC is holding an item.
    private bool isHoldingItem = false;
    // Flag indicating if the NPC is returning an item.
    private bool isReturningItem = false;
    // Flag indicating if the NPC is investigating.
    private bool isInvestigating = false;

    // Task-based flags
    // Flag indicating if the player is running.
    private bool isPlayerRunning = false;
    // Flag indicating if the player is returning an item.
    private bool isPlayerReturningItem = false;

    [Header("Player Action Memory")]
    // Size of the player action memory.
    public int memorySize = 5;
    // List of player actions.
    private List<PlayerAction> playerActions = new List<PlayerAction>();

    // Flag indicating if the NPC is searching.
    public bool IsSearching { get; private set; } = false;
    // Flag indicating if the NPC is alerted.
    public bool IsAlerted { get; private set; } = false;

    // Reference to the SecurityMovement script.
    private SecurityMovement securityMovement;

    // Initializes the CharacterAI with NavMeshAgent and player transform.
    public void Initialize(NavMeshAgent navAgent, Transform playerTransform)
    {
        agent = navAgent;
        player = playerTransform;
        playerRb = player.GetComponent<Rigidbody>();
    }

    // Called when the script starts.
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerRb = player.GetComponent<Rigidbody>();
        securityMovement = GetComponent<SecurityMovement>();
        pickUpDropScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PickUpDropScript>();

        // Initialize trust UI elements.
        if (npcTrustUIList != null)
        {
            foreach (NPCTrustUI trustUI in npcTrustUIList)
            {
                if (trustUI.npcName == npcName && trustUI.trustBar != null)
                {
                    trustUI.trustBar.maxValue = maxTrust;
                    trustUI.trustBar.value = currentTrust;
                    break;
                }
            }
        }
        else
        {
            Debug.LogError("No Trust UI List assigned!");
        }

        // Update the trust level.
        UpdateTrustLevel();
    }

    // Enum to define different player actions.
    private enum PlayerAction
    {
        Running,
        PickingUpItem,
        GivingGift
    }

    // Called every frame.
    void Update()
    {
        // Update the trust level.
        UpdateTrustLevel();
        // Adjust the NPC's behavior based on the trust level.
        AdjustBehaviorBasedOnTrust();
        // Detect and handle items in the environment.
        DetectAndHandleItems();
    }

    // Updates the trust level and UI.
    private void UpdateTrustLevel()
    {
        // Clamp the current trust level within the min and max trust values.
        currentTrust = Mathf.Clamp(currentTrust, minTrust, maxTrust);
        // Update the trust bar UI for the corresponding NPC.
        foreach (NPCTrustUI trustUI in npcTrustUIList)
        {
            if (trustUI.npcName == npcName && trustUI.trustBar != null)
            {
                trustUI.trustBar.value = currentTrust;
                break;
            }
        }
    }

    // Detects and handles items in the environment.
    public void DetectAndHandleItems()
    {
        // Check if the NPC is not already holding or returning an item.
        if (!isHoldingItem && !isReturningItem)
        {
            // Find all colliders within the detection radius.
            Collider[] items = Physics.OverlapSphere(transform.position, detectionRadius);
            // Iterate through the found colliders.
            foreach (Collider item in items)
            {
                // Check if the item is a pickable item.
                if (item.CompareTag(pickableItemTag))
                {
                    // Start coroutine to pick up the item after a delay.
                    StartCoroutine(PickUpItemAfterDelay(item.gameObject));
                    break;
                }
                // Check if the item is a gift and the gift cooldown has expired.
                else if (item.CompareTag(giftTag) && Time.time > lastGiftTime + giftCooldown)
                {
                    Debug.Log("Gift detected: " + item.name);
                    // Handle the detected gift.
                    HandleGiftDetection(item.gameObject);
                    break;
                }
            }
        }
    }

    // Starts an investigation at the given position.
    public void StartInvestigation(Vector3 position)
    {
        // Set the agent's destination to the given position.
        agent.SetDestination(position);
        // Set the searching flag to true.
        IsSearching = true;
        // Set the investigating flag to true.
        isInvestigating = true;
    }

    // Called when the NPC receives a gift.
    public void ReceiveGift()
    {
        // Increase the trust level.
        currentTrust += giftTrustIncrease;
        // Clamp the trust level within the min and max trust values.
        currentTrust = Mathf.Clamp(currentTrust, minTrust, maxTrust);
        // Update the last gift time.
        lastGiftTime = Time.time;
        Debug.Log("NPC received a gift! Trust increased to: " + currentTrust);
    }

    // Called when a collider enters the trigger.
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider has the return trigger tag.
        if (other.CompareTag(returnTriggerTag))
        {
            // Check if the NPC is holding an item.
            if (isHoldingItem)
            {
                // Store the item to destroy.
                GameObject itemToDestroy = currentItem;
                // Drop the item.
                DropItem();
                // Decrease the trust level.
                currentTrust -= 10f;

                // Show popup message.
                if (popupTrigger != null)
                {
                    popupTrigger.ShowPopup("Item Returned!");
                }
                // Play item return audio.
                if (audioSource != null && itemReturnAudioClip != null)
                {
                    audioSource.PlayOneShot(itemReturnAudioClip);
                }
                // Destroy the item.
                Destroy(itemToDestroy);
                // Return to patrol.
                ReturnToPatrol();
            }
            // Check if the player is returning an item.
            else if (isPlayerReturningItem)
            {
                // Increase the trust level.
                currentTrust += trustGainRate;
                // Reset the player returning item flag.
                isPlayerReturningItem = false;
                // Show popup message.
                if (popupTrigger != null)
                {
                    popupTrigger.ShowPopup("Item Returned!");
                }
                // Play item return audio.
                if (audioSource != null && itemReturnAudioClip != null)
                {
                    audioSource.PlayOneShot(itemReturnAudioClip);
                }
            }
        }
    }

    // Handles the detection of a gift.
    private void HandleGiftDetection(GameObject gift)
    {
        Debug.Log("Handling gift detection: " + gift.name);

        // Check if the gift object is not null.
        if (gift != null)
        {
            // Increase the trust level.
            currentTrust += giftTrustIncrease;
            // Clamp the trust level within the min and max trust values.
            currentTrust = Mathf.Clamp(currentTrust, minTrust, maxTrust);
            // Update the last gift time.
            lastGiftTime = Time.time;

            Debug.Log("Destroying gift: " + gift.name);
            // Destroy the gift object.
            Destroy(gift);
            Debug.Log("Gift destroyed.");

            // Show popup message.
            if (popupTrigger != null)
            {
                popupTrigger.ShowPopup("Gift Received!");
            }

            // Play gift audio.
            if (audioSource != null && giftAudioClip != null)
            {
                audioSource.PlayOneShot(giftAudioClip);
            }

            Debug.Log("NPC received a gift! Trust increased to: " + currentTrust);
        }
        else
        {
            Debug.LogWarning("Gift object is null, cannot process.");
        }
    }

    // Called when a collider exits the trigger.
    private void OnTriggerExit(Collider other)
    {
        // Check if the collider has the player tag.
        if (other.CompareTag("Player"))
        {
            // Reset the player running flag.
            isPlayerRunning = false;
        }
    }

    // Coroutine to decrease trust over time while the player is running.
    private IEnumerator DecreaseTrustOverTime()
    {
        while (isPlayerRunning)
        {
            // Decrease the trust level.
            currentTrust -= trustLossRate * Time.deltaTime;
            yield return null;
        }
    }

    // Property to check if the NPC is busy with an item (holding or returning).
    public bool IsBusyWithItem
    {
        get { return isHoldingItem || isReturningItem; }
    }

    // Coroutine to pick up an item after a delay.
    public IEnumerator PickUpItemAfterDelay(GameObject item)
    {
        // Set the returning item flag to true.
        isReturningItem = true;
        // Wait for the item pickup delay.
        yield return new WaitForSeconds(itemPickupDelay);

        // Check if the item is null (destroyed or removed).
        if (item == null)
        {
            Debug.Log("Item is null, pickup cancelled.");
            // Reset the returning item flag and exit the coroutine.
            isReturningItem = false;
            yield break;
        }

        // Calculate the distance to the item.
        float distanceToItem = Vector3.Distance(transform.position, item.transform.position);
        // Check if the item is out of detection radius.
        if (distanceToItem > detectionRadius)
        {
            Debug.Log("Item is out of detection radius (" + distanceToItem + " > " + detectionRadius + "). Pickup cancelled.");
            // Reset the returning item flag and exit the coroutine.
            isReturningItem = false;
            yield break;
        }

        // Set the current item.
        currentItem = item;
        // Initialize hold position to null.
        Transform holdPosition = null;
        // Find the hold position for the current NPC.
        foreach (NPCHoldPosition npcHold in npcHoldPositions)
        {
            if (npcHold.npcName == npcName)
            {
                holdPosition = npcHold.holdPosition;
                break;
            }
        }

        // Check if a hold position was found.
        if (holdPosition != null)
        {
            // Set the item as a child of the hold position.
            currentItem.transform.SetParent(holdPosition);
            // Reset the item's local position to zero.
            currentItem.transform.localPosition = Vector3.zero;
            // Get the item's rigidbody component.
            Rigidbody rb = currentItem.GetComponent<Rigidbody>();
            // If the item has a rigidbody, set it to kinematic.
            if (rb != null)
            {
                rb.isKinematic = true;
            }
            // Set the holding item flag to true.
            isHoldingItem = true;
            // Find the return zone object.
            GameObject returnZone = GameObject.FindGameObjectWithTag(returnTriggerTag);
            // If the return zone is found, set the agent's destination to it.
            if (returnZone != null)
            {
                agent.SetDestination(returnZone.transform.position);
            }
            else
            {
                Debug.LogError("Return trigger not found!");
            }
        }
        else
        {
            Debug.LogError("Hold position not found for NPC: " + npcName);
        }
    }

    // Coroutine to reset the gift cooldown.
    private IEnumerator ResetGiftCooldown()
    {
        // Wait for the gift cooldown time.
        yield return new WaitForSeconds(giftCooldown);
        // Reset the gift given flag.
        giftGiven = false;
    }

    // Method to drop the currently held item.
    private void DropItem()
    {
        // Check if there is a current item.
        if (currentItem != null)
        {
            // Unparent the item.
            currentItem.transform.SetParent(null);
            // Set the item's rigidbody to non-kinematic.
            currentItem.GetComponent<Rigidbody>().isKinematic = false;
            // Reset the current item.
            currentItem = null;
            // Reset the holding item and returning item flags.
            isHoldingItem = false;
            isReturningItem = false;
        }
    }

    // Method to return the NPC to its patrol route.
    private void ReturnToPatrol()
    {
        Debug.Log("Returning to patrol via AdilaMovement...");

        // Check if the NPC has the AdilaMovement component and call its ReturnToPatrol method.
        AdilaMovement adilaMovement = GetComponent<AdilaMovement>();
        if (adilaMovement != null)
        {
            adilaMovement.ReturnToPatrol();
        }
        else
        {
            Debug.LogWarning("AdilaMovement component not found on NPC.");
        }

        // Check if the NPC has the LucaMovement component and call its ReturnToPatrol method.
        LucaMovement lucaMovement = GetComponent<LucaMovement>();
        if (lucaMovement != null)
        {
            lucaMovement.ReturnToPatrol();
        }

        // Check if the NPC has the DalilaMovement component and call its ReturnToPatrol method.
        DalilaMovement dalilaMovement = GetComponent<DalilaMovement>();
        if (dalilaMovement != null)
        {
            dalilaMovement.ReturnToPatrol();
        }

        // Check if the NPC has the MosiMovement component and call its ReturnToPatrol method.
        MosiMovement mosiMovement = GetComponent<MosiMovement>();
        if (mosiMovement != null)
        {
            mosiMovement.ReturnToPatrol();
        }

        // Check if the NPC has the GodfreyMovement component and call its ReturnToPatrol method.
        GodfreyMovement godfreyMovement = GetComponent<GodfreyMovement>();
        if (godfreyMovement != null)
        {
            godfreyMovement.ReturnToPatrol();
        }
    }

    // Method to adjust the NPC's behavior based on the trust level.
    private void AdjustBehaviorBasedOnTrust()
    {
        // If the trust level is below the security call threshold, call security.
        if (currentTrust <= securityCallThreshold)
        {
            CallSecurity();
        }
        // If the trust level is below 50, avoid the player.
        else if (currentTrust < 50f)
        {
            AvoidPlayer();
        }
        // If the trust level is 75 or above, follow the player.
        else if (currentTrust >= 75f)
        {
            FollowPlayer();
        }
        // Otherwise, use default behavior (patrol or idle).
        else
        {
            // Default behavior (e.g., patrol or idle)
        }
    }

    // Method to make the NPC avoid the player.
    private void AvoidPlayer()
    {
        // Check if the player is within detection radius.
        if (Vector3.Distance(transform.position, player.position) <= detectionRadius)
        {
            // Calculate the direction away from the player.
            Vector3 directionAwayFromPlayer = transform.position - player.position;
            // Calculate the avoid destination.
            Vector3 avoidDestination = transform.position + directionAwayFromPlayer.normalized * lowTrustAvoidDistance;
            // Set the agent's destination to the avoid destination.
            agent.SetDestination(avoidDestination);
        }
        // If the player is outside the detection radius, reset the path and return to patrol.
        else
        {
            agent.ResetPath();
            ReturnToPatrol();
        }
    }

    // Method to make the NPC follow the player.
    private void FollowPlayer()
    {
        // Calculate the follow destination.
        Vector3 followDestination = player.position - (player.forward * highTrustFollowDistance);
        // Set the agent's destination to the follow destination.
        agent.SetDestination(followDestination);
    }

    // Method to call security and chase the player.
    private void CallSecurity()
    {
        Debug.Log("Trust is too low! Calling security and chasing the player...");
        ChasePlayer();
    }

    // Method to make the NPC chase the player.
    private void ChasePlayer()
    {
        // Disable the SecurityMovement component if present.
        SecurityMovement patrol = GetComponent<SecurityMovement>();
        if (patrol != null)
        {
            patrol.enabled = false;
        }
        // Set the agent's speed to the run speed.
        agent.speed = runSpeed;
        // Set the agent's destination to the player's position.
        if (player != null)
        {
            agent.SetDestination(player.position);
        }
    }

    // Method to draw gizmos in the editor.
    private void OnDrawGizmosSelected()
    {
        // Set the gizmo color to yellow.
        Gizmos.color = Color.yellow;
        // Draw a wire sphere to visualize the detection radius.
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
