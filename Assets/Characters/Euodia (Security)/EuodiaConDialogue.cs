using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EuodiaConDialogue : MonoBehaviour
{
    public TMP_Text dialogueText;
    public Button path1Button;
    public Button path2Button;
    public Button path3Button;

    private bool isTyping = false;
    private string fullSentence = "";
    private bool dialogueActive = false;

    [SerializeField] private Canvas dialogueCanvas;
    private int dialogueLayer = 1;
    private int provokeCount = 0;

    public EuodiaAI securityGuards;

    [Header("Dialogue Trigger Settings")]
    public float dialogueTriggerRadius = 3f;
    private Transform playerTransform;
    public FahariTrust wealthyGirl;

    private void Awake()
    {
        dialogueCanvas.gameObject.SetActive(false);
    }

    void Start()
    {
        path1Button.onClick.AddListener(() => ChoosePath(1));
        path2Button.onClick.AddListener(() => ChoosePath(2));
        path3Button.onClick.AddListener(() => ChoosePath(3));

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
        if (wealthyGirl != null && playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= dialogueTriggerRadius && wealthyGirl.IsLowTrustConditionMet() && !securityGuards.isChasingPlayer && securityGuards.currentGuardState != EuodiaAI.GuardState.Dialogue && securityGuards.currentGuardState != EuodiaAI.GuardState.Chasing)
            {
                ShowDialogue();
            }
        }
        else
        {
            Debug.LogError("WealthyGirl or Player Transform not assigned in EuodiaConDialogue!");
        }
    }

    public void ShowDialogue()
    {
        if (dialogueActive) return; 

        dialogueActive = true;
        dialogueCanvas.gameObject.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        ThirdPersonCam.isInteractingWithUI = true;

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
            yield return new WaitForSeconds(0.05f);
        }

        isTyping = false;
    }

    void SetDialogueLayer()
    {
        switch (dialogueLayer)
        {
            case 1:
                StartCoroutine(TypeSentence("Euodia: You think you're slick, huh? I've got my eye on you. Don't push your luck."));
                SetButtonLabels("I'm just minding my own business.", "Who do you think you are?", "I'm not doing anything wrong.");
                break;
            case 2:
                StartCoroutine(TypeSentence("Euodia: I warned you. Are you deliberately trying to get on my nerves?"));
                SetButtonLabels("I'm leaving.", "You're overreacting.", "Just relax, I'm not a threat.");
                break;
            case 3:
                StartCoroutine(TypeSentence("Euodia: That's it! You've crossed the line. You're going to regret this!"));
                SetButtonLabels("Fine, I'll go.", "Try and stop me!", "This is ridiculous.");
                break;
            default:
                StartCoroutine(TypeSentence("Euodia: You asked for it! Now you'll see what happens when you mess with me!"));
                EndDialogue();
                InitiateChaseSequence();
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
                InitiateChaseSequence();
            }
            else
            {
                securityGuards.ResumePatrolling();
                securityGuards.DisableChasing();
                securityGuards.currentGuardState = EuodiaAI.GuardState.Patrolling; 
            }
        }
    }

    private void InitiateChaseSequence()
    {
        Debug.Log("Euodia: The chase begins!");

        if (securityGuards != null)
        {
            securityGuards.StartChasingPlayer(playerTransform);
        }
        else
        {
            Debug.LogError("SecurityGuards script not assigned in EuodiaConDialogue!");
        }
    }

    void HandleLayer1(int path)
    {
        if (path == 1)
        {
            dialogueText.text = "Player: I'm just minding my own business.";
        }
        else if (path == 2)
        {
            dialogueText.text = "Player: Who do you think you are?";
            provokeCount++;
        }
        else
        {
            dialogueText.text = "Player: I'm not doing anything wrong.";
        }
    }

    void HandleLayer2(int path)
    {
        if (path == 1)
        {
            dialogueText.text = "Player: I'm leaving.";
        }
        else if (path == 2)
        {
            dialogueText.text = "Player: You're overreacting.";
            provokeCount++;
        }
        else
        {
            dialogueText.text = "Player: Just relax, I'm not a threat.";
        }
    }

    void HandleLayer3(int path)
    {
        if (path == 1)
        {
            dialogueText.text = "Player: Fine, I'll go.";
            securityGuards.ResumePatrolling(); 
        }
        else if (path == 2)
        {
            dialogueText.text = "Player: Try and stop me!";
            provokeCount++; 
            InitiateChaseSequence(); 
        }
        else
        {
            dialogueText.text = "Player: This is ridiculous.";
        }
    }
}