using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AlenConDialogue : MonoBehaviour
{
    //Check Michael Dialogue script for comments

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

    public AlenAI securityGuards;

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

            if (distanceToPlayer <= dialogueTriggerRadius && wealthyGirl.IsLowTrustConditionMet() && !securityGuards.isChasingPlayer && securityGuards.currentGuardState != AlenAI.GuardState.Dialogue && securityGuards.currentGuardState != AlenAI.GuardState.Chasing)
            {
                ShowDialogue();
            }
        }
        else
        {
            Debug.LogError("WealthyGirl or Player Transform not assigned in AlenConDialogue!");
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
                StartCoroutine(TypeSentence("Alen: I've seen things you wouldn't believe. I'm telling you this for your own good, just stay out of trouble."));
                SetButtonLabels("I understand, I'll be careful.", "Are you threatening me?", "I'm not looking for trouble.");
                break;
            case 2:
                StartCoroutine(TypeSentence("Alen: You're pushing your luck. I've given you a fair warning. Don't make me regret it."));
                SetButtonLabels("I'm leaving now.", "You're taking this too seriously.", "I'll try to stay out of your way.");
                break;
            case 3:
                StartCoroutine(TypeSentence("Alen: Enough. I've had enough of this. You've made your choice."));
                SetButtonLabels("This is a misunderstanding.", "You can't stop me.", "I'm not going to argue with you.");
                break;
            default:
                StartCoroutine(TypeSentence("Alen: You forced my hand. Now, you'll face the consequences."));
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
                securityGuards.currentGuardState = AlenAI.GuardState.Patrolling;
            }
        }
    }

    private void InitiateChaseSequence()
    {
        Debug.Log("Alen: The chase begins.");

        if (securityGuards != null)
        {
            securityGuards.StartChasingPlayer(playerTransform);
        }
        else
        {
            Debug.LogError("SecurityGuards script not assigned in AlenConDialogue!");
        }
    }

    void HandleLayer1(int path)
    {
        if (path == 1)
        {
            dialogueText.text = "Player: I understand, I'll be careful.";
        }
        else if (path == 2)
        {
            dialogueText.text = "Player: Are you threatening me?";
            provokeCount++;
        }
        else
        {
            dialogueText.text = "Player: I'm not looking for trouble.";
        }
    }

    void HandleLayer2(int path)
    {
        if (path == 1)
        {
            dialogueText.text = "Player: I'm leaving now.";
        }
        else if (path == 2)
        {
            dialogueText.text = "Player: You're taking this too seriously.";
            provokeCount++; 
        }
        else
        {
            dialogueText.text = "Player: I'll try to stay out of your way.";
        }
    }

    void HandleLayer3(int path)
    {
        if (path == 1)
        {
            dialogueText.text = "Player: This is a misunderstanding.";
            securityGuards.ResumePatrolling();
        }
        else if (path == 2)
        {
            dialogueText.text = "Player: You can't stop me.";
            provokeCount++; 
            InitiateChaseSequence(); 
        }
        else
        {
            dialogueText.text = "Player: I'm not going to argue with you.";
        }
    }
}