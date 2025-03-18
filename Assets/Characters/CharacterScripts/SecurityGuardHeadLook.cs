using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class SecurityGuardHeadLook : MonoBehaviour
{
    // Reference to the Rig component that controls head look.
    [SerializeField] private Rig headLookRig;
    // Transform that the head will look at.
    [SerializeField] private Transform headLookAtTransform;

    // Flag to track if the guard is currently looking at the player.
    private bool isLookingAtPlayer;
    // Transform of the player.
    private Transform player;

    // Interval for looking at the player.
    public float lookAtInterval = 15f;
    // Interval for looking away from the player.
    public float lookAwayInterval = 30f;

    // Called when the script instance is being loaded.
    private void Start()
    {
        // Log a warning if the HeadLook Rig is not assigned.
        if (headLookRig == null)
        {
            Debug.LogWarning("HeadLook Rig is not assigned on " + gameObject.name);
        }
    }

    // Called once per frame.
    private void Update()
    {
        // Handle the head look behavior.
        HandleHeadLook();
    }

    // Starts the head look at the player.
    public void StartLookingAtPlayer(Transform playerTransform)
    {
        // Set the player transform and flag.
        player = playerTransform;
        isLookingAtPlayer = true;
        // Stop any existing coroutines and start the toggle coroutine.
        StopAllCoroutines();
        StartCoroutine(ToggleHeadLook());
    }

    // Stops the head look at the player.
    public void StopLookingAtPlayer()
    {
        // Reset the flag and player transform.
        isLookingAtPlayer = false;
        player = null;
        // Stop all coroutines and set the rig weight to 0.
        StopAllCoroutines();
        headLookRig.weight = 0f;
    }

    // Handles the head look behavior.
    private void HandleHeadLook()
    {
        // Return if the rig or player transform is not assigned.
        if (headLookRig == null || player == null)
            return;

        // Calculate the target weight for the rig.
        float targetWeight = isLookingAtPlayer ? 1f : 0f;
        // Set the lerp speed for the weight transition.
        float lerpSpeed = 2f;
        // Smoothly interpolate the rig weight towards the target weight.
        headLookRig.weight = Mathf.Lerp(headLookRig.weight, targetWeight, Time.deltaTime * lerpSpeed);

        // Set the head look at transform position if looking at the player.
        if (isLookingAtPlayer)
        {
            headLookAtTransform.position = player.position;
        }
    }

    // Coroutine to toggle the head look at intervals.
    private IEnumerator ToggleHeadLook()
    {
        while (true)
        {
            // Set the flag to look at the player and wait for the look at interval.
            isLookingAtPlayer = true;
            yield return new WaitForSeconds(lookAtInterval);

            // Set the flag to look away from the player and wait for the look away interval.
            isLookingAtPlayer = false;
            yield return new WaitForSeconds(lookAwayInterval);
        }
    }
}