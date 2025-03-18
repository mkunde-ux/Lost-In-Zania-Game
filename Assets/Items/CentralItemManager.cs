using UnityEngine;
using System.Collections.Generic;

public class CentralItemManager : MonoBehaviour
{
    /*
    // Singleton instance of the CentralItemManager.
    private static CentralItemManager _instance;
    // Public accessor for the singleton instance.
    public static CentralItemManager Instance => _instance;

    // List to hold all registered items.
    private List<Item> allItems = new List<Item>();

    // Called when the script instance is being loaded.
    private void Awake()
    {
        // Ensure only one instance of the CentralItemManager exists.
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        // Set the singleton instance.
        _instance = this;
        // Prevent the CentralItemManager from being destroyed when loading new scenes.
        DontDestroyOnLoad(this.gameObject);
    }

    // Registers an item with the CentralItemManager.
    public void RegisterItem(Item item)
    {
        // Add the item to the list if it's not already registered.
        if (!allItems.Contains(item))
        {
            allItems.Add(item);
        }
    }

    // Unregisters an item from the CentralItemManager.
    public void UnregisterItem(Item item)
    {
        // Remove the item from the list if it's registered.
        if (allItems.Contains(item))
        {
            allItems.Remove(item);
        }
    }

    // Gets the closest item to a given position within a specified range.
    public Item GetClosestItem(Vector3 position, float range)
    {
        // Initialize variables to track the closest item and its distance.
        Item closest = null;
        float closestDist = Mathf.Infinity;

        // Iterate through all registered items.
        foreach (var item in allItems)
        {
            // Skip if the item is null or being held.
            if (item == null) continue;
            if (item.isHeld) continue;

            // Calculate the distance between the position and the item.
            float dist = Vector3.Distance(position, item.transform.position);
            // Update the closest item if it's within range and closer than the current closest item.
            if (dist < range && dist < closestDist)
            {
                closestDist = dist;
                closest = item;
            }
        }
        // Return the closest item.
        return closest;
    }

    // Picks up an item and attaches it to a holder transform.
    public void PickUpItem(Item item, Transform holder)
    {
        // Return if the item is already being held.
        if (item.isHeld) return;

        // Set the item's held flag and parent it to the holder.
        item.isHeld = true;
        item.transform.SetParent(holder);
        // Set the item's local position to zero.
        item.transform.localPosition = Vector3.zero;
        // Disable the item's Rigidbody if it has one.
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    // Drops an item and detaches it from its parent.
    public void DropItem(Item item)
    {
        // Return if the item is not being held.
        if (!item.isHeld) return;

        // Reset the item's held flag and unparent it.
        item.isHeld = false;
        item.transform.SetParent(null);
        // Enable the item's Rigidbody if it has one.
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;
    }

    // Gifts an item and destroys its GameObject.
    public void GiftItem(Item item)
    {
        // Log that the item was gifted.
        Debug.Log("Item was gifted: " + item.name);
        // Destroy the item's GameObject.
        Destroy(item.gameObject);
    }
    */
}