using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class ImaniFollowUpDialogue : MonoBehaviour
{
    public TMP_Text dialogueText; // The TMP text element to display dialogue.
    public Button path1Button; // Apologize/Action Button
    public Button path2Button; // Befriend/Action Button
    public Canvas dialogueCanvas; // The Canvas to hold the dialogue UI.
    public float detectionRadius = 5f; // Radius within which the player can trigger dialogue.

    private bool isInDialogueRange = false; // Whether the player is within range.
    private bool isDialogueActive = false; // Whether the dialogue is currently active.
    private bool isTyping = false; // Whether the text is currently being typed.
    private string fullSentence = ""; // Store the full sentence to display instantly if needed.

    public ImaniTrust imaniTrust; // Reference to the ImaniTrust script to check trust.
    public SecurityAI securityGuard; // Reference to SecurityGuards to stop chase.
    public ImaniDialogue firstDialogue; // Reference to the initial confrontation dialogue.

    private Transform playerTransform;
    private bool isDialogueCompleted = false; // Track if the initial dialogue is completed.

    private void Start()
    {
        // Initially hide the dialogue canvas.
        dialogueCanvas.gameObject.SetActive(false);

        // Assign button listeners.
        path1Button.onClick.AddListener(() => ChooseResponse(1));
        path2Button.onClick.AddListener(() => ChooseResponse(2));
    }

    private void Update()
    {
        // Check if the first dialogue is finished.
        if (!firstDialogue.enabled && imaniTrust != null && !isDialogueCompleted)
        {
            // Check player's distance from Imani.
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
            bool playerInRange = false;

            foreach (Collider collider in hitColliders)
            {
                if (collider.CompareTag("Player"))
                {
                    playerInRange = true;
                    playerTransform = collider.transform;
                    break;
                }
            }

            HandleDialogueDisplay(playerInRange);
        }

        // Skip typing effect if space is pressed.
        if (isTyping && Input.GetKeyDown(KeyCode.Space))
        {
            isTyping = false;
            dialogueText.text = fullSentence; // Show the full sentence immediately.
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        fullSentence = sentence; // Store the full sentence.
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            if (!isTyping) // If typing is interrupted, show the full sentence.
            {
                dialogueText.text = sentence;
                break;
            }
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f); // Typing speed.
        }

        isTyping = false;
    }

    private void HandleDialogueDisplay(bool playerInRange)
    {
        if (playerInRange && !isDialogueActive)
        {
            // Player entered the radius and dialogue is not yet active.
            isDialogueActive = true;
            ShowDialogue();
        }
        else if (!playerInRange && isDialogueActive)
        {
            // Player left the radius and dialogue is still active.
            ResetDialogue();
        }
    }

    public void ShowDialogue()
    {
        // Activate the dialogue UI and cursor.
        dialogueCanvas.gameObject.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Display the dialogue based on the trust level.
        if (imaniTrust.GetCurrentTrust() >= imaniTrust.maxTrust * 0.85f)
        {
            StartCoroutine(TypeSentence("Imani: Oh, back for more, my daring duo? What’s this—still chasing shadows, or did you just miss my radiant charm?"));
            SetButtonLabels("Oops, my bad!", "Let’s team up!");
        }
        else
        {
            StartCoroutine(TypeSentence("Imani: Ha! You again? Scramble off, darlings—my sunshine’s not for sale today!"));
            SetButtonLabels("Sorry, I’ll behave!", "Reserved for later.");
        }
    }

    private void HideDialogue()
    {
        // Deactivate the dialogue UI and cursor.
        dialogueCanvas.gameObject.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Reset dialogue state.
        isDialogueActive = false;
    }

    private void ResetDialogue()
    {
        if (!isDialogueCompleted)
        {
            // Reset trust and dialogue state if the dialogue is not completed.
            imaniTrust.ResetTrust();
            HideDialogue();
        }
    }

    private void SetButtonLabels(string path1Text, string path2Text)
    {
        // Ensure the buttons have TMP_Text components and set the labels.
        path1Button.GetComponentInChildren<TMP_Text>().text = path1Text;
        path2Button.GetComponentInChildren<TMP_Text>().text = path2Text;
    }

    private void ChooseResponse(int path)
    {
        if (path == 1)
        {
            StartCoroutine(TypeSentence("David: Oops, my bad! Didn’t mean to stumble into your glow again, Imani."));
        }
        else if (path == 2 && imaniTrust.GetCurrentTrust() >= imaniTrust.maxTrust * 0.85f)
        {
            StartCoroutine(TypeSentence("Matias: Hey, let’s team up! Two heroes and a wise star—unstoppable, right?"));
        }

        if (path == 2 && imaniTrust.GetCurrentTrust() >= imaniTrust.maxTrust * 0.85f && securityGuard.isChasingPlayer)
        {
            // If the player befriends Imani during a chase, stop the chase.
            StopChase();
        }

        // Mark the dialogue as completed.
        isDialogueCompleted = true;

        // Hide the dialogue after a response.
        Invoke("HideDialogue", 3f); // Wait for 3 seconds before hiding.
    }

    private void StopChase()
    {
        if (securityGuard != null)
        {
            securityGuard.DoNotChase(); // Stop the guard from chasing the player.
            StartCoroutine(TypeSentence("Imani: Oh, fine, you win me over! I’ll whistle the shadows away—stay sharp, loves!"));
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the detection radius in the Scene view.
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}