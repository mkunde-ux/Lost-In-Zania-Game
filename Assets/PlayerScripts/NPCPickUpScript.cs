using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public class NPCPickUpScript : MonoBehaviour
{
    // Settings for the NPC's pickup behavior.
    [Header("NPC Settings")]
    public float detectionRadius = 10f;
    public Transform holdPosition;
    public Transform pickableSpot;
    public float throwForce = 700f;

    // List of GameObjects that the NPC can pick up.
    [Header("Pickable Item References")]
    public List<GameObject> availablePickUpItems = new List<GameObject>();

    // Rigidbody of the item currently detected by the NPC.
    private Rigidbody detectedItemRb = null;
    // NavMeshAgent component of the NPC.
    private NavMeshAgent agent;
    // Flag to track if the NPC has picked up an item.
    private bool hasPickedUpItem = false;
    // Flag to track if the NPC has reached the pickable spot.
    private bool reachedPickableSpot = false;
    // GameObject of the item currently picked up by the NPC.
    private GameObject currentPickedUpItem = null;
    // Flag to track if an item has been thrown towards the NPC.
    private bool itemThrownTowardsNPC = false;

    // Called when the script instance is being loaded.
    void Start()
    {
        // Get the NavMeshAgent component.
        agent = GetComponent<NavMeshAgent>();

        // Find the pickable spot GameObject and assign its transform.
        GameObject spot = GameObject.FindGameObjectWithTag("PickableSpot");
        if (spot != null)
        {
            pickableSpot = spot.transform;
        }
        // Log a warning if the pickable spot is not found.
        else
        {
            Debug.LogWarning("PickableSpot not found! Assign it or check the tag.");
        }
    }

    // Called once per frame.
    void Update()
    {
        // Detect a pickable item if the NPC has not picked up an item.
        if (!hasPickedUpItem)
        {
            DetectPickableItem();
        }

        // Move towards the item if it has been thrown towards the NPC.
        if (itemThrownTowardsNPC && currentPickedUpItem != null && !hasPickedUpItem)
        {
            MoveTowardsItem();
        }

        // Move towards the pickable spot if the NPC has picked up an item.
        if (hasPickedUpItem && !reachedPickableSpot && pickableSpot != null)
        {
            // Set the destination to the pickable spot if it's not already set.
            if (agent.destination != pickableSpot.position)
            {
                agent.ResetPath();
                agent.SetDestination(pickableSpot.position);
                Debug.Log("Setting destination to pickableSpot after picking up item.");
            }

            // Drop the item when the NPC reaches the pickable spot.
            if (Vector3.Distance(transform.position, pickableSpot.position) <= 1f)
            {
                reachedPickableSpot = true;
                DropItem();
            }
        }
    }

    // Detects a pickable item within the detection radius.
    private void DetectPickableItem()
    {
        // Find all colliders within the detection radius.
        Collider[] itemsInRange = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider itemCollider in itemsInRange)
        {
            // Check if the collider has the "PickableItem" tag and the NPC has not picked up an item.
            if (itemCollider.CompareTag("PickableItem") && !hasPickedUpItem)
            {
                // Get the Rigidbody component of the item.
                Rigidbody itemRb = itemCollider.GetComponent<Rigidbody>();
                if (itemRb != null)
                {
                    // Set the detected item Rigidbody and GameObject, and set the item thrown flag.
                    detectedItemRb = itemRb;
                    currentPickedUpItem = itemCollider.gameObject;
                    itemThrownTowardsNPC = true;
                    break;
                }
            }
        }
    }

    // Moves the NPC towards the detected item.
    private void MoveTowardsItem()
    {
        // Move towards the item if it exists.
        if (currentPickedUpItem != null)
        {
            agent.SetDestination(currentPickedUpItem.transform.position);

            // Pick up the item when the NPC is close enough.
            if (Vector3.Distance(transform.position, currentPickedUpItem.transform.position) <= 1.5f)
            {
                PickUpItem();
            }
        }
    }

    // Picks up the detected item.
    private void PickUpItem()
    {
        // Pick up the item if it exists and the NPC has not picked up an item.
        if (currentPickedUpItem != null && detectedItemRb != null && !hasPickedUpItem)
        {
            // Set the item's Rigidbody to kinematic and parent it to the hold position.
            detectedItemRb.isKinematic = true;
            currentPickedUpItem.transform.SetParent(holdPosition);
            currentPickedUpItem.transform.localPosition = Vector3.zero;
            hasPickedUpItem = true;
            reachedPickableSpot = false;

            // Set the destination to the pickable spot.
            agent.ResetPath();
            agent.SetDestination(pickableSpot.position);
            Debug.Log("PickUpItem: Destination set to pickableSpot.");
        }
    }

    // Drops the currently picked up item.
    private void DropItem()
    {
        // Drop the item if it exists.
        if (currentPickedUpItem != null)
        {
            // Unparent the item and set its Rigidbody to non-kinematic.
            currentPickedUpItem.transform.SetParent(null);
            detectedItemRb.isKinematic = false;
            ResetFlagsForNextPickUp();
        }
    }

    // Throws the currently picked up item in a specified direction.
    public void ThrowItem(Vector3 direction)
    {
        // Throw the item if it exists.
        if (currentPickedUpItem != null && detectedItemRb != null)
        {
            // Unparent the item, set its Rigidbody to non-kinematic, and apply force.
            currentPickedUpItem.transform.SetParent(null);
            detectedItemRb.isKinematic = false;
            detectedItemRb.AddForce(direction * throwForce);
            ResetFlagsForNextPickUp();
        }
    }

    // Resets the flags for the next pickup.
    private void ResetFlagsForNextPickUp()
    {
        currentPickedUpItem = null;
        detectedItemRb = null;
        hasPickedUpItem = false;
        itemThrownTowardsNPC = false;
        reachedPickableSpot = false;
    }

    // Draws a wire sphere Gizmo in the editor to visualize the detection radius.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}