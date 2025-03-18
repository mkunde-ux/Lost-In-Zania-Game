using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LikedNPCSliderBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    public float smoothSpeed = 0.1f; // Speed of the smooth transition

    private float targetValue; // The target value we want to reach

    private void Update()
    {
        // Smoothly transition to the target value
        slider.value = Mathf.Lerp(slider.value, targetValue, smoothSpeed * Time.deltaTime);
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    // Set the maximum trust level
    public void SetMaxTrust(int trust)
    {
        slider.maxValue = trust;
        targetValue = trust;
        slider.value = trust;
        fill.color = gradient.Evaluate(1f);
    }

    // Set the current trust level
    public void SetTrust(int trust)
    {
        targetValue = trust; // Set the new target value for smooth transition
    }
}

