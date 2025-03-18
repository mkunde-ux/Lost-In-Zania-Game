using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class DalilaDialogue : MonoBehaviour
{
    //Check Adila dialogue script for comments

    [Header("UI References")]
    public TMP_Text dialogueText;
    public Button path1Button, path2Button, path3Button, path4Button;
    public Slider timerSlider;
    [SerializeField] private Canvas dialogueCanvas;

    [Header("Dialogue Settings")]
    public int maxDialogueLayer = 8;
    public float timer = 15f;
    private bool isTimerRunning = false;

    [Header("Trust System")]
    public DalilaTrust dalilaTrust;
    public UnityEvent onDialogueEnd;

    [Header("Audio Settings (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip typeSoundClip;
    [SerializeField] private AudioClip buttonPressClip;

    private bool isTyping = false;
    private string fullSentence = "";
    private bool isPlayerResponseComplete = false;
    private int dialogueLayer = 1;
    private bool isPlayerTurn = false;
    private bool isDialogueCompleted = false;
    private bool isPlayerInRange = false;
    private bool interactionLatched = false;

    public Canvas DialogueCanvas => dialogueCanvas;

    void Start()
    {
        path1Button.onClick.AddListener(() => ChoosePath(1));
        path2Button.onClick.AddListener(() => ChoosePath(2));
        path3Button.onClick.AddListener(() => ChoosePath(3));
        path4Button.onClick.AddListener(() => ChoosePath(4));

        timerSlider.maxValue = timer;
        timerSlider.value = timer;
        dialogueCanvas.gameObject.SetActive(false);
    }

    public void ResetDialogue()
    {
        StopAllCoroutines();
        dialogueLayer = 1;
        isDialogueCompleted = false;
        isPlayerTurn = false;
        StartDialogue();
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && !dialogueCanvas.gameObject.activeSelf)
        {
            StartDialogue();
        }

        if (isTyping && Input.GetKeyDown(KeyCode.Space))
        {
            isTyping = false;
            dialogueText.text = fullSentence;
        }

        if (isPlayerResponseComplete && Input.GetKeyDown(KeyCode.Space))
        {
            isPlayerResponseComplete = false;
            dialogueLayer++;
            isPlayerTurn = false;
            SetDialogueLayer();
        }

        if (isTimerRunning)
        {
            timer -= Time.deltaTime;
            timerSlider.value = timer;
            if (timer <= 0f)
            {
                isTimerRunning = false;
                OnTimerExpired();
            }
        }
    }

    void OnTimerExpired()
    {
        if (dalilaTrust != null)
        {
            dalilaTrust.AdjustTrust(-10);
        }
        StartCoroutine(TypeSentence("Dalila: \"Oh, darling, don’t just stand there gawking say something, or I’ll assume you’re plotting against me!\""));
        ProceedToNextLayer();
    }

    void StartTimer()
    {
        timer = 15f;
        timerSlider.value = timer;
        isTimerRunning = true;
    }

    void StopTimer()
    {
        isTimerRunning = false;
    }

    public void StartDialogue()
    {
        if (isDialogueCompleted) return;
        dialogueCanvas.gameObject.SetActive(true);
        Debug.Log("Dialogue canvas activated.");
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        isPlayerTurn = false;
        SetDialogueLayer();
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        fullSentence = sentence;
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }
        else
        {
            Debug.LogError("dialogueText is null!");
            yield break;
        }
        foreach (char letter in sentence.ToCharArray())
        {
            if (!isTyping)
            {
                dialogueText.text = sentence;
                break;
            }

            dialogueText.text += letter;

            if (audioSource != null && typeSoundClip != null && letter != ' ')
            {
                audioSource.PlayOneShot(typeSoundClip);
            }

            yield return new WaitForSeconds(0.05f);
        }
        isTyping = false;
        if (isPlayerTurn)
            isPlayerResponseComplete = true;
    }

    public bool IsDialogueComplete()
    {
        return isDialogueCompleted;
    }

    public void SetPlayerInRange(bool inRange)
    {
        isPlayerInRange = inRange;
        Debug.Log("Player in range: " + inRange);

        if (inRange)
        {
            interactionLatched = true;
        }
        else
        {
            interactionLatched = false;
            if (dialogueCanvas.gameObject.activeSelf)
            {
                EndDialogue();
            }
        }
    }

    void SetDialogueLayer()
    {
        if (!isPlayerTurn)
        {
            if (dialogueLayer > maxDialogueLayer)
            {
                EndDialogue();
                return;
            }

            switch (dialogueLayer)
            {
                case 1:
                    StartCoroutine(TypeSentence("Dalila: \"Well, hello there, my dears! I’m Dalila Malamu, legal counsel for the Zuwena Family. I heard about your little… phone mishap. Care to chat about it?\""));
                    break;
                case 2:
                    StartCoroutine(TypeSentence("Dalila: \"Oh, don’t look so glum! It’s just a phone or two. But let’s keep this quiet, hmm? No need to stir up trouble at this lovely restaurant. What do you say?\""));
                    break;
                case 3:
                    StartCoroutine(TypeSentence("Dalila: \"Charming boys like you must have seen something in that market. Tell me, anything odd catch your eye? I’m dying to know strictly off the record, of course!\""));
                    break;
                case 4:
                    StartCoroutine(TypeSentence("Dalila: \"Come now, don’t be shy! I’m trying to help you and the Zuwena Family’s pristine reputation. Any whispers about those thieves? ‘Oops! It’s ours now,’ was it?\""));
                    break;
                case 5:
                    StartCoroutine(TypeSentence("Dalila: \"You’re sharper than you look, aren’t you? I suspect a certain someone’s behind this, but I need proof. Seen anyone sneaky around here tonight?\""));
                    break;
                case 6:
                    StartCoroutine(TypeSentence("Dalila: \"Oh, I adore a good mystery! But I’m not here to play games well, not entirely. What do you know about the staff here? Any odd behavior?\""));
                    break;
                case 7:
                    StartCoroutine(TypeSentence("Dalila: \"Time’s ticking, darlings! Security’s sniffing around tomorrow, but I’d rather wrap this up tonight. Give me something useful or are you hiding it?\""));
                    break;
                case 8:
                    StartCoroutine(TypeSentence("Dalila: \"Last chance, boys! I’m a busy woman with a reputation to uphold. Spill something juicy about those thieves, or I’ll assume you’re no help at all!\""));
                    break;
            }
            StartCoroutine(WaitForNPCDialogue());
        }
    }

    IEnumerator WaitForNPCDialogue()
    {
        yield return new WaitUntil(() => !isTyping);
        yield return new WaitForSeconds(0.5f);
        SetPlayerTurn();
    }

    void SetPlayerTurn()
    {
        isPlayerTurn = true;
        switch (dialogueLayer)
        {
            case 1:
                SetButtonLabels("David: Just here to enjoy Tanzania, ma’am!",
                                "Matias: Yeah, until some jerks stole our phones!",
                                "David: We’d rather not talk about it, if that’s okay.",
                                "Matias: You’re a lawyer? What’s this got to do with you?");
                break;
            case 2:
                SetButtonLabels("David: Sure, we’ll keep it quiet for now.",
                                "Matias: Quiet? Our phones are gone someone’s gotta pay!",
                                "David: Why’s this place so important to you?",
                                "Matias: What’s in it for us if we stay hush?");
                break;
            case 3:
                SetButtonLabels("David: Just the usual market chaos nothing special.",
                                "Matias: Odd? Yeah, us getting robbed!",
                                "David: Didn’t see much too busy panicking!",
                                "Matias: Why’re you so nosy about it?");
                break;
            case 4:
                SetButtonLabels("David: Haven’t heard a thing sorry!",
                                "Matias: ‘Oops! It’s ours now’? That’s all we know!",
                                "David: Maybe some shady folks, but no names.",
                                "Matias: You know more than you’re letting on, don’t you?");
                break;
            case 5:
                SetButtonLabels("David: Sneaky? Not really just normal diners.",
                                "Matias: Proof? We’re the victims here, lady!",
                                "David: Saw a guy lurking near the bar maybe?",
                                "Matias: Why don’t you tell us who you suspect?");
                break;
            case 6:
                SetButtonLabels("David: Staff seemed fine busy, that’s all.",
                                "Matias: Odd behavior? You’re the odd one here!",
                                "David: One waiter was acting nervous could be nothing.",
                                "Matias: What’s your game, Dalila? Spill it!");
                break;
            case 7:
                SetButtonLabels("David: Useful? I’ve got nothing solid yet!",
                                "Matias: We’re not your detectives figure it out!",
                                "David: Heard a name Juma near the kitchen.",
                                "Matias: Hiding? Nah, we just don’t trust you!");
                break;
            case 8:
                SetButtonLabels("David: I’ll tell you what I saw just leave us out!",
                                "Matias: Fine, saw a guy with a scar good enough?",
                                "David: Nothing more can we go now?",
                                "Matias: You’re on your own, Dalila deal with it!");
                break;
        }
        path1Button.gameObject.SetActive(true);
        path2Button.gameObject.SetActive(true);
        path3Button.gameObject.SetActive(true);
        path4Button.gameObject.SetActive(true);
        StartTimer();
    }

    void SetButtonLabels(string path1Text, string path2Text, string path3Text, string path4Text)
    {
        path1Button.GetComponentInChildren<TMP_Text>().text = path1Text;
        path2Button.GetComponentInChildren<TMP_Text>().text = path2Text;
        path3Button.GetComponentInChildren<TMP_Text>().text = path3Text;
        path4Button.GetComponentInChildren<TMP_Text>().text = path4Text;
    }

    void ChoosePath(int path)
    {
        if (audioSource != null && buttonPressClip != null)
        {
            audioSource.PlayOneShot(buttonPressClip);
        }

        StopTimer();
        if (timer > 0f && dalilaTrust != null)
        {
            Debug.Log("Quick reply! Dalila appreciates efficiency gain 5 trust.");
            dalilaTrust.AdjustTrust(5);
        }

        path1Button.gameObject.SetActive(false);
        path2Button.gameObject.SetActive(false);
        path3Button.gameObject.SetActive(false);
        path4Button.gameObject.SetActive(false);

        StartCoroutine(HandleLayerWithResponse(path));
    }

    IEnumerator HandleLayerWithResponse(int path)
    {
        switch (dialogueLayer)
        {
            case 1: yield return StartCoroutine(HandleLayer1(path)); break;
            case 2: yield return StartCoroutine(HandleLayer2(path)); break;
            case 3: yield return StartCoroutine(HandleLayer3(path)); break;
            case 4: yield return StartCoroutine(HandleLayer4(path)); break;
            case 5: yield return StartCoroutine(HandleLayer5(path)); break;
            case 6: yield return StartCoroutine(HandleLayer6(path)); break;
            case 7: yield return StartCoroutine(HandleLayer7(path)); break;
            case 8: yield return StartCoroutine(HandleLayer8(path)); break;
        }
        yield return new WaitForSeconds(3);
        AdjustTrustBasedOnPath(path);
        yield return StartCoroutine(DalilaResponse(path));
        ProceedToNextLayer();
    }

    void ProceedToNextLayer()
    {
        dialogueLayer++;
        if (dialogueLayer > maxDialogueLayer)
            EndDialogue();
        else
            SetDialogueLayer();
    }

    void AdjustTrustBasedOnPath(int path)
    {
        if (dalilaTrust == null) return;

        switch (dialogueLayer)
        {
            case 1:
                if (path == 1) dalilaTrust.AdjustTrust(3);
                else if (path == 2) dalilaTrust.AdjustTrust(5);
                else if (path == 3) dalilaTrust.AdjustTrust(2);
                else if (path == 4) dalilaTrust.AdjustTrust(-2);
                break;
            case 2:
                if (path == 1) dalilaTrust.AdjustTrust(7);
                else if (path == 2) dalilaTrust.AdjustTrust(-5);
                else if (path == 3) dalilaTrust.AdjustTrust(-3);
                else if (path == 4) dalilaTrust.AdjustTrust(4);
                break;
            case 3:
                if (path == 1) dalilaTrust.AdjustTrust(0);
                else if (path == 2) dalilaTrust.AdjustTrust(-2);
                else if (path == 3) dalilaTrust.AdjustTrust(-1);
                else if (path == 4) dalilaTrust.AdjustTrust(-4);
                break;
            case 4:
                if (path == 1) dalilaTrust.AdjustTrust(-2);
                else if (path == 2) dalilaTrust.AdjustTrust(3);
                else if (path == 3) dalilaTrust.AdjustTrust(6);
                else if (path == 4) dalilaTrust.AdjustTrust(-5);
                break;
            case 5:
                if (path == 1) dalilaTrust.AdjustTrust(0);
                else if (path == 2) dalilaTrust.AdjustTrust(-3);
                else if (path == 3) dalilaTrust.AdjustTrust(8);
                else if (path == 4) dalilaTrust.AdjustTrust(-6);
                break;
            case 6:
                if (path == 1) dalilaTrust.AdjustTrust(2);
                else if (path == 2) dalilaTrust.AdjustTrust(-4);
                else if (path == 3) dalilaTrust.AdjustTrust(7);
                else if (path == 4) dalilaTrust.AdjustTrust(-3);
                break;
            case 7:
                if (path == 1) dalilaTrust.AdjustTrust(-1);
                else if (path == 2) dalilaTrust.AdjustTrust(-5);
                else if (path == 3) dalilaTrust.AdjustTrust(10);
                else if (path == 4) dalilaTrust.AdjustTrust(-2);
                break;
            case 8:
                if (path == 1) dalilaTrust.AdjustTrust(5);
                else if (path == 2) dalilaTrust.AdjustTrust(12);
                else if (path == 3) dalilaTrust.AdjustTrust(0);
                else if (path == 4) dalilaTrust.AdjustTrust(-8);
                break;
        }
    }

    IEnumerator DalilaResponse(int path)
    {
        if (dalilaTrust == null) yield break;

        int trustChange = dalilaTrust.GetLastTrustChange();
        string[] positiveResponses = {
            "Dalila: \"Oh, you’re delightful! I knew you’d be useful!\"",
            "Dalila: \"Perfect, darlings! You’re making my night!\"",
            "Dalila: \"Brilliant! Keep that up, and we’ll get along famously!\""
        };
        string[] negativeResponses = {
            "Dalila: \"Ugh, really? I expected more from you two!\"",
            "Dalila: \"How dull! Don’t waste my time with that nonsense!\"",
            "Dalila: \"Pathetic! You’re testing my patience, boys!\""
        };

        if (trustChange > 0)
            yield return StartCoroutine(TypeSentence(positiveResponses[Random.Range(0, positiveResponses.Length)]));
        else if (trustChange < 0)
            yield return StartCoroutine(TypeSentence(negativeResponses[Random.Range(0, negativeResponses.Length)]));
    }

    IEnumerator HandleLayer1(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Just here to enjoy Tanzania, ma’am!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Yeah, until some jerks stole our phones!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"We’d rather not talk about it, if that’s okay.\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"You’re a lawyer? What’s this got to do with you?\""));
    }

    IEnumerator HandleLayer2(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Sure, we’ll keep it quiet for now.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Quiet? Our phones are gone someone’s gotta pay!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Why’s this place so important to you?\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"What’s in it for us if we stay hush?\""));
    }

    IEnumerator HandleLayer3(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Just the usual market chaos nothing special.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Odd? Yeah, us getting robbed!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Didn’t see much too busy panicking!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Why’re you so nosy about it?\""));
    }

    IEnumerator HandleLayer4(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Haven’t heard a thing sorry!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"‘Oops! It’s ours now’? That’s all we know!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Maybe some shady folks, but no names.\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"You know more than you’re letting on, don’t you?\""));
    }

    IEnumerator HandleLayer5(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Sneaky? Not really just normal diners.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Proof? We’re the victims here, lady!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Saw a guy lurking near the bar maybe?\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Why don’t you tell us who you suspect?\""));
    }

    IEnumerator HandleLayer6(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Staff seemed fine busy, that’s all.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Odd behavior? You’re the odd one here!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"One waiter was acting nervous could be nothing.\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"What’s your game, Dalila? Spill it!\""));
    }

    IEnumerator HandleLayer7(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Useful? I’ve got nothing solid yet!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"We’re not your detectives figure it out!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Heard a name Juma near the kitchen.\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Hiding? Nah, we just don’t trust you!\""));
    }

    IEnumerator HandleLayer8(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"I’ll tell you what I saw just leave us out!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Fine, saw a guy with a scar good enough?\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Nothing more can we go now?\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"You’re on your own, Dalila deal with it!\""));
    }

    public void EndDialogue()
    {
        if (dialogueLayer > maxDialogueLayer)
        {
            path1Button.gameObject.SetActive(false);
            path2Button.gameObject.SetActive(false);
            path3Button.gameObject.SetActive(false);
            path4Button.gameObject.SetActive(false);
            dialogueCanvas.gameObject.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            if (dalilaTrust != null)
            {
                dalilaTrust.CheckTrustLevelAtEnd();
            }
            else
            {
                Debug.LogError("DalilaTrust reference missing!");
            }
            onDialogueEnd?.Invoke();
            isDialogueCompleted = true;
        }
    }
}