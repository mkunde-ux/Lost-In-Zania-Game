using UnityEngine;
using TMPro;

public class InteractableDoor : MonoBehaviour
{
    // Settings for player interaction with the door.
    [Header("Interaction Settings")]
    public float interactionRadius = 3f;
    public string playerTag = "Player";
    public GameObject popupUI;
    public KeyCode interactKey = KeyCode.F;

    // Settings for the door's rotation animation.
    [Header("Door Rotation Settings")]
    public float rotationAngle = 90f;
    public float rotationSpeed = 2f;

    // Reference to the player's transform.
    private Transform playerTransform;
    // Flag to track if the player is within interaction range.
    private bool isPlayerInRange = false;
    // Flag to track if the door is currently open.
    private bool isDoorOpen = false;
    // Initial rotation of the door.
    private Quaternion initialRotation;
    // Target rotation for the door.
    private Quaternion targetRotation;

    // Flag to track if the door is currently interactable.
    private bool isInteractable = false;
    // Called when the script instance is being loaded.
    void Start()
    {
        // Find the player's transform using the player tag.
        playerTransform = GameObject.FindGameObjectWithTag(playerTag)?.transform;
        // Log an error if the player is not found.
        if (playerTransform == null) Debug.LogError("Player not found!");

        // Store the initial rotation of the door.
        initialRotation = transform.rotation;
        // Initialize the target rotation to the initial rotation.
        targetRotation = initialRotation;

        // Disable the popup UI if it's assigned.
        if (popupUI != null)
        {
            popupUI.SetActive(false);
        }
    }

    // Called once per frame.
    void Update()
    {
        // Return if the player transform is not assigned.
        if (playerTransform == null) return;

        // Calculate the distance between the player and the door.
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        // Set the player in range flag based on the distance.
        isPlayerInRange = distance <= interactionRadius;

        // Enable or disable the popup UI based on player proximity, door state, and interactability.
        if (popupUI != null)
        {
            popupUI.SetActive(isPlayerInRange && !isDoorOpen && isInteractable);
        }

        // Open the door if the player is in range and presses the interact key.
        if (isPlayerInRange && Input.GetKeyDown(interactKey) && !isDoorOpen && isInteractable)
        {
            OpenDoor();
        }

        // Smoothly rotate the door towards the target rotation.
        if (transform.rotation != targetRotation)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // Sets the interactable state of the door.
    public void SetInteractable(bool interactable)
    {
        // Update the interactable flag and log the change.
        isInteractable = interactable;
        Debug.Log($"Door interactable: {interactable}");
    }

    // Opens the door by setting the target rotation.
    void OpenDoor()
    {
        // Calculate the target rotation for the open door.
        targetRotation = initialRotation * Quaternion.Euler(0, rotationAngle, 0);
        // Set the door open flag.
        isDoorOpen = true;

        // Disable the popup UI if it's assigned.
        if (popupUI != null)
        {
            popupUI.SetActive(false);
        }

        // Log that the door has been opened.
        Debug.Log("Door opened.");
    }

    // Draws a wire sphere Gizmo in the editor to visualize the interaction radius.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}