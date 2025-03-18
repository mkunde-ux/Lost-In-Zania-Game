using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FahariTrustAlert : MonoBehaviour
{
    // Reference to the FahariTrust script.
    public FahariTrust wealthyGirl;
    // The exclamation mark image.
    public Image alertImage;
    // The alert sound effect.
    public AudioClip alertSound;
    // AudioSource component on the gameobject.
    private AudioSource audioSource;

    // Track if the alert is currently active.
    private bool alertActive = false;

    // Duration of the stretch animation.
    public float stretchDuration = 0.2f;
    // Scale factor during stretch.
    public float stretchScale = 1.5f;

    // Store the original scale of the image.
    private Vector3 originalScale;

    void Start()
    {
        // Ensure necessary components are attached.
        if (wealthyGirl == null)
        {
            Debug.LogError("FahariTrust script not assigned!");
            return; // Exit early to avoid errors
        }

        if (alertImage == null)
        {
            Debug.LogError("Alert Image not assigned!");
            return;
        }

        if (alertSound == null)
        {
            Debug.LogError("Alert Sound not assigned!");
            return;
        }

        // Initialize the alert image to be inactive.
        alertImage.gameObject.SetActive(false);

        // Get the AudioSource component or add one if it doesn't exist.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Set the alert sound.
        audioSource.clip = alertSound;

        // Store original scale.
        originalScale = alertImage.transform.localScale;
        // Start inactive.
        alertImage.gameObject.SetActive(false);
    }

    void Update()
    {
        // Check if the trust threshold has been reached and the alert is not already active.
        if (wealthyGirl.currentTrust <= wealthyGirl.maxTrust * (wealthyGirl.trustThresholdToAlertGuard / 100f) && !alertActive)
        {
            ActivateAlert();
        }
        // This is optional, if you want the alarm to turn off once the trust is higher again.
        else if (wealthyGirl.currentTrust > wealthyGirl.maxTrust * (wealthyGirl.trustThresholdToAlertGuard / 100f) && alertActive)
        {
            DeactivateAlert();
        }
    }

    private void ActivateAlert()
    {
        // Set alertActive to true.
        alertActive = true;
        // Make the alert image visible.
        alertImage.gameObject.SetActive(true);

        // Start the animation coroutine.
        StartCoroutine(StretchAndReturn());
        // Play the alert sound.
        audioSource.Play();
        // Log the alert activation.
        Debug.Log("Alert Activated!");
    }

    private void DeactivateAlert()
    {
        // Set alertActive to false.
        alertActive = false;
        // Make the alert image invisible.
        alertImage.gameObject.SetActive(false);
        // Stop the alert sound.
        audioSource.Stop();
        // Reset the scale of the alert image immediately.
        alertImage.transform.localScale = originalScale;
        // Log the alert deactivation.
        Debug.Log("Alert Deactivated!");
    }

    private IEnumerator StretchAndReturn()
    {
        // Timer for the animation.
        float timer = 0f;

        // Stretch phase.
        while (timer < stretchDuration)
        {
            // Increment the timer.
            timer += Time.deltaTime;
            // Normalized time (0 to 1).
            float t = timer / stretchDuration;
            // Smooth interpolation.
            float currentScale = Mathf.Lerp(1f, stretchScale, t);
            // Apply the scale to the alert image.
            alertImage.transform.localScale = originalScale * currentScale;
            // Wait for the next frame.
            yield return null;
        }

        // Return to original size phase (optional, but makes it smoother).
        timer = 0f;
        while (timer < stretchDuration)
        {
            timer += Time.deltaTime;
            float t = timer / stretchDuration;
            float currentScale = Mathf.Lerp(stretchScale, 1f, t);
            alertImage.transform.localScale = originalScale * currentScale;
            yield return null;
        }

        // Ensure original scale is set.
        alertImage.transform.localScale = originalScale;
    }
}