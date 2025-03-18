using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdilaTrustGuage : MonoBehaviour
{
    // Reference to the UI Slider.
    public Slider slider;
    // Gradient for the fill color.
    public Gradient gradient;
    // Image component of the fill area.
    public Image fill;

    // Speed for smoothing the slider's value changes.
    public float smoothSpeed = 0.1f;
    // Target value for the slider.
    private float targetValue;

    // Called once per frame.
    private void Update()
    {
        // Check if the slider or fill image is not assigned.
        if (slider == null || fill == null)
        {
            Debug.LogError("Slider or fill Image reference is not assigned in WealthyGirldTrustBar.");
            return;
        }

        // Smoothly interpolate the slider's value towards the target value.
        slider.value = Mathf.Lerp(slider.value, targetValue, smoothSpeed * Time.deltaTime);
        // Update the fill color based on the slider's normalized value.
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    // Sets the maximum trust value and initializes the slider.
    public void SetMaxTrust(int trust)
    {
        // Check if the slider or fill image is not assigned.
        if (slider == null || fill == null)
        {
            Debug.LogError("Slider or fill Image reference is not assigned in WealthyGirldTrustBar.");
            return;
        }

        // Set the slider's maximum value.
        slider.maxValue = trust;
        // Set the target value to the maximum trust.
        targetValue = trust;
        // Set the slider's current value to the maximum trust.
        slider.value = trust;
        // Set the fill color to the maximum gradient value.
        fill.color = gradient.Evaluate(1f);
    }

    // Sets the target trust value for the slider.
    public void SetTrust(int trust)
    {
        // Set the target value for the slider.
        targetValue = trust;
    }
}