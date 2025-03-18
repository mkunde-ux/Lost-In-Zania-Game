using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PickUpDropScript : MonoBehaviour
{
    [Header("References")]
    // Reference to the player GameObject.
    public GameObject player;
    // Transform representing the position where the object will be held.
    public Transform holdPosition;
    // List of available pick-up items.
    public List<GameObject> availablePickUpItems = new List<GameObject>();
    // Reference to the ThirdPersonCam script.
    public ThirdPersonCam thirdPersonCam;
    // Reference to the in-world popup UI GameObject.
    public GameObject inWorldPopup;
    // Tag for pickable items.
    public string pickableItemTag = "PickableItem";
    // Tag for gift items.
    public string giftTag = "Gift";

    [Header("Settings")]
    // Range within which items can be picked up.
    public float pickUpRange = 3f;
    // Key to pick up or drop objects.
    public KeyCode pickUpKey = KeyCode.Q;
    // Key to throw objects.
    public KeyCode throwKey = KeyCode.Mouse1;
    // Minimum force applied when throwing objects.
    public float minThrowForce = 100f;
    // Maximum force applied when throwing objects.
    public float maxThrowForce = 1000f;
    // Maximum time the throw key can be held to charge the throw.
    public float maxHoldTime = 3f;

    // Rigidbody of the currently held object.
    private Rigidbody pickUpRb;
    // Flag indicating if an object is currently being held.
    private bool isHoldingObject = false;
    // Time the throw key has been held.
    private float throwHoldTime = 0f;

    // Currently picked up item GameObject.
    private GameObject currentPickedUpItem = null;
    // Currently interactable item GameObject.
    private GameObject currentInteractableItem = null;

    // Public property to check if an object is being held.
    public bool IsHoldingObject => isHoldingObject;

    // Time of the last gift given.
    private float lastGiftTime = -10f;
    // Cooldown time for giving gifts.
    public float giftCooldown = 10f;
    // Flag indicating if a gift has been given.
    private bool isGiftGiven = false;

    // Public property to get the currently held item.
    public GameObject HeldItem
    {
        get { return currentPickedUpItem; }
    }

    // Public property to get and privately set the currently picked up item.
    public GameObject CurrentPickedUpItem
    {
        get { return currentPickedUpItem; }
        private set { currentPickedUpItem = value; }
    }

    private void Start()
    {
        // Ensure the in-world popup is initially hidden.
        if (inWorldPopup != null)
        {
            inWorldPopup.SetActive(false);
        }
    }

    private void Update()
    {
        // Check for interactable items within range.
        CheckForInteractableItems();

        // Handle pick up/drop input.
        if (Input.GetKeyDown(pickUpKey))
        {
            if (isHoldingObject)
            {
                DropObject();
            }
            else
            {
                TryPickUpObject();
            }
        }

        // Handle throw charge.
        if (isHoldingObject && Input.GetKey(throwKey))
        {
            throwHoldTime += Time.deltaTime;
        }

        // Handle throw release.
        if (isHoldingObject && Input.GetKeyUp(throwKey))
        {
            ThrowObject();
        }
    }

    private void CheckForInteractableItems()
    {
        // Find all colliders within the pick-up range.
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, pickUpRange);
        GameObject closestItem = null;
        float closestDistance = Mathf.Infinity;

        // check through colliders to find the closest pickable or gift item.
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(pickableItemTag) || hitCollider.CompareTag(giftTag))
            {
                float distance = Vector3.Distance(player.transform.position, hitCollider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestItem = hitCollider.gameObject;
                }
            }
        }

        // Update the current interactable item and popup visibility.
        if (closestItem != null && !isHoldingObject)
        {
            currentInteractableItem = closestItem;
            if (inWorldPopup != null)
            {
                inWorldPopup.SetActive(true);
            }
        }
        else
        {
            currentInteractableItem = null;
            if (inWorldPopup != null)
            {
                inWorldPopup.SetActive(false);
            }
        }
    }

    private void TryPickUpObject()
    {
        // Check if the player is already holding an item.
        if (isHoldingObject)
        {
            Debug.Log("Player is already holding an item and cannot pick up another.");
            return;
        }

        // Check if there is a currently interactable item.
        if (currentInteractableItem != null)
        {
            // Set the currently picked up item.
            currentPickedUpItem = currentInteractableItem;
            // Get the Rigidbody component of the picked up item.
            pickUpRb = currentPickedUpItem.GetComponent<Rigidbody>();

            // Check if the item has a Rigidbody component.
            if (pickUpRb == null)
            {
                Debug.LogError("Item does not have a Rigidbody: " + currentPickedUpItem.name);
                return;
            }

            // Call the PickUpObject method.
            PickUpObject();
        }
    }

    public void AddPickUpItem(GameObject item)
    {
        // Check if the item is not null and not already in the list.
        if (item != null && !availablePickUpItems.Contains(item))
        {
            // Add the item to the list of available pick-up items.
            availablePickUpItems.Add(item);
            Debug.Log("Added item to pick-up list: " + item.name);
        }
        else
        {
            Debug.LogWarning("Item is null or already in the list.");
        }
    }

    private void PickUpObject()
    {
        // Check if there is a currently picked up item.
        if (currentPickedUpItem != null)
        {
            // Set the picked up item's parent to the hold position.
            currentPickedUpItem.transform.SetParent(holdPosition);
            // Set the picked up item's local position to zero.
            currentPickedUpItem.transform.localPosition = Vector3.zero;
            // Set the Rigidbody's isKinematic property to true.
            pickUpRb.isKinematic = true;
            // Set the isHoldingObject flag to true.
            isHoldingObject = true;
            // Reset the throw hold time.
            throwHoldTime = 0f;
            Debug.Log("Picked up: " + currentPickedUpItem.name);

            // Hide the in-world popup when an item is picked up.
            if (inWorldPopup != null)
            {
                inWorldPopup.SetActive(false);
            }
        }
    }

    public void DropObject()
    {
        // Check if there is an item to drop.
        if (currentPickedUpItem == null)
        {
            Debug.Log("No item to drop.");
            return;
        }

        // Unparent the picked up item.
        currentPickedUpItem.transform.SetParent(null);
        // Set the Rigidbody's isKinematic property to false.
        pickUpRb.isKinematic = false;
        // Clear the currently picked up item.
        currentPickedUpItem = null;
        // Set the isHoldingObject flag to false.
        isHoldingObject = false;
        Debug.Log("Dropped item.");

        // Check for interactable items to re-enable the popup.
        CheckForInteractableItems();
    }

    private void ThrowObject()
    {
        // Check if there is an item to throw.
        if (currentPickedUpItem == null)
        {
            Debug.Log("No item to throw.");
            return;
        }

        // Unparent the picked up item.
        currentPickedUpItem.transform.SetParent(null);
        // Set the Rigidbody's isKinematic property to false.
        pickUpRb.isKinematic = false;

        // Calculate the throw force based on the hold time.
        float normalizedHoldTime = Mathf.Clamp01(throwHoldTime / maxHoldTime);
        float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, normalizedHoldTime);

        // Calculate the throw direction based on the player's forward direction.
        Vector3 throwDirection = thirdPersonCam.playerObj.forward;
        // Apply force to the Rigidbody.
        pickUpRb.AddForce(throwDirection * throwForce);

        // Clear the currently picked up item.
        currentPickedUpItem = null;
        // Set the isHoldingObject flag to false.
        isHoldingObject = false;
        // Reset the throw hold time.
        throwHoldTime = 0f;
        Debug.Log("Threw item with force: " + throwForce);

        // Check for interactable items to re-enable the popup.
        CheckForInteractableItems();
    }

    public void HandleGift(GameObject gift)
    {
        // Process the gift when interacting with an NPC.
        Debug.Log("Gift given to NPC!");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.transform.position, pickUpRange);
    }
}