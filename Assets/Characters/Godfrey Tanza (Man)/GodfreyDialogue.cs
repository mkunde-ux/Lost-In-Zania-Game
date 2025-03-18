using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class GodfreyDialogue : MonoBehaviour
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
    public GodfreyTrust godfreyTrust; 
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
        if (godfreyTrust != null)
        {
            godfreyTrust.AdjustTrust(-10);
        }
        StartCoroutine(TypeSentence("Godfrey: \"Oh, darling, don’t just stand there gawking—say something, or I’ll assume you’re plotting against me!\""));
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
        isDialogueCompleted = false;
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
                    StartCoroutine(TypeSentence("Godfrey: \"Well, well, look who’s wandered into the lion’s den! I’m Godfrey—yes, *the* Godfrey. You two don’t look like the usual Zuwena crowd. What’s your story?\""));
                    break;
                case 2:
                    StartCoroutine(TypeSentence("Godfrey: \"A phone theft, eh? Sounds like a petty little drama. Still, you made it here—impressive for a couple of nobodies. How’d you snag an invite?\""));
                    break;
                case 3:
                    StartCoroutine(TypeSentence("Godfrey: \"Hmph, I don’t care much for sob stories, but I’m curious—what’d you see at that market? Don’t bore me with the obvious!\""));
                    break;
                case 4:
                    StartCoroutine(TypeSentence("Godfrey: \"Fans or not, you’re in my spotlight now. I’m here for Sadiki, not you, but I’ll bite—what’s this ‘Greater Few’ nonsense you’re on about?\""));
                    break;
                case 5:
                    StartCoroutine(TypeSentence("Godfrey: \"Oh, please, don’t waste my time with vague answers! I’ve got a million followers waiting for my next move. Seen anything shady around here tonight?\""));
                    break;
                case 6:
                    StartCoroutine(TypeSentence("Godfrey: \"You’re testing my patience, boys. I’m a star, not a detective, but I’ll humor you—any odd characters lurking about? Spill it quick!\""));
                    break;
                case 7:
                    StartCoroutine(TypeSentence("Godfrey: \"Time’s ticking, and I’m not here for charity chats. Give me something worth hearing, or I’m off to dazzle someone more important!\""));
                    break;
                case 8:
                    StartCoroutine(TypeSentence("Godfrey: \"Last chance to impress me, peasants! I’ve got a stage to own. Got any dirt on those thieves, or are you just dead weight?\""));
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
                SetButtonLabels("David: \"I’m David, here to help my aunty. Love your song ‘Greater Few’!\"",
                                "Matias: \"Matias here. Got our phones snatched—now we’re crashing your party!\"",
                                "David: \"We’re just tourists caught up in some bad luck, that’s all.\"",
                                "Matias: \"Who cares about us? You’re the big shot—why talk to us?\"");
                break;
            case 2:
                SetButtonLabels("David: \"My aunty knows the Zuwena Family—she got us in.\"",
                                "Matias: \"Dumb luck, man! We’re not fancy like you.\"",
                                "David: \"Not sure, but we’re here now—pretty cool, right?\"",
                                "Matias: \"Why’s a star like you bothering with our invite?\"");
                break;
            case 3:
                SetButtonLabels("David: \"Just a crowded market—nothing special.\"",
                                "Matias: \"Some jerk ran off with our phones, that’s it!\"",
                                "David: \"Didn’t catch much—too busy freaking out.\"",
                                "Matias: \"What’s it to you? You gonna sing about it?\"");
                break;
            case 4:
                SetButtonLabels("David: \"‘Greater Few’ is your best track—huge fan!\"",
                                "Matias: \"Never heard it, but you’re famous, so whatever.\"",
                                "David: \"Just saying it’s awesome—thought you’d like to know.\"",
                                "Matias: \"Why’s Sadiki such a big deal to you, huh?\"");
                break;
            case 5:
                SetButtonLabels("David: \"Just fancy folks here—nothing weird.\"",
                                "Matias: \"Shady? Nah, just rich snobs like you!\"",
                                "David: \"Maybe a guy by the bar looked sketchy—dunno.\"",
                                "Matias: \"You tell us—what’s shady in your fancy world?\"");
                break;
            case 6:
                SetButtonLabels("David: \"Staff’s normal—busy with the party.\"",
                                "Matias: \"Odd? You’re the oddest thing here, superstar!\"",
                                "David: \"One waiter seemed jumpy—probably nothing.\"",
                                "Matias: \"Why’re you digging? Got a song idea or what?\"");
                break;
            case 7:
                SetButtonLabels("David: \"Nothing solid yet—sorry, man.\"",
                                "Matias: \"We’re not your spies—buzz off!\"",
                                "David: \"Heard someone mention a ‘Kweku’ by the door.\"",
                                "Matias: \"You’re the big shot—why need us?\"");
                break;
            case 8:
                SetButtonLabels("David: \"Saw some guy slip out back—keep us out of it!\"",
                                "Matias: \"Yeah, a dude with a limp—take it or leave it!\"",
                                "David: \"That’s all we’ve got—let us enjoy the party!\"",
                                "Matias: \"Figure it out yourself, Mr. Million Followers!\"");
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
        if (timer > 0f && godfreyTrust != null)
        {
            Debug.Log("Quick reply! Godfrey appreciates efficiency—gain 5 trust.");
            godfreyTrust.AdjustTrust(5); 
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
        yield return StartCoroutine(GodfreyResponse(path));
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
        if (godfreyTrust == null) return;

        switch (dialogueLayer)
        {
            case 1:
                if (path == 1) godfreyTrust.AdjustTrust(5);
                else if (path == 2) godfreyTrust.AdjustTrust(2);
                else if (path == 3) godfreyTrust.AdjustTrust(0);
                else if (path == 4) godfreyTrust.AdjustTrust(-3);
                break;
            case 2:
                if (path == 1) godfreyTrust.AdjustTrust(4);
                else if (path == 2) godfreyTrust.AdjustTrust(1);
                else if (path == 3) godfreyTrust.AdjustTrust(2);
                else if (path == 4) godfreyTrust.AdjustTrust(-4);
                break;
            case 3:
                if (path == 1) godfreyTrust.AdjustTrust(0);
                else if (path == 2) godfreyTrust.AdjustTrust(-1);
                else if (path == 3) godfreyTrust.AdjustTrust(-2);
                else if (path == 4) godfreyTrust.AdjustTrust(-5);
                break;
            case 4:
                if (path == 1) godfreyTrust.AdjustTrust(7);
                else if (path == 2) godfreyTrust.AdjustTrust(-2);
                else if (path == 3) godfreyTrust.AdjustTrust(3);
                else if (path == 4) godfreyTrust.AdjustTrust(-4);
                break;
            case 5:
                if (path == 1) godfreyTrust.AdjustTrust(0);
                else if (path == 2) godfreyTrust.AdjustTrust(-3);
                else if (path == 3) godfreyTrust.AdjustTrust(6);
                else if (path == 4) godfreyTrust.AdjustTrust(-5);
                break;
            case 6:
                if (path == 1) godfreyTrust.AdjustTrust(1);
                else if (path == 2) godfreyTrust.AdjustTrust(-4);
                else if (path == 3) godfreyTrust.AdjustTrust(5);
                else if (path == 4) godfreyTrust.AdjustTrust(-3);
                break;
            case 7:
                if (path == 1) godfreyTrust.AdjustTrust(-1);
                else if (path == 2) godfreyTrust.AdjustTrust(-6);
                else if (path == 3) godfreyTrust.AdjustTrust(8);
                else if (path == 4) godfreyTrust.AdjustTrust(-2);
                break;
            case 8:
                if (path == 1) godfreyTrust.AdjustTrust(6);
                else if (path == 2) godfreyTrust.AdjustTrust(10);
                else if (path == 3) godfreyTrust.AdjustTrust(0);
                else if (path == 4) godfreyTrust.AdjustTrust(-8);
                break;
        }
    }

    IEnumerator GodfreyResponse(int path)
    {
        if (godfreyTrust == null) yield break;

        int trustChange = godfreyTrust.GetLastTrustChange();
        string[] positiveResponses = new string[]
        {
            "Godfrey: \"Ha! You’ve got taste—I like that!\"",
            "Godfrey: \"Not bad, kids! You might just shine in my shadow!\"",
            "Godfrey: \"Well, well, you’re worth a verse or two!\""
        };
        string[] negativeResponses = new string[]
        {
            "Godfrey: \"Ugh, you’re duller than a broken mic!\"",
            "Godfrey: \"Pathetic—you’re wasting my spotlight!\"",
            "Godfrey: \"Get lost, peasants—I’ve got better fans!\""
        };

        if (trustChange > 0)
            yield return StartCoroutine(TypeSentence(positiveResponses[Random.Range(0, positiveResponses.Length)]));
        else if (trustChange < 0)
            yield return StartCoroutine(TypeSentence(negativeResponses[Random.Range(0, negativeResponses.Length)]));
    }

    IEnumerator HandleLayer1(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"I’m David, here to help my aunty. Love your song ‘Greater Few’!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Matias here. Got our phones snatched—now we’re crashing your party!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"We’re just tourists caught up in some bad luck, that’s all.\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Who cares about us? You’re the big shot—why talk to us?\""));
    }

    IEnumerator HandleLayer2(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"My aunty knows the Zuwena Family—she got us in.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Dumb luck, man! We’re not fancy like you.\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Not sure, but we’re here now—pretty cool, right?\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Why’s a star like you bothering with our invite?\""));
    }

    IEnumerator HandleLayer3(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Just a crowded market—nothing special.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Some jerk ran off with our phones, that’s it!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Didn’t catch much—too busy freaking out.\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"What’s it to you? You gonna sing about it?\""));
    }

    IEnumerator HandleLayer4(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"‘Greater Few’ is your best track—huge fan!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Never heard it, but you’re famous, so whatever.\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Just saying it’s awesome—thought you’d like to know.\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Why’s Sadiki such a big deal to you, huh?\""));
    }

    IEnumerator HandleLayer5(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Just fancy folks here—nothing weird.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Shady? Nah, just rich snobs like you!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Maybe a guy by the bar looked sketchy—dunno.\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"You tell us—what’s shady in your fancy world?\""));
    }

    IEnumerator HandleLayer6(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Staff’s normal—busy with the party.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Odd? You’re the oddest thing here, superstar!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"One waiter seemed jumpy—probably nothing.\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Why’re you digging? Got a song idea or what?\""));
    }

    IEnumerator HandleLayer7(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Nothing solid yet—sorry, man.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"We’re not your spies—buzz off!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Heard someone mention a ‘Kweku’ by the door.\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"You’re the big shot—why need us?\""));
    }

    IEnumerator HandleLayer8(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Saw some guy slip out back—keep us out of it!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Yeah, a dude with a limp—take it or leave it!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"That’s all we’ve got—let us enjoy the party!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Figure it out yourself, Mr. Million Followers!\""));
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

            if (godfreyTrust != null)
            {
                godfreyTrust.CheckTrustLevelAtEnd();
            }
            onDialogueEnd?.Invoke();
            isDialogueCompleted = true;
        }
    }
}