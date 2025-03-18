using System.Collections;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    // Radius of the field of view.
    public float radius = 10f;
    // Angle of the field of view.
    [Range(0, 360)] public float angle = 120f;
    // Reference to the guard's script.
    public MonoBehaviour guardScript;
    // Layer mask for target objects (e.g., player).
    public LayerMask targetMask;
    // Layer mask for obstruction objects (e.g., walls).
    public LayerMask obstructionMask;

    // Flag indicating if the player is within the field of view.
    public bool canSeePlayer { get; private set; }
    // Reference to the player's transform.
    public Transform playerRef { get; private set; }

    // Radius within which dialogue can be triggered.
    public float dialogueRadius = 2f;

    // Called when the script instance is being loaded.
    private void Start()
    {
        // Set the player reference.
        SetPlayerReference();
        // Validate the guard script assignment.
        ValidateGuardScript();
        // Start the field of view check routine.
        StartCoroutine(FOVRoutine());
    }

    // Sets the player reference by finding the GameObject with the "Player" tag.
    private void SetPlayerReference()
    {
        // Find the player GameObject.
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        // If the player GameObject is found, set the player reference.
        if (playerObject != null)
        {
            playerRef = playerObject.transform;
        }
        // If the player GameObject is not found, log an error.
        else
        {
            Debug.LogError("Player object with tag 'Player' not found!");
        }
    }

    // Validates that the guard script is assigned.
    private void ValidateGuardScript()
    {
        // If the guard script is not assigned, log an error.
        if (guardScript == null)
        {
            Debug.LogError("Guard script not assigned in FieldOfView! Please assign a Guard script.");
        }
    }

    // Coroutine for the field of view check routine.
    private IEnumerator FOVRoutine()
    {
        // Wait for 0.2 seconds between checks.
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        // Continuously check the field of view.
        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }

    // Checks the field of view for the player.
    private void FieldOfViewCheck()
    {
        // If the player reference or guard script is null, log an error and return.
        if (playerRef == null || guardScript == null)
        {
            Debug.LogError("Missing references: either playerRef or guardScript is null.");
            return;
        }

        // Check if the player is visible.
        bool playerIsVisible = CheckPlayerVisibility();

        // If the player is visible and was not previously seen, set canSeePlayer to true and call PlayerDetected.
        if (playerIsVisible && !canSeePlayer)
        {
            canSeePlayer = true;
            CallPlayerDetected();
            Debug.Log("Player Detected");
        }
        // If the player is not visible and was previously seen, set canSeePlayer to false and call PlayerLost.
        else if (!playerIsVisible && canSeePlayer)
        {
            canSeePlayer = false;
            CallPlayerLost();
            Debug.Log("Player Lost");
        }
        // Calculate the distance to the player and log it.
        float distanceToPlayer = Vector3.Distance(transform.position, playerRef.position);
        Debug.Log("Distance to player: " + distanceToPlayer);

        // Try to trigger dialogue if the player is within dialogue radius.
        TryTriggerDialogue(distanceToPlayer);
    }

    // Checks if the player is visible from the guard's position.
    private bool CheckPlayerVisibility()
    {
        // If the player reference is null, log a message and return false.
        if (playerRef == null)
        {
            Debug.Log("Player ref is null in CheckPlayerVisibility");
            return false;
        }

        // Calculate the direction to the player.
        Vector3 directionToPlayer = (playerRef.position - transform.position).normalized;

        // If the player is within the field of view angle.
        if (Vector3.Angle(transform.forward, directionToPlayer) < angle / 2)
        {
            // Calculate the distance to the player.
            float distanceToPlayer = Vector3.Distance(transform.position, playerRef.position);
            // Check if there are any obstructions between the guard and the player.
            bool raycastHit = !Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstructionMask);
            Debug.Log("Raycast hit: " + raycastHit);
            return raycastHit;
        }
        return false;
    }

    // Calls the PlayerDetected method on the guard script.
    private void CallPlayerDetected()
    {
        // Call PlayerDetected on the appropriate guard script.
        if (guardScript is SecurityAI)
        {
            ((SecurityAI)guardScript).PlayerDetected();
        }
        else if (guardScript is EuodiaAI)
        {
            ((EuodiaAI)guardScript).PlayerDetected();
        }
        else if (guardScript is AlenAI)
        {
            ((AlenAI)guardScript).PlayerDetected();
        }
    }

    // Calls the PlayerLost method on the guard script.
    private void CallPlayerLost()
    {
        // Call PlayerLost on the appropriate guard script.
        if (guardScript is SecurityAI)
        {
            ((SecurityAI)guardScript).PlayerLost();
        }
        else if (guardScript is EuodiaAI)
        {
            ((EuodiaAI)guardScript).PlayerLost();
        }
        else if (guardScript is AlenAI)
        {
            ((AlenAI)guardScript).PlayerLost();
        }
    }

    // Tries to trigger dialogue with the player.
    private void TryTriggerDialogue(float distanceToPlayer)
    {
        // If the player is within dialogue radius.
        if (distanceToPlayer <= dialogueRadius)
        {
            // Trigger dialogue on the appropriate guard script.
            if (guardScript is SecurityAI)
            {
                SecurityAI securityAI = (SecurityAI)guardScript;
                // Check guard state to prevent dialogue during chasing or when already in dialogue.
                if (!securityAI.isChasingPlayer && securityAI.currentGuardState != SecurityAI.GuardState.Dialogue && securityAI.currentGuardState != SecurityAI.GuardState.Chasing)
                {
                    securityAI.MoveToPlayerAndStartDialogue();
                    Debug.Log("SecurityAI Dialogue Triggered");
                }
            }
            else if (guardScript is EuodiaAI)
            {
                EuodiaAI euodiaAI = (EuodiaAI)guardScript;
                // Check guard state to prevent dialogue during chasing or when already in dialogue.
                if (!euodiaAI.isChasingPlayer && euodiaAI.currentGuardState != EuodiaAI.GuardState.Dialogue && euodiaAI.currentGuardState != EuodiaAI.GuardState.Chasing)
                {
                    euodiaAI.MoveToPlayerAndStartDialogue();
                    Debug.Log("EuodiaAI Dialogue Triggered");
                }
            }
            else if (guardScript is AlenAI)
            {
                AlenAI alenAI = (AlenAI)guardScript;
                // Check guard state to prevent dialogue during chasing or when already in dialogue.
                if (!alenAI.isChasingPlayer && alenAI.currentGuardState != AlenAI.GuardState.Dialogue && alenAI.currentGuardState != AlenAI.GuardState.Chasing)
                {
                    alenAI.MoveToPlayerAndStartDialogue();
                    Debug.Log("AlenAI Dialogue Triggered");
                }
            }
        }
    }
}