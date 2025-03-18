using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NPCAIAlert : MonoBehaviour
{
    public FieldOfViewNPC fieldOfView; // Reference to the FieldOfViewNPC script
    public Image alertImage; // The exclamation mark image
    public AudioClip alertSound; // The alert sound effect
    private AudioSource audioSource; // AudioSource component on the gameobject

    private bool alertActive = false; // Track if the alert is currently active

    public float stretchDuration = 0.2f; // Duration of the stretch animation
    public float stretchScale = 1.5f; // Scale factor during stretch

    private Vector3 originalScale; // Store the original scale of the image

    void Start()
    {
        // Ensure necessary components are attached
        if (fieldOfView == null)
        {
            Debug.LogError("FieldOfViewNPC script not assigned!");
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

        // Initialize the alert image to be inactive
        alertImage.gameObject.SetActive(false);

        // Get the AudioSource component or add one if it doesn't exist
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = alertSound; // Set the alert sound

        originalScale = alertImage.transform.localScale; // Store original scale
    }

    void Update()
    {
        // Check if the player is in the field of view and the alert is not already active
        if (fieldOfView.canSeePlayer && !alertActive)
        {
            ActivateAlert();
        }
        // Optional: Deactivate alert if player is no longer seen.
        else if (!fieldOfView.canSeePlayer && alertActive)
        {
            DeactivateAlert();
        }
    }

    private void ActivateAlert()
    {
        alertActive = true;
        alertImage.gameObject.SetActive(true);

        StartCoroutine(StretchAndReturn()); // Start the animation coroutine
        audioSource.Play();
        Debug.Log("Alert Activated!");
    }

    private void DeactivateAlert()
    {
        alertActive = false;
        alertImage.gameObject.SetActive(false);
        audioSource.Stop();
        alertImage.transform.localScale = originalScale; // Reset scale immediately
        Debug.Log("Alert Deactivated!");
    }

    private IEnumerator StretchAndReturn()
    {
        float timer = 0f;

        // Stretch phase
        while (timer < stretchDuration)
        {
            timer += Time.deltaTime;
            float t = timer / stretchDuration; // Normalized time (0 to 1)
            float currentScale = Mathf.Lerp(1f, stretchScale, t); // Smooth interpolation
            alertImage.transform.localScale = originalScale * currentScale;
            yield return null; // Wait for the next frame
        }

        // Return to original size phase (optional, but makes it smoother)
        timer = 0f;
        while (timer < stretchDuration)
        {
            timer += Time.deltaTime;
            float t = timer / stretchDuration;
            float currentScale = Mathf.Lerp(stretchScale, 1f, t);
            alertImage.transform.localScale = originalScale * currentScale;
            yield return null;
        }

        alertImage.transform.localScale = originalScale; // Ensure original scale is set
    }
}