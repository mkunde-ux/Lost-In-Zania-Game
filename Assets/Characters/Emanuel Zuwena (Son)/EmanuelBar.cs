using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class EmanuelBar : MonoBehaviour
{
    //Check Adila TrustGuage script for comments

    public Slider slider;
    public Gradient gradient;
    public Image fill;

    public float smoothSpeed = 0.1f;
    private float targetValue;

    private void Update()
    {

        if (slider == null || fill == null)
        {
            Debug.LogError("Slider or fill Image reference is not assigned in WealthyGirldTrustBar.");
            return;
        }

        slider.value = Mathf.Lerp(slider.value, targetValue, smoothSpeed * Time.deltaTime);
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    public void SetMaxTrust(int trust)
    {
        if (slider == null || fill == null)
        {
            Debug.LogError("Slider or fill Image reference is not assigned in WealthyGirldTrustBar.");
            return;
        }

        slider.maxValue = trust;
        targetValue = trust;
        slider.value = trust;
        fill.color = gradient.Evaluate(1f);
    }

    public void SetTrust(int trust)
    {
        targetValue = trust;
    }
}