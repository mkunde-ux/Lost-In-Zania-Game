using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class MichaelSecurityDialogueScript : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text dialogueText; // Text element to display dialogue.
    public Button path1Button;    // Cooperative option.
    public Button path2Button;    // Provocative option.
    public Button path3Button;    // Reserved for future use.
    [SerializeField] private Canvas dialogueCanvas; // Dialogue canvas.

    [Header("Dialogue Trigger Settings")]
    public float dialogueTriggerRadius = 3f; // Radius within which dialogue can be triggered.
    public FahariTrust wealthyGirl;          // Trust component (example: wealthyGirl).

    [Header("Audio Settings (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip typeSoundClip;    // Sound played during typewriter effect.
    [SerializeField] private AudioClip buttonPressClip;    // Sound played on option selection.

    // Dialogue and state management.
    private bool isTyping = false;
    private string fullSentence = ""; // Stores full sentence for instant display if skipped.
    private bool dialogueActive = false; // Tracks whether dialogue is active.
    private int dialogueLayer = 1; // Current dialogue layer.
    private int provokeCount = 0;  // Tracks number of provocations (when player chooses Path 2).

    // References to other systems.
    public SecurityAI securityGuards; // Reference to Michael's SecurityAI script.
    private Transform playerTransform; // Cached reference to the player's transform.

    private void Awake()
    {
        dialogueCanvas.gameObject.SetActive(false); // Hide dialogue at start.
    }

    void Start()
    {
        // Assign button click events.
        path1Button.onClick.AddListener(() => ChoosePath(1));
        path2Button.onClick.AddListener(() => ChoosePath(2));
        path3Button.onClick.AddListener(() => ChoosePath(3));

        // Cache the player's transform.
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player object not found!");
        }
    }

    void Update()
    {
        // Check if conditions are met to trigger dialogue.
        if (wealthyGirl != null && playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= dialogueTriggerRadius &&
                wealthyGirl.IsLowTrustConditionMet() &&
                !securityGuards.isChasingPlayer &&
                securityGuards.currentGuardState != SecurityAI.GuardState.Dialogue &&
                securityGuards.currentGuardState != SecurityAI.GuardState.Chasing)
            {
                ShowDialogue();
            }
        }
        else
        {
            Debug.LogError("WealthyGirl or Player Transform not assigned in MichaelSecurityDialogueScript!");
        }
    }

    public void ShowDialogue()
    {
        if (dialogueActive) return; // Prevent multiple triggers.

        dialogueActive = true;
        dialogueCanvas.gameObject.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        ThirdPersonCam.isInteractingWithUI = true;

        // Move the security guard to the player and initiate dialogue.
        if (securityGuards != null)
        {
            securityGuards.MoveToPlayerAndStartDialogue();
        }
    }

    public void HideDialogue()
    {
        dialogueActive = false;
        dialogueCanvas.gameObject.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        ThirdPersonCam.isInteractingWithUI = false;
    }

    public void ResetDialogue()
    {
        dialogueLayer = 1;
        provokeCount = 0;
        // Reactivate all option buttons.
        path1Button.gameObject.SetActive(true);
        path2Button.gameObject.SetActive(true);
        path3Button.gameObject.SetActive(true);

        if (securityGuards != null && !securityGuards.isChasingPlayer)
        {
            securityGuards.ResumePatrolling();
        }

        SetDialogueLayer();
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        fullSentence = sentence;
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            if (!isTyping)
            {
                dialogueText.text = sentence;
                break;
            }
            dialogueText.text += letter;

            // Play typewriter sound for each non-space character.
            if (audioSource != null && typeSoundClip != null && letter != ' ')
            {
                audioSource.PlayOneShot(typeSoundClip);
            }

            yield return new WaitForSeconds(0.05f);
        }

        isTyping = false;
    }

    void SetDialogueLayer()
    {
        switch (dialogueLayer)
        {
            case 1:
                StartCoroutine(TypeSentence("Michael: Hey, I am watching you, don’t mess around or you won’t like how it ends."));
                SetButtonLabels("I won’t be messing around.", "It’s not my fault, blame them!", "Reserved for future use.");
                break;
            case 2:
                StartCoroutine(TypeSentence("Michael: I told you, keep it calm. Do we have a problem here?"));
                SetButtonLabels("No problem, I’ll be on my way.", "It’s them, not me causing trouble!", "Reserved for future use.");
                break;
            case 3:
                StartCoroutine(TypeSentence("Michael: You don’t want to test me. This is your last warning!"));
                SetButtonLabels("I’m leaving now.", "You won’t stop me!", "Reserved for future use.");
                break;
            default:
                StartCoroutine(TypeSentence("Michael: I gave you a chance, now face the consequences!"));
                EndDialogue();
                InitiateChaseSequence(); // Begin chase sequence.
                break;
        }
    }

    void SetButtonLabels(string path1Text, string path2Text, string path3Text)
    {
        path1Button.GetComponentInChildren<TMP_Text>().text = path1Text;
        path2Button.GetComponentInChildren<TMP_Text>().text = path2Text;
        path3Button.GetComponentInChildren<TMP_Text>().text = path3Text;
    }

    void ChoosePath(int path)
    {
        // Execute the appropriate response for the current dialogue layer.
        switch (dialogueLayer)
        {
            case 1:
                HandleLayer1(path);
                break;
            case 2:
                HandleLayer2(path);
                break;
            case 3:
                HandleLayer3(path);
                break;
        }

        // Advance dialogue or end dialogue.
        if (dialogueLayer < 3)
        {
            dialogueLayer++;
            SetDialogueLayer();
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        HideDialogue();

        if (securityGuards != null)
        {
            if (provokeCount >= 2)
            {
                InitiateChaseSequence(); // Start chasing if provoked enough.
            }
            else
            {
                securityGuards.ResumePatrolling();
                securityGuards.DisableChasing();
                securityGuards.currentGuardState = SecurityAI.GuardState.Patrolling; // Ensure guard resumes patrolling.
            }
        }
    }

    private void InitiateChaseSequence()
    {
        Debug.Log("Michael: The chase begins!");

        if (securityGuards != null)
        {
            securityGuards.StartChasingPlayer(playerTransform);
        }
        else
        {
            Debug.LogError("SecurityGuards script not assigned in MichaelSecurityDialogueScript!");
        }
    }

    void HandleLayer1(int path)
    {
        if (path == 1)
        {
            dialogueText.text = "Player: I won’t be messing around.";
        }
        else if (path == 2)
        {
            dialogueText.text = "Player: It’s not my fault, blame them!";
            provokeCount++; // Increase provocation count.
        }
        else
        {
            dialogueText.text = "Player: Reserved for future use.";
        }
    }

    void HandleLayer2(int path)
    {
        if (path == 1)
        {
            dialogueText.text = "Player: No problem, I’ll be on my way.";
        }
        else if (path == 2)
        {
            dialogueText.text = "Player: It’s them, not me causing trouble!";
            provokeCount++; // Increase provocation count.
        }
        else
        {
            dialogueText.text = "Player: Reserved for future use.";
        }
    }

    void HandleLayer3(int path)
    {
        if (path == 1)
        {
            dialogueText.text = "Player: I’m leaving now.";
            securityGuards.ResumePatrolling(); // Resume patrolling.
        }
        else if (path == 2)
        {
            dialogueText.text = "Player: You won’t stop me!";
            provokeCount++; // Final provocation.
            InitiateChaseSequence(); // Begin chase immediately.
        }
        else
        {
            dialogueText.text = "Player: Reserved for future use.";
        }
    }
}