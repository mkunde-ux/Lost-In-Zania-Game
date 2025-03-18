using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class LucaDialogue : MonoBehaviour
{
    //Check Adila dialogue script for comments

    [Header("UI References")]
    public TMP_Text dialogueText;
    public Button path1Button, path2Button, path3Button, path4Button;
    public Slider timerSlider;
    [SerializeField] private Canvas dialogueCanvas;

    [Header("Audio Settings (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip typeSoundClip;
    [SerializeField] private AudioClip buttonPressClip;

    private bool isTyping = false;
    private string fullSentence = "";
    private bool isPlayerResponseComplete = false;

    public int dialogueLayer = 1;
    private bool isPlayerTurn = false;

    public LucaTrust lucaTrust; 
    public UnityEvent onDialogueEnd;

    private int maxDialogueLayer = 8;
    private float timer = 15f;
    private bool isTimerRunning = false;

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
        if (lucaTrust != null)
        {
            lucaTrust.AdjustMiziki(-10); 
        }
        StartCoroutine(TypeSentence("Luca: \"Oh, come now, don’t waste my time! I’m a busy man speak up, or I’ll find someone who will!\""));
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
                    StartCoroutine(TypeSentence("Luca: \"Well, well! Two young lads looking a bit lost at this fine party. I’m Luca charmed to meet you! Now, what brings you to Shadiki’s lovely restaurant?\""));
                    break;
                case 2:
                    StartCoroutine(TypeSentence("Luca: \"Stolen phones, eh? How unfortunate! But perhaps… fortuitous for me. I’m here on business, you see, and I could use some… information. Know anything juicy about this place?\""));
                    break;
                case 3:
                    StartCoroutine(TypeSentence("Luca: \"Oh, don’t play coy! I’ve got big plans for Africa, and Shadiki’s the key. But he’s stubborn, you know? I need something… persuasive. Seen anything odd around here lately?\""));
                    break;
                case 4:
                    StartCoroutine(TypeSentence("Luca: \"Hah, tourists with a nose for trouble! I like that! But let’s cut to the chase what do you know about Shadiki’s dealings? Any whispers of dirt I can… leverage?\""));
                    break;
                case 5:
                    StartCoroutine(TypeSentence("Luca: \"Come now, don’t hold out on me! I smell opportunity, and you two might just be my lucky charm. Tell me more what’s the word on the street about this restaurant?\""));
                    break;
                case 6:
                    StartCoroutine(TypeSentence("Luca: \"Interesting… very interesting. You’ve got a sharp eye or a sharp tongue! But I need more. What about the Zuwena family? Any secrets I can use to… nudge Shadiki my way?\""));
                    break;
                case 7:
                    StartCoroutine(TypeSentence("Luca: \"Now we’re getting somewhere! But I’m a man of ambition I need a solid lead. One last chance: give me something concrete, lads, or I’ll have to dig elsewhere… and I dig deep.\""));
                    break;
                case 8:
                    StartCoroutine(TypeSentence("Luca: \"Time to wrap this up, boys. You’ve been… entertaining, but I’ve got deals to seal. Final offer: spill something I can use on Shadiki, or I walk and I don’t forget faces.\""));
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
                SetButtonLabels("David: We’re just here on holiday, sir!",
                                "Matias: Got our phones stolen bit of a mess!",
                                "David: I’m Tanzanian, reconnecting with my roots!",
                                "Matias: Looking for some answers, that’s all!");
                break;
            case 2:
                SetButtonLabels("David: Not much, just trying to get our phones back!",
                                "Matias: Juicy? We’re the ones who got robbed, man!",
                                "David: It’s a nice place, but I don’t know any secrets!",
                                "Matias: Why? You digging for dirt or something?");
                break;
            case 3:
                SetButtonLabels("David: Odd? Just the usual restaurant buzz, I think!",
                                "Matias: Seen nothing weird why’re you so curious?",
                                "David: I don’t know Shadiki well what’s your deal?",
                                "Matias: What’s in it for us if we spill anything?");
                break;
            case 4:
                SetButtonLabels("David: No idea about his dealings just here to eat!",
                                "Matias: Dirt? Nah, we’re not snitches, Luca!",
                                "David: Maybe some shady types around, but nothing solid!",
                                "Matias: Sounds like you’ve got bigger plans than us!");
                break;
            case 5:
                SetButtonLabels("David: Just heard it’s a popular spot, nothing more!",
                                "Matias: Word on the street? It’s all food and music!",
                                "David: Seen some odd folks in the market that count?",
                                "Matias: You’re fishing hard why should we help?");
                break;
            case 6:
                SetButtonLabels("David: Secrets? I don’t know the family like that!",
                                "Matias: Nudge? Sounds like you’re up to no good!",
                                "David: Heard they’re tight-knit nothing shady, though!",
                                "Matias: Maybe you’re asking the wrong guys, Luca!");
                break;
            case 7:
                SetButtonLabels("David: I’ve got nothing concrete just guesses!",
                                "Matias: Dig elsewhere we’re not your spies!",
                                "David: Saw some guy arguing with staff maybe a lead?",
                                "Matias: How ‘bout you tell us what you’re really after?");
                break;
            case 8:
                SetButtonLabels("David: I’ll tell you what I know just don’t drag us in!",
                                "Matias: Fine, heard a rumor about payments happy?",
                                "David: I’ve got nothing more let us be, please!",
                                "Matias: Walk away, man we’re not your pawns!");
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
        if (timer > 0f && lucaTrust != null)
        {
            Debug.Log("Quick reply! Luca likes decisiveness gain 5 trust.");
            lucaTrust.AdjustMiziki(5); 
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
        yield return StartCoroutine(LucaResponse(path));
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
        if (lucaTrust == null)
        {
            Debug.LogError("LucaTrust reference missing!");
            return;
        }

        switch (dialogueLayer)
        {
            case 1:
                if (path == 1) lucaTrust.AdjustMiziki(3);
                else if (path == 2) lucaTrust.AdjustMiziki(5);
                else if (path == 3) lucaTrust.AdjustMiziki(7);
                else if (path == 4) lucaTrust.AdjustMiziki(2);
                break;
            case 2:
                if (path == 1) lucaTrust.AdjustMiziki(-2);
                else if (path == 2) lucaTrust.AdjustMiziki(-5);
                else if (path == 3) lucaTrust.AdjustMiziki(0);
                else if (path == 4) lucaTrust.AdjustMiziki(-3);
                break;
            case 3:
                if (path == 1) lucaTrust.AdjustMiziki(0);
                else if (path == 2) lucaTrust.AdjustMiziki(-3);
                else if (path == 3) lucaTrust.AdjustMiziki(-2);
                else if (path == 4) lucaTrust.AdjustMiziki(4);
                break;
            case 4:
                if (path == 1) lucaTrust.AdjustMiziki(-2);
                else if (path == 2) lucaTrust.AdjustMiziki(-5);
                else if (path == 3) lucaTrust.AdjustMiziki(6);
                else if (path == 4) lucaTrust.AdjustMiziki(2);
                break;
            case 5:
                if (path == 1) lucaTrust.AdjustMiziki(0);
                else if (path == 2) lucaTrust.AdjustMiziki(-1);
                else if (path == 3) lucaTrust.AdjustMiziki(8);
                else if (path == 4) lucaTrust.AdjustMiziki(-4);
                break;
            case 6:
                if (path == 1) lucaTrust.AdjustMiziki(-3);
                else if (path == 2) lucaTrust.AdjustMiziki(-6);
                else if (path == 3) lucaTrust.AdjustMiziki(5);
                else if (path == 4) lucaTrust.AdjustMiziki(-2);
                break;
            case 7:
                if (path == 1) lucaTrust.AdjustMiziki(0);
                else if (path == 2) lucaTrust.AdjustMiziki(-5);
                else if (path == 3) lucaTrust.AdjustMiziki(10);
                else if (path == 4) lucaTrust.AdjustMiziki(3);
                break;
            case 8:
                if (path == 1) lucaTrust.AdjustMiziki(5);
                else if (path == 2) lucaTrust.AdjustMiziki(12);
                else if (path == 3) lucaTrust.AdjustMiziki(0);
                else if (path == 4) lucaTrust.AdjustMiziki(-8);
                break;
        }
    }

    IEnumerator LucaResponse(int path)
    {
        if (lucaTrust == null)
        {
            Debug.LogError("LucaTrust reference missing!");
            yield break;
        }

        int trustChange = lucaTrust.GetLastMizikiChange(); 
        string[] positiveResponses = {
            "Luca: \"Hah, I like your spirit! You’re proving useful already!\"",
            "Luca: \"Now that’s what I’m talking about! Keep it coming, lads!\"",
            "Luca: \"Oh, splendid! You’ve got a knack for this let’s chat more!\""
        };
        string[] negativeResponses = {
            "Luca: \"Tch, boring! I thought you’d have more for me step it up!\"",
            "Luca: \"Really? That’s all? Don’t waste my time with crumbs!\"",
            "Luca: \"Disappointing… I expected better from you two. Try harder!\""
        };

        if (trustChange > 0)
            yield return StartCoroutine(TypeSentence(positiveResponses[Random.Range(0, positiveResponses.Length)]));
        else if (trustChange < 0)
            yield return StartCoroutine(TypeSentence(negativeResponses[Random.Range(0, negativeResponses.Length)]));
    }

    IEnumerator HandleLayer1(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"We’re just here on holiday, sir!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Got our phones stolen bit of a mess!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"I’m Tanzanian, reconnecting with my roots!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Looking for some answers, that’s all!\""));
    }

    IEnumerator HandleLayer2(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Not much, just trying to get our phones back!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Juicy? We’re the ones who got robbed, man!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"It’s a nice place, but I don’t know any secrets!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Why? You digging for dirt or something?\""));
    }

    IEnumerator HandleLayer3(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Odd? Just the usual restaurant buzz, I think!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Seen nothing weird why’re you so curious?\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"I don’t know Shadiki well what’s your deal?\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"What’s in it for us if we spill anything?\""));
    }

    IEnumerator HandleLayer4(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"No idea about his dealings just here to eat!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Dirt? Nah, we’re not snitches, Luca!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Maybe some shady types around, but nothing solid!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Sounds like you’ve got bigger plans than us!\""));
    }

    IEnumerator HandleLayer5(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Just heard it’s a popular spot, nothing more!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Word on the street? It’s all food and music!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Seen some odd folks in the market that count?\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"You’re fishing hard why should we help?\""));
    }

    IEnumerator HandleLayer6(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Secrets? I don’t know the family like that!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Nudge? Sounds like you’re up to no good!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Heard they’re tight-knit nothing shady, though!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Maybe you’re asking the wrong guys, Luca!\""));
    }

    IEnumerator HandleLayer7(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"I’ve got nothing concrete just guesses!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Dig elsewhere we’re not your spies!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Saw some guy arguing with staff maybe a lead?\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"How ‘bout you tell us what you’re really after?\""));
    }

    IEnumerator HandleLayer8(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"I’ll tell you what I know just don’t drag us in!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Fine, heard a rumor about payments happy?\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"I’ve got nothing more let us be, please!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Walk away, man we’re not your pawns!\""));
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

            if (lucaTrust != null)
            {
                lucaTrust.CheckMizikiLevelAtEnd();
            }
            else
            {
                Debug.LogError("LucaTrust reference missing!");
            }

            onDialogueEnd?.Invoke();
            isDialogueCompleted = true;
        }
    }
}
