using UnityEngine;
using UnityEngine.Animations.Rigging;

public class NesHeadLook : MonoBehaviour
{
    [Header("Settings")]
    // Reference to the Rig component that controls the head look.
    [SerializeField] private Rig rig;
    // Transform that the head will look at.
    [SerializeField] private Transform headLookAtTransform;
    // Speed at which the head look weight changes.
    [SerializeField] private float lookSpeed = 3f;

    // Flag indicating whether the head is currently looking at a position.
    private bool isLookingAtPosition;
    // Current weight of the rig.
    private float currentWeight;
    // Default position for the head look.
    private Vector3 defaultLookPosition;

    private void Start()
    {
        // Store the default look position.
        defaultLookPosition = headLookAtTransform.position;
    }

    private void Update()
    {
        // Update the look weight based on whether the head should be looking at a position.
        UpdateLookWeight();
    }

    private void UpdateLookWeight()
    {
        // Determine the target weight based on whether the head should be looking at a position.
        float targetWeight = isLookingAtPosition ? 1f : 0f;
        // Smoothly interpolate the current weight towards the target weight.
        currentWeight = Mathf.Lerp(currentWeight, targetWeight, lookSpeed * Time.deltaTime);
        // Set the rig's weight to the current weight.
        rig.weight = currentWeight;
    }

    public void LookAtPosition(Vector3 position)
    {
        // Set the flag to indicate that the head should be looking at a position.
        isLookingAtPosition = true;
        // Set the head look at transform's position to the specified position.
        headLookAtTransform.position = position;
    }

    public void StopLooking()
    {
        // Set the flag to indicate that the head should stop looking at a position.
        isLookingAtPosition = false;
        // Reset the head look at transform's position to the default position.
        headLookAtTransform.position = defaultLookPosition;
    }
}