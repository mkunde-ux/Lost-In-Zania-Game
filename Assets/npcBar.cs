using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class npcBar : MonoBehaviour
{
    public int maxHealth = 20;
    public int currentHealth;
    public SliderBar healthBar;

    public float updateInterval = 4f; // Time between each suspicion update
    public int suspicionChangeAmount = 3; // Amount to increase or decrease suspicion

    // Reference to NPCBehavior to access likedNPC, dislikedNPC, and player
    public NPCBehavior npcBehavior;

    private void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);

        // Start updating suspicion automatically
        StartCoroutine(SuspicionBarUpdate());
    }

    private void Update()
    {
        // Example test: Heal the NPC with the "H" key
        if (Input.GetKeyDown(KeyCode.H))
        {
            Heal(suspicionChangeAmount);
        }
    }

    // Gradually decrease suspicion (like taking damage)
    public void TakeDamage(int damage)
    {
        StartCoroutine(ChangeSuspicion(-damage));
    }

    // Gradually heal or restore suspicion (like regaining trust)
    public void Heal(int healAmount)
    {
        StartCoroutine(ChangeSuspicion(healAmount));
    }

    // Coroutine to handle gradual suspicion changes over time
    IEnumerator ChangeSuspicion(int changeAmount)
    {
        int steps = Mathf.Abs(changeAmount);
        int direction = changeAmount > 0 ? 1 : -1;

        for (int i = 0; i < steps; i++)
        {
            currentHealth += direction; // Add or subtract from current suspicion level
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Keep within bounds
            healthBar.SetHealth(currentHealth); // Update UI bar
            yield return new WaitForSeconds(0.1f); // Small delay for smooth transition
        }
    }

    // Coroutine that updates the suspicion bar automatically every few seconds
    IEnumerator SuspicionBarUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);

            // Use npcBehavior to check nearby NPCs and player
            if (npcBehavior != null)
            {
                // Check if both disliked NPC and player are nearby
                if (npcBehavior.dislikedNPC != null && npcBehavior.player != null)
                {
                    TakeDamage(suspicionChangeAmount); // Increase suspicion
                }
                // Check if both liked NPC and player are nearby
                else if (npcBehavior.likedNPC != null && npcBehavior.player != null)
                {
                    Heal(suspicionChangeAmount); // Decrease suspicion
                }
            }
        }
    }
}