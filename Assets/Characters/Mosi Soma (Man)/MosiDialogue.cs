using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class MosiDialogue : MonoBehaviour
{
    //Check Adila dialogue script for comments

    [Header("UI References")]
    public TMP_Text dialogueText;
    public Button path1Button, path2Button, path3Button, path4Button;
    public Slider timerSlider;
    [SerializeField] private Canvas dialogueCanvas;
    public Canvas DialogueCanvas => dialogueCanvas;

    [Header("Dialogue Settings")]
    public int maxDialogueLayer = 8;
    public float timer = 15f;
    private bool isTimerRunning = false;

    [Header("Trust System")]
    public MosiTrust mosiTrust; 
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
        if (mosiTrust != null)
        {
            mosiTrust.AdjustTrust(-10); 
        }
        StartCoroutine(TypeSentence("Mosi: \"Oh, darling, don’t just stand there gawking say something, or I’ll assume you’re plotting against me!\""));
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
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        isPlayerTurn = false;
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
            if (letter != ' ' && audioSource != null && typeSoundClip != null)
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
        if (!inRange && dialogueCanvas.gameObject.activeSelf)
        {
            EndDialogue();
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
                    StartCoroutine(TypeSentence("Mosi: \"Welcome to Nasfi Na Lada, boys! I’m Mosi, head chef best in Zania, if I say so myself. You look lost what brings you here tonight?\""));
                    break;
                case 2:
                    StartCoroutine(TypeSentence("Mosi: \"Phone thieves, huh? Sounds like a messy tale. We’re about to close up, though maybe try that dive down the street instead?\""));
                    break;
                case 3:
                    StartCoroutine(TypeSentence("Mosi: \"Look, I’ve got enough on my plate with this charity nonsense. What exactly did you see at that market? Make it quick!\""));
                    break;
                case 4:
                    StartCoroutine(TypeSentence("Mosi: \"Oh, spare me the detective act! I run a kitchen, not a crime scene. You think those thieves are here? What’s your proof?\""));
                    break;
                case 5:
                    StartCoroutine(TypeSentence("Mosi: \"You’re pushing it, kids. I’ve got a reputation to keep. Seen anything odd around my restaurant tonight or are you just guessing?\""));
                    break;
                case 6:
                    StartCoroutine(TypeSentence("Mosi: \"I’m not here to play hide-and-seek with thieves! My staff’s too busy for this. Anyone acting strange speak up or get out!\""));
                    break;
                case 7:
                    StartCoroutine(TypeSentence("Mosi: \"Time’s up, boys! I’ve got a kitchen to run. Give me something useful about those crooks, or take your whining elsewhere!\""));
                    break;
                case 8:
                    StartCoroutine(TypeSentence("Mosi: \"Last chance, you little pests! I’m not your babysitter. Got real dirt on those thieves, or are you wasting my night?\""));
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
                SetButtonLabels("David: \"I’m David, helping my aunt. Heard the phone thieves work here.\"",
                                "Matias: \"Matias here. Our phones got jacked someone said check this place!\"",
                                "David: \"Just passing through didn’t expect trouble.\"",
                                "Matias: \"You’re the big chef why’s this place a thief hangout?\"");
                break;
            case 2:
                SetButtonLabels("David: \"We’re not leaving heard the thieves are here.\"",
                                "Matias: \"Close? Nah, we’re staying till we figure this out!\"",
                                "David: \"Why push us away? Something to hide?\"",
                                "Matias: \"What’s with the dodge? You know these crooks?\"");
                break;
            case 3:
                SetButtonLabels("David: \"Just a busy market didn’t see much.\"",
                                "Matias: \"Some punk swiped our phones fast as hell!\"",
                                "David: \"Too chaotic couldn’t catch a good look.\"",
                                "Matias: \"Why’s it your problem? You covering for someone?\"");
                break;
            case 4:
                SetButtonLabels("David: \"No proof yet just a tip we got.\"",
                                "Matias: \"Tip said they’re here deal with it, chef!\"",
                                "David: \"Heard they sell stuff through the back maybe?\"",
                                "Matias: \"You tell us your kitchen’s shady, right?\"");
                break;
            case 5:
                SetButtonLabels("David: \"Nothing weird just fancy diners.\"",
                                "Matias: \"Odd? Yeah, you acting all jumpy!\"",
                                "David: \"Saw a guy by the kitchen door suspicious vibe.\"",
                                "Matias: \"What’s your deal? Hiding thieves in your crew?\"");
                break;
            case 6:
                SetButtonLabels("David: \"Staff looks fine busy like you said.\"",
                                "Matias: \"Strange? You’re the strangest guy here, boss!\"",
                                "David: \"One cook kept looking around nervous maybe.\"",
                                "Matias: \"Why so defensive? Your staff in on it?\"");
                break;
            case 7:
                SetButtonLabels("David: \"Nothing solid yet still looking.\"",
                                "Matias: \"We’re not your errand boys find ‘em yourself!\"",
                                "David: \"Heard a name Zuberi near the pantry.\"",
                                "Matias: \"You’re the chef why’s this our job?\"");
                break;
            case 8:
                SetButtonLabels("David: \"Saw a guy sneak out back keep us out of it!\"",
                                "Matias: \"Yeah, dude with a cap good enough for you?\"",
                                "David: \"That’s all we know let us eat in peace!\"",
                                "Matias: \"Handle it, chef stop bugging us!\"");
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
        if (timer > 0f && mosiTrust != null)
        {
            Debug.Log("Quick reply! Mosi appreciates efficiency gain 5 trust.");
            mosiTrust.AdjustTrust(5); 
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
        yield return StartCoroutine(MosiResponse(path));
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
        if (mosiTrust == null) return;
        switch (dialogueLayer)
        {
            case 1:
                if (path == 1) mosiTrust.AdjustTrust(3);
                else if (path == 2) mosiTrust.AdjustTrust(2);
                else if (path == 3) mosiTrust.AdjustTrust(0);
                else if (path == 4) mosiTrust.AdjustTrust(-4);
                break;
            case 2:
                if (path == 1) mosiTrust.AdjustTrust(2);
                else if (path == 2) mosiTrust.AdjustTrust(4);
                else if (path == 3) mosiTrust.AdjustTrust(-2);
                else if (path == 4) mosiTrust.AdjustTrust(-5);
                break;
            case 3:
                if (path == 1) mosiTrust.AdjustTrust(0);
                else if (path == 2) mosiTrust.AdjustTrust(1);
                else if (path == 3) mosiTrust.AdjustTrust(-1);
                else if (path == 4) mosiTrust.AdjustTrust(-6);
                break;
            case 4:
                if (path == 1) mosiTrust.AdjustTrust(0);
                else if (path == 2) mosiTrust.AdjustTrust(3);
                else if (path == 3) mosiTrust.AdjustTrust(5);
                else if (path == 4) mosiTrust.AdjustTrust(-4);
                break;
            case 5:
                if (path == 1) mosiTrust.AdjustTrust(1);
                else if (path == 2) mosiTrust.AdjustTrust(-3);
                else if (path == 3) mosiTrust.AdjustTrust(6);
                else if (path == 4) mosiTrust.AdjustTrust(-5);
                break;
            case 6:
                if (path == 1) mosiTrust.AdjustTrust(2);
                else if (path == 2) mosiTrust.AdjustTrust(-4);
                else if (path == 3) mosiTrust.AdjustTrust(7);
                else if (path == 4) mosiTrust.AdjustTrust(-3);
                break;
            case 7:
                if (path == 1) mosiTrust.AdjustTrust(-1);
                else if (path == 2) mosiTrust.AdjustTrust(-5);
                else if (path == 3) mosiTrust.AdjustTrust(9);
                else if (path == 4) mosiTrust.AdjustTrust(-2);
                break;
            case 8:
                if (path == 1) mosiTrust.AdjustTrust(6);
                else if (path == 2) mosiTrust.AdjustTrust(10);
                else if (path == 3) mosiTrust.AdjustTrust(0);
                else if (path == 4) mosiTrust.AdjustTrust(-7);
                break;
        }
    }

    IEnumerator MosiResponse(int path)
    {
        if (mosiTrust == null) yield break;

        int trustChange = mosiTrust.GetLastTrustChange();
        string[] positiveResponses = new string[]
        {
            "Mosi: \"Well, aren’t you helpful? I might keep you around!\"",
            "Mosi: \"Good taste, boys maybe you’re not useless after all!\"",
            "Mosi: \"Fine, you’ve got my attention. Let’s see what cooks!\""
        };
        string[] negativeResponses = new string[]
        {
            "Mosi: \"Ugh, you’re as dull as yesterday’s stew!\"",
            "Mosi: \"Get out of my kitchen with that nonsense!\"",
            "Mosi: \"You’re trying my patience beat it, pests!\""
        };

        if (trustChange > 0)
            yield return StartCoroutine(TypeSentence(positiveResponses[Random.Range(0, positiveResponses.Length)]));
        else if (trustChange < 0)
            yield return StartCoroutine(TypeSentence(negativeResponses[Random.Range(0, negativeResponses.Length)]));
    }

    IEnumerator HandleLayer1(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"I’m David, helping my aunt. Heard the phone thieves work here.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Matias here. Our phones got jacked someone said check this place!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Just passing through didn’t expect trouble.\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"You’re the big chef why’s this place a thief hangout?\""));
    }

    IEnumerator HandleLayer2(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"We’re not leaving heard the thieves are here.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Close? Nah, we’re staying till we figure this out!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Why push us away? Something to hide?\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"What’s with the dodge? You know these crooks?\""));
    }

    IEnumerator HandleLayer3(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Just a busy market didn’t see much.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Some punk swiped our phones fast as hell!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Too chaotic couldn’t catch a good look.\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Why’s it your problem? You covering for someone?\""));
    }

    IEnumerator HandleLayer4(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"No proof yet just a tip we got.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Tip said they’re here deal with it, chef!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Heard they sell stuff through the back maybe?\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"You tell us your kitchen’s shady, right?\""));
    }

    IEnumerator HandleLayer5(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Nothing weird just fancy diners.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Odd? Yeah, you acting all jumpy!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Saw a guy by the kitchen door suspicious vibe.\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"What’s your deal? Hiding thieves in your crew?\""));
    }

    IEnumerator HandleLayer6(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Staff looks fine busy like you said.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Strange? You’re the strangest guy here, boss!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"One cook kept looking around nervous maybe.\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Why so defensive? Your staff in on it?\""));
    }

    IEnumerator HandleLayer7(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Nothing solid yet still looking.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"We’re not your errand boys find ‘em yourself!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Heard a name Zuberi near the pantry.\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"You’re the chef why’s this our job?\""));
    }

    IEnumerator HandleLayer8(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Saw a guy sneak out back keep us out of it!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Yeah, dude with a cap good enough for you?\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"That’s all we know let us eat in peace!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Handle it, chef stop bugging us!\""));
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

            if (mosiTrust != null)
            {
                mosiTrust.CheckTrustLevelAtEnd();
            }
            else
            {
                Debug.LogError("MosiTrust reference missing!");
            }

            onDialogueEnd?.Invoke();
            isDialogueCompleted = true;
        }
    }
}