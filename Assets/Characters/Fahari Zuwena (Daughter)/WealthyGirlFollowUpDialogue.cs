using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class WealthyGirlFollowUpDialogue : MonoBehaviour
{
    public TMP_Text dialogueText; // The TMP text element to display dialogue.
    public Button path1Button; // Apologize Button ("Oh, I'm sorry").
    public Button path2Button; // Befriend Button ("Let's be friends").
    public Canvas dialogueCanvas; // The Canvas to hold the dialogue UI.
    public float detectionRadius = 5f; // Radius within which the player can trigger dialogue.

    private bool isInDialogueRange = false; // Whether the player is within range.
    private bool isDialogueActive = false; // Whether the dialogue is currently active.
    private bool isTyping = false; // Whether the text is currently being typed.
    private string fullSentence = ""; // Store the full sentence to display instantly if needed.

    public FahariTrust wealthyGirl; // Reference to the WealthyGirl script to check trust.
    public SecurityAI securityGuard; // Reference to SecurityGuards to stop chase.
    public FahariDialogue firstDialogue; // Reference to the initial confrontation dialogue.

    private Transform playerTransform;

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
        if (!firstDialogue.enabled && wealthyGirl != null)
        {
            // Check player's distance from the Wealthy Girl.
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
            HideDialogue();
        }
    }

    private void ShowDialogue()
    {
        // Activate the dialogue UI and cursor.
        dialogueCanvas.gameObject.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Display the dialogue based on the trust level.
        if (wealthyGirl.currentTrust >= wealthyGirl.maxTrust * 0.85f)
        {
            StartCoroutine(TypeSentence("Fahari: What! Do you still want to talk, or something? Shouldn’t you be asking about your stolen items?"));
            SetButtonLabels("Oh, sorry.", "Let's be friends.");
        }
        else
        {
            StartCoroutine(TypeSentence("Fahari: Go away, stop wasting my time!"));
            SetButtonLabels("Oh, I'm sorry.", "Reserved for later use.");
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
            StartCoroutine(TypeSentence("Player: Oh, I'm sorry, I didn’t mean to bump into you again."));
        }
        else if (path == 2 && wealthyGirl.currentTrust >= wealthyGirl.maxTrust * 0.85f)
        {
            StartCoroutine(TypeSentence("Player: Hey, let's be friends. Maybe we can help each other."));
        }

        if (path == 2 && wealthyGirl.currentTrust >= wealthyGirl.maxTrust * 0.85f && securityGuard.isChasingPlayer)
        {
            // If the player befriends the Wealthy Girl during a chase, stop the chase.
            StopChase();
        }

        // Hide the dialogue after a response.
        Invoke("HideDialogue", 3f); // Wait for 3 seconds before hiding.
    }

    private void StopChase()
    {
        if (securityGuard != null)
        {
            securityGuard.DoNotChase(); // Stop the guard from chasing the player.
            StartCoroutine(TypeSentence("Fahari: Fine. I’ll call Michael off. You better be careful."));
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the detection radius in the Scene view.
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}