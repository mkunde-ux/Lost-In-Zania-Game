using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class ImaniDialogue : MonoBehaviour
{
    //Check the Fahari Dialogue Script for comments

    public TMP_Text dialogueText;
    public Button path1Button, path2Button, path3Button, path4Button, path5Button, path6Button;
    public Slider timerSlider;
    [SerializeField] private Canvas dialogueCanvas;

    private bool isTyping = false;
    private string fullSentence = "";
    private bool isPlayerResponseComplete = false;

    public int dialogueLayer = 1;
    private bool isPlayerTurn = false;

    public ImaniTrust imaniTrust;
    public UnityEvent onDialogueEnd;

    private int maxDialogueLayer = 6;
    private float timer = 20f;
    private bool isTimerRunning = false;

    private bool isDialogueCompleted = false;
    private bool isPlayerInRange = false; 

    public AudioSource audioSource;    
    public AudioClip buttonPressClip;    
    public AudioClip typeSoundClip;    



    void Start()
    {
        path1Button.onClick.AddListener(() => ChoosePath(1));
        path2Button.onClick.AddListener(() => ChoosePath(2));
        path3Button.onClick.AddListener(() => ChoosePath(3));
        path4Button.onClick.AddListener(() => ChoosePath(4));
        path5Button.onClick.AddListener(() => ChoosePath(5));
        path6Button.onClick.AddListener(() => ChoosePath(6));

        timerSlider.maxValue = timer;
        timerSlider.value = timer;

        dialogueCanvas.gameObject.SetActive(false);
    }

    public void ResetDialogue()
    {
        StopAllCoroutines();
        isTyping = false;
        dialogueLayer = 1;
        isDialogueCompleted = false;
        isPlayerTurn = false;
        StartDialogue();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E key pressed. isPlayerInRange: " + isPlayerInRange);
            if (isPlayerInRange && !isDialogueCompleted)
            {
                StartDialogue();
            }
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
        imaniTrust.AdjustTrust(-15);
        StartCoroutine(TypeSentence("Imani: \"What, cat got your tongue? My patience isn’t a charity, darlings!\""));
        ProceedToNextLayer();
    }

    void StartTimer()
    {
        timer = 20f;
        timerSlider.value = timer;
        isTimerRunning = true;
    }

    void StopTimer()
    {
        isTimerRunning = false;
    }

    public void StartDialogue()
    {
        if (dialogueCanvas.gameObject.activeSelf) return;
        if (!isPlayerInRange) return;

        dialogueCanvas.gameObject.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        isPlayerTurn = false;
        isDialogueCompleted = false;
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
        if (isPlayerTurn) isPlayerResponseComplete = true;
    }

    public bool IsDialogueComplete()
    {
        return isDialogueCompleted;
    }

    public void SetPlayerInRange(bool inRange)
    {
        isPlayerInRange = inRange;
        Debug.Log("Player in range: " + inRange);
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
                    StartCoroutine(TypeSentence("Imani: \"Well, well, two sweaty strangers in my restaurant! Did the market spit you out, or are you just lost chasing the sun? Spill it!\""));
                    break;
                case 2:
                    StartCoroutine(TypeSentence("Imani: \"Stolen phones, huh? Juicy! But why should I care about your little drama when my wine’s worth more than your sob story?\""));
                    break;
                case 3:
                    StartCoroutine(TypeSentence("Imani: \"Ha! You slipped past my bouncers? Either you’re slicker than a mango peel, or they’re napping again how’d you do it?\""));
                    break;
                case 4:
                    StartCoroutine(TypeSentence("Imani: \"Okay, I’m hooked what’s the prize? Your phones better be gold-plated, ‘cause I don’t waste glitter on petty thieves!\""));
                    break;
                case 5:
                    StartCoroutine(TypeSentence("Imani: \"This is getting spicy! You’re stirring my pot, and I don’t like burned stew. Prove you’re worth my shine or else!\""));
                    break;
                case 6:
                    StartCoroutine(TypeSentence("Imani: \"Curtain’s dropping, sweethearts! Trust me to save your skins, or run before my patience turns to ash what’s it gonna be?\""));
                    break;
            }
            StartCoroutine(WaitForNPCDialogue());
        }
    }

    IEnumerator WaitForNPCDialogue()
    {
        yield return new WaitUntil(() => !isTyping);
        SetPlayerTurn();
    }

    void SetPlayerTurn()
    {
        isPlayerTurn = true;
        switch (dialogueLayer)
        {
            case 1:
                SetButtonLabels("David: Our phones got nabbed help us!",
                               "Matias: What’s it to you, fancy pants?",
                               "David: I’m Tanzanian this is home!",
                               "Matias: Chasing thieves, not sunsets!",
                               "David: Someone here knows the crooks!",
                               "Matias: Your place screamed ‘adventure’!");
                break;
            case 2:
                SetButtonLabels("David: We need our phones back please!",
                               "Matias: Your wine’s safe, chill out!",
                               "David: It’s my roots I’m not leaving!",
                               "Matias: Drama’s my jam join the fun!",
                               "David: Thieves hit us your staff’s shady!",
                               "Matias: We’re worth more than your vintage!");
                break;
            case 3:
                SetButtonLabels("David: I’m sneaky it’s my turf!",
                               "Matias: Your bouncers? Total snoozers!",
                               "David: Dodged worse back home!",
                               "Matias: Outsmarted ‘em too easy!",
                               "David: Know this place like my backyard!",
                               "Matias: Walked in like VIPs deal with it!");
                break;
            case 4:
                SetButtonLabels("David: Our phones our lifeline!",
                               "Matias: A thrill worth your glitter!",
                               "David: Honor’s at stake help me!",
                               "Matias: To outwit the crooks cool, right?",
                               "David: Answers, not your fancy forks!",
                               "Matias: Epic loot phones and glory!");
                break;
            case 5:
                SetButtonLabels("David: We’ll fight for what’s ours!",
                               "Matias: Spicy? I’m the whole chili!",
                               "David: I’ll slip out with my pride!",
                               "Matias: Worth your shine bet on us!",
                               "David: You’re hiding the truth spill it!",
                               "Matias: Burned stew? We’re the flavor!");
                break;
            case 6:
                SetButtonLabels("David: Trust you save our phones!",
                               "Matias: Dash out with swagger bye!",
                               "David: Team up end this right!",
                               "Matias: Talk our way to freedom!",
                               "David: Wait for the win smart move!",
                               "Matias: Bolt before you turn sour!");
                break;
        }
        path1Button.gameObject.SetActive(true);
        path2Button.gameObject.SetActive(true);
        path3Button.gameObject.SetActive(true);
        path4Button.gameObject.SetActive(true);
        path5Button.gameObject.SetActive(true);
        path6Button.gameObject.SetActive(true);
        StartTimer();
    }

    void SetButtonLabels(string path1Text, string path2Text, string path3Text, string path4Text, string path5Text, string path6Text)
    {
        path1Button.GetComponentInChildren<TMP_Text>().text = path1Text;
        path2Button.GetComponentInChildren<TMP_Text>().text = path2Text;
        path3Button.GetComponentInChildren<TMP_Text>().text = path3Text;
        path4Button.GetComponentInChildren<TMP_Text>().text = path4Text;
        path5Button.GetComponentInChildren<TMP_Text>().text = path5Text;
        path6Button.GetComponentInChildren<TMP_Text>().text = path6Text;
    }

    void ChoosePath(int path)
    {
        StopTimer();

        if (audioSource != null && buttonPressClip != null)
        {
            audioSource.PlayOneShot(buttonPressClip);
        }

        if (timer > 0f)
        {
            Debug.Log("Quick reply! Imani likes it gain 5 trust.");
            imaniTrust.AdjustTrust(5);
        }

        path1Button.gameObject.SetActive(false);
        path2Button.gameObject.SetActive(false);
        path3Button.gameObject.SetActive(false);
        path4Button.gameObject.SetActive(false);
        path5Button.gameObject.SetActive(false);
        path6Button.gameObject.SetActive(false);

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
        }
        yield return new WaitForSeconds(5); 
        AdjustTrustBasedOnPath(path); 
        yield return StartCoroutine(ImaniResponse(path));
        ProceedToNextLayer();
    }

    void ProceedToNextLayer()
    {
        dialogueLayer++;
        if (dialogueLayer > maxDialogueLayer) EndDialogue();
        else SetDialogueLayer();
    }

    void AdjustTrustBasedOnPath(int path)
    {
        if (imaniTrust == null) { Debug.LogError("ImaniTrust reference missing!"); return; }
        switch (dialogueLayer)
        {
            case 1:
                if (path == 1) imaniTrust.AdjustTrust(90);     // David’s plea
                else if (path == 2) imaniTrust.AdjustTrust(-30); // Matias’ sass
                else if (path == 3) imaniTrust.AdjustTrust(-40);  // David’s roots
                else if (path == 4) imaniTrust.AdjustTrust(20); // Matias’ flippancy
                else if (path == 5) imaniTrust.AdjustTrust(20);  // David’s hint
                else if (path == 6) imaniTrust.AdjustTrust(30); // Matias’ drama
                break;
            case 2:
                if (path == 1) imaniTrust.AdjustTrust(7);
                else if (path == 2) imaniTrust.AdjustTrust(-13);
                else if (path == 3) imaniTrust.AdjustTrust(15);
                else if (path == 4) imaniTrust.AdjustTrust(-14);
                else if (path == 5) imaniTrust.AdjustTrust(12);
                else if (path == 6) imaniTrust.AdjustTrust(-16);
                break;
            case 3:
                if (path == 1) imaniTrust.AdjustTrust(5);
                else if (path == 2) imaniTrust.AdjustTrust(18);
                else if (path == 3) imaniTrust.AdjustTrust(13);
                else if (path == 4) imaniTrust.AdjustTrust(16);
                else if (path == 5) imaniTrust.AdjustTrust(-12);
                else if (path == 6) imaniTrust.AdjustTrust(-15);
                break;
            case 4:
                if (path == 1) imaniTrust.AdjustTrust(8);
                else if (path == 2) imaniTrust.AdjustTrust(-24);
                else if (path == 3) imaniTrust.AdjustTrust(27);
                else if (path == 4) imaniTrust.AdjustTrust(-23);
                else if (path == 5) imaniTrust.AdjustTrust(24);
                else if (path == 6) imaniTrust.AdjustTrust(-8);
                break;
            case 5:
                if (path == 1) imaniTrust.AdjustTrust(6);
                else if (path == 2) imaniTrust.AdjustTrust(49);
                else if (path == 3) imaniTrust.AdjustTrust(25);
                else if (path == 4) imaniTrust.AdjustTrust(27);
                else if (path == 5) imaniTrust.AdjustTrust(-22);
                else if (path == 6) imaniTrust.AdjustTrust(10);
                break;
            case 6:
                if (path == 1) imaniTrust.AdjustTrust(22);
                else if (path == 2) imaniTrust.AdjustTrust(16);
                else if (path == 3) imaniTrust.AdjustTrust(40);
                else if (path == 4) imaniTrust.AdjustTrust(48);
                else if (path == 5) imaniTrust.AdjustTrust(44);
                else if (path == 6) imaniTrust.AdjustTrust(-70);
                break;
            default:
                Debug.LogWarning("Invalid dialogue layer!");
                break;
        }
    }

    IEnumerator ImaniResponse(int path)
    {
        int trustChange = imaniTrust.GetLastTrustChange();
        string[] positiveResponses = {
            "Imani: \"Oh, stars align! You’ve got some spark maybe you’re worth my shine!\"",
            "Imani: \"Clever darlings! I’m dazzled don’t trip over your own glow now!\"",
            "Imani: \"Ha! You’ve got guts and glitter I might just keep you around!\""
        };
        string[] negativeResponses = {
            "Imani: \"Ugh, dull as a rusty spoon! You’re wasting my sparkle scram!\"",
            "Imani: \"What a flop! Dumber than a sack of sand get lost, loves!\"",
            "Imani: \"Oh, honey, no! You’re a storm cloud on my sunny day out!\""
        };
        if (trustChange > 0)
            yield return StartCoroutine(TypeSentence(positiveResponses[Random.Range(0, positiveResponses.Length)]));
        else if (trustChange < 0)
            yield return StartCoroutine(TypeSentence(negativeResponses[Random.Range(0, negativeResponses.Length)]));
    }

    IEnumerator HandleLayer1(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Our phones got nabbed help us, please!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"What’s it to you, fancy pants? Mind your wine!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"I’m Tanzanian this is home, not your turf!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Chasing thieves, not sunsets deal with it!\""));
        else if (path == 5) yield return StartCoroutine(TypeSentence("David: \"Someone here knows the crooks spill it!\""));
        else if (path == 6) yield return StartCoroutine(TypeSentence("Matias: \"Your place screamed ‘adventure’ here we are!\""));
    }

    IEnumerator HandleLayer2(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"We need our phones back please, Imani!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Your wine’s safe, chill out help us instead!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"It’s my roots I’m not leaving without them!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Drama’s my jam join the fun, princess!\""));
        else if (path == 5) yield return StartCoroutine(TypeSentence("David: \"Thieves hit us your staff’s shady, check it!\""));
        else if (path == 6) yield return StartCoroutine(TypeSentence("Matias: \"We’re worth more than your vintage bet on it!\""));
    }

    IEnumerator HandleLayer3(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"I’m sneaky it’s my turf, I know the moves!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Your bouncers? Total snoozers too easy!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Dodged worse back home child’s play!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Outsmarted ‘em too easy, Imani!\""));
        else if (path == 5) yield return StartCoroutine(TypeSentence("David: \"Know this place like my backyard try me!\""));
        else if (path == 6) yield return StartCoroutine(TypeSentence("Matias: \"Walked in like VIPs deal with it, darling!\""));
    }

    IEnumerator HandleLayer4(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Our phones our lifeline, help us out!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"A thrill worth your glitter game on!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Honor’s at stake help me, Imani!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"To outwit the crooks cool, right?\""));
        else if (path == 5) yield return StartCoroutine(TypeSentence("David: \"Answers, not your fancy forks dig in!\""));
        else if (path == 6) yield return StartCoroutine(TypeSentence("Matias: \"Epic loot phones and glory, baby!\""));
    }

    IEnumerator HandleLayer5(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"We’ll fight for what’s ours no backing down!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Spicy? I’m the whole chili bring it!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"I’ll slip out with my pride watch me!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Worth your shine bet on us, Imani!\""));
        else if (path == 5) yield return StartCoroutine(TypeSentence("David: \"You’re hiding the truth spill it now!\""));
        else if (path == 6) yield return StartCoroutine(TypeSentence("Matias: \"Burned stew? We’re the flavor taste it!\""));
    }

    IEnumerator HandleLayer6(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Trust you save our phones, wise one!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Dash out with swagger bye, Imani!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Team up end this right, together!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Talk our way to freedom smooth moves!\""));
        else if (path == 5) yield return StartCoroutine(TypeSentence("David: \"Wait for the win smart move, huh?\""));
        else if (path == 6) yield return StartCoroutine(TypeSentence("Matias: \"Bolt before you turn sour later!\""));
    }

    public void EndDialogue()
    {
        if (dialogueLayer > maxDialogueLayer)
        {
            path1Button.gameObject.SetActive(false);
            path2Button.gameObject.SetActive(false);
            path3Button.gameObject.SetActive(false);
            path4Button.gameObject.SetActive(false);
            path5Button.gameObject.SetActive(false);
            path6Button.gameObject.SetActive(false);
            dialogueCanvas.gameObject.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            if (imaniTrust != null) imaniTrust.CheckTrustLevelAtEnd();
            else Debug.LogError("ImaniTrust reference missing!");
            onDialogueEnd?.Invoke();
            isDialogueCompleted = true; 
        }
    }
}