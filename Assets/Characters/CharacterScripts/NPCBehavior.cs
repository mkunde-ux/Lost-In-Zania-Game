using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBehavior : MonoBehaviour
{
    // Settings for detecting nearby NPCs.
    [Header("Detection Settings")]
    public float detectionRadius = 10f;
    public string playerTag = "Player";
    public string likedNPCTag = "LikedNPC";
    public string dislikedNPCTag = "DislikedNPC";

    // Public properties to access detected NPCs.
    public Transform likedNPC { get; private set; }
    public Transform dislikedNPC { get; private set; }
    public Transform player { get; private set; }

    // Settings for suspicion behavior.
    [Header("Suspicion Settings")]
    public npcBar suspicionBar;
    public Guard securityGuard;
    public Guard euodiaGuard;
    private bool guardFollowingPlayer = false;

    // Settings for gradual suspicion changes.
    public float suspicionChangeInterval = 4f;
    private float suspicionTimer = 0f;

    // Called when the script instance is being loaded.
    private void Start()
    {
        // Initialize suspicion bar.
        suspicionBar.currentHealth = suspicionBar.maxHealth;
        suspicionBar.healthBar.SetMaxHealth(suspicionBar.maxHealth);

        // Start gradual suspicion change coroutine.
        StartCoroutine(GradualSuspicionChange());
    }

    // Called once per frame.
    private void Update()
    {
        // Detect nearby NPCs.
        DetectNearbyNPCs();
        // Handle NPC behavior based on detected NPCs.
        HandleBehavior();

        // Check if suspicion is low enough to make guards follow the player.
        if (suspicionBar.currentHealth <= suspicionBar.maxHealth / 4)
        {
            if (!guardFollowingPlayer && player != null)
            {
                MakeGuardsFollowPlayer();
                guardFollowingPlayer = true;
            }
        }
        // Check if suspicion is high enough to make guards resume patrolling.
        else if (suspicionBar.currentHealth > suspicionBar.maxHealth / 4)
        {
            if (guardFollowingPlayer)
            {
                MakeGuardsResumePatrolling();
                guardFollowingPlayer = false;
            }
        }
    }

    // Detects nearby NPCs within the detection radius.
    void DetectNearbyNPCs()
    {
        // Find all colliders within the detection radius.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);

        // Reset detected NPCs.
        likedNPC = null;
        dislikedNPC = null;
        player = null;

        // Iterate through detected colliders.
        foreach (var hitCollider in hitColliders)
        {
            // Check if the collider has the "LikedNPC" tag.
            if (hitCollider.CompareTag(likedNPCTag))
            {
                likedNPC = hitCollider.transform;
            }
            // Check if the collider has the "DislikedNPC" tag.
            else if (hitCollider.CompareTag(dislikedNPCTag))
            {
                dislikedNPC = hitCollider.transform;
            }
            // Check if the collider has the "Player" tag.
            else if (hitCollider.CompareTag(playerTag))
            {
                player = hitCollider.transform;
            }
        }
    }

    // Handles NPC behavior based on detected NPCs.
    void HandleBehavior()
    {
        // Check if a disliked NPC and player are detected.
        if (dislikedNPC != null && player != null)
        {
            IncreaseSuspicion(5);
            MoveAwayFrom(dislikedNPC);
        }
        // Check if a liked NPC and player are detected.
        else if (likedNPC != null && player != null)
        {
            DecreaseSuspicion(5);
            MoveTowards(likedNPC);
            likedNPC.GetComponent<NPCBehavior>()?.MoveAwayFrom(player);
        }
        // Check if only a liked NPC is detected.
        else if (likedNPC != null)
        {
            StayIdle();
        }
    }

    // Coroutine for gradual suspicion changes.
    IEnumerator GradualSuspicionChange()
    {
        while (true)
        {
            yield return new WaitForSeconds(suspicionChangeInterval);

            // Check if a disliked NPC and player are detected.
            if (dislikedNPC != null && player != null)
            {
                IncreaseSuspicion(3);
            }
            // Check if a liked NPC and player are detected.
            else if (likedNPC != null && player != null)
            {
                DecreaseSuspicion(3);
            }
        }
    }

    // Makes guards follow the player.
    void MakeGuardsFollowPlayer()
    {
        if (securityGuard != null && player != null)
        {
            securityGuard.FollowPlayer(player);
        }
        if (euodiaGuard != null && player != null)
        {
            euodiaGuard.FollowPlayer(player);
        }
    }

    // Makes guards resume patrolling.
    void MakeGuardsResumePatrolling()
    {
        if (securityGuard != null)
        {
            securityGuard.ResumePatrolling();
        }
        if (euodiaGuard != null)
        {
            euodiaGuard.ResumePatrolling();
        }
    }

    // Increases suspicion.
    void IncreaseSuspicion(int damage)
    {
        suspicionBar.TakeDamage(damage);
    }

    // Decreases suspicion.
    void DecreaseSuspicion(int healAmount)
    {
        suspicionBar.Heal(healAmount);
    }

    // Moves the NPC away from a target.
    void MoveAwayFrom(Transform target)
    {
        Vector3 direction = (transform.position - target.position).normalized;
        transform.position += direction * Time.deltaTime * 2f;
    }

    // Moves the NPC towards a target.
    void MoveTowards(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * Time.deltaTime * 2f;
    }

    // Makes the NPC stay idle.
    void StayIdle()
    {

    }

    // Draws a wire sphere Gizmo in the editor to visualize the detection radius.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}