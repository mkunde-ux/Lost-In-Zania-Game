using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections;

public class AdilaHeadLook : MonoBehaviour
{
    [Header("Settings")]
    // Rig component for head look.
    [SerializeField] private Rig rig;
    // Transform to look at.
    [SerializeField] private Transform headLookAtTransform;
    // Speed of looking at the target.
    [SerializeField] private float lookSpeed = 3f;
    // Duration to look at the target.
    [SerializeField] private float lookDuration = 2f;
    // Speed of returning to default position.
    [SerializeField] private float returnToDefaultSpeed = 2f;

    // Flag indicating if the head is looking at a position.
    private bool isLookingAtPosition;
    // Current weight of the rig.
    private float currentWeight;
    // Default look position.
    private Vector3 defaultLookPosition;
    // Coroutine for looking at a position.
    private Coroutine lookCoroutine;

    // Called when the script starts.
    private void Start()
    {
        // Store the default look position.
        defaultLookPosition = headLookAtTransform.position;
    }

    // Called every frame.
    private void Update()
    {
        // Update the look weight.
        UpdateLookWeight();
    }

    // Updates the look weight based on the looking state.
    private void UpdateLookWeight()
    {
        // Target weight is 1 if looking at a position, 0 otherwise.
        float targetWeight = isLookingAtPosition ? 1f : 0f;

        // Lerp the current weight towards the target weight.
        currentWeight = Mathf.Lerp(currentWeight, targetWeight, (isLookingAtPosition ? lookSpeed : returnToDefaultSpeed) * Time.deltaTime);
        // Set the rig weight.
        rig.weight = currentWeight;
    }

    // Makes the head look at a position.
    public void LookAtPosition(Vector3 position, float duration = 0f)
    {
        // Stop any existing look coroutine.
        if (lookCoroutine != null)
        {
            StopCoroutine(lookCoroutine);
        }

        // Set the looking flag to true.
        isLookingAtPosition = true;
        // Set the look at transform position.
        headLookAtTransform.position = position;

        // Start a coroutine to stop looking after a duration if specified.
        if (duration > 0)
        {
            lookCoroutine = StartCoroutine(StopLookingAfterDuration(duration));
        }
    }

    // Coroutine to stop looking after a duration.
    private IEnumerator StopLookingAfterDuration(float duration)
    {
        // Wait for the specified duration.
        yield return new WaitForSeconds(duration);
        // Stop looking.
        StopLooking();
        // Reset the look coroutine.
        lookCoroutine = null;
    }

    // Stops the head from looking at a position.
    public void StopLooking()
    {
        // Set the looking flag to false.
        isLookingAtPosition = false;
    }
}