using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    public float smoothSpeed = 0.1f; // Speed of the smooth transition

    private float targetHealth; // The target value we want to reach

    private void Update()
    {
        // Smoothly transition to the target value
        slider.value = Mathf.Lerp(slider.value, targetHealth, smoothSpeed * Time.deltaTime);
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        targetHealth = health;
        slider.value = health;
        fill.color = gradient.Evaluate(1f);
    }

    public void SetHealth(int health)
    {
        targetHealth = health; // Set the new target value for smooth transition
    }
}



/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;


    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;

        fill.color = gradient.Evaluate(1f);
    }

    public void SetHealth(int health)
    {
        slider.value = health;

        fill.color = gradient.Evaluate(slider.normalizedValue);

    }


}
*/