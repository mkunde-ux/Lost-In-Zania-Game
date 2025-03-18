using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class EmanuelDT : MonoBehaviour
{
    //Check the Fahari Dialogue Script for comments

    [Header("UI References")]
    public TMP_Text dialogueText;
    public Button path1Button, path2Button, path3Button, path4Button, path5Button, path6Button;
    public Slider timerSlider;
    [SerializeField] private Canvas dialogueCanvas;

    [Header("Trust System")]
    [SerializeField] private EmanuelTrust emanuelTrust;
    public UnityEvent onDialogueEnd;

    [Header("Dialogue Settings")]
    private bool isTyping = false;
    private string fullSentence = "";
    private bool isPlayerResponseComplete = false;
    public int dialogueLayer = 1;
    private bool isPlayerTurn = false;
    private int maxDialogueLayer = 10;
    private float timer = 20f;
    private bool isTimerRunning = false;
    private bool isDialogueCompleted = false;
    public bool isPlayerInRange = false;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip typeSoundClip;
    [SerializeField] private AudioClip buttonPressClip;

    void Start()
    {
        dialogueCanvas.gameObject.SetActive(false);

        path1Button.onClick.AddListener(() => ChoosePath(1));
        path2Button.onClick.AddListener(() => ChoosePath(2));
        path3Button.onClick.AddListener(() => ChoosePath(3));
        path4Button.onClick.AddListener(() => ChoosePath(4));
        path5Button.onClick.AddListener(() => ChoosePath(5));
        path6Button.onClick.AddListener(() => ChoosePath(6));

        timerSlider.maxValue = timer;
        timerSlider.value = timer;
    }

    public void ResetDialogue()
    {
        dialogueLayer = 1;
        isDialogueCompleted = false;
        StartDialogue();
    }

    public void SetPlayerInRange(bool inRange)
    {
        isPlayerInRange = inRange;
        if (!inRange && dialogueCanvas.gameObject.activeSelf)
        {
            EndDialogue();
        }
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
        emanuelTrust.AdjustTrust(-15);
        StartCoroutine(TypeSentence("Emmanuel: \"Time’s up, slowpokes! You’re as quick as a sloth on sedatives!\""));
        ProceedToNextLayer();
    }

    void StartTimer()
    {
        timer = 40f;
        timerSlider.value = timer;
        isTimerRunning = true;
    }

    void StopTimer()
    {
        isTimerRunning = false;
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

    public void StartDialogue()
    {
        if (isDialogueCompleted) return;
        dialogueCanvas.gameObject.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        isPlayerTurn = false;
        SetDialogueLayer();
    }

    public bool IsDialogueComplete()
    {
        return isDialogueCompleted;
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
                    StartCoroutine(TypeSentence("Emanuel: \"Well, look who stumbled in. That Spanish accent’s thick enough to choke on. What brings you lot to Tanzania? I’ve been to Spain nice enough, but the people? Soft as soggy bread.\""));
                    break;
                case 2:
                    StartCoroutine(TypeSentence("Emanuel: \"Back again, are you? This place used to be decent now it’s a dumping ground for every lowlife with sticky fingers. Respect’s dead here, and I’m surrounded by fools.\""));
                    break;
                case 3:
                    StartCoroutine(TypeSentence("Emanuel: \"Still loitering, eh? If you’re digging for info, try Alen bald as an egg and half as useful or J, who’s got the brains of a brick. Good luck with that mess.\""));
                    break;
                case 4:
                    StartCoroutine(TypeSentence("Emanuel: \"What’s this, you expect me to hold your hand? I’m not your babysitter figure it out yourselves, or are your heads just for decoration?\""));
                    break;
                case 5:
                    StartCoroutine(TypeSentence("Emanuel: \"Oh, brilliant, you’re still here pestering me. Should I bow to your genius or just call you what you are clueless tourists with too much time?\""));
                    break;
                case 6:
                    StartCoroutine(TypeSentence("Emanuel: \"Alright, I’ve had my fun. Day’s been a slog, so here’s the deal: Michael, that lumbering oaf in blue, knows where Alen and J are. Go bother him instead.\""));
                    break;
                case 7:
                    StartCoroutine(TypeSentence("Emanuel: \"You’re persistent, I’ll give you that. Spain, huh? I’ve got contacts there Matias and David. Might ring them up one day if you stop wasting my time.\""));
                    break;
                case 8:
                    StartCoroutine(TypeSentence("Emanuel: \"What, still sniffing around? Keep it up, and maybe I’ll throw you a bone Matias owes me a favor, and David’s got connections. Could be useful for you in Spain.\""));
                    break;
                case 9:
                    StartCoroutine(TypeSentence("Emanuel: \"You’re not as dull as you look, I suppose. I’ll think about calling Matias and David could set you up nicely over there. Don’t screw it up if I do.\""));
                    break;
                case 10:
                    StartCoroutine(TypeSentence("Emanuel: \"Fine, you’ve worn me down. I’ll reach out to Matias and David tomorrow Spain’s their turf, and they’ll sort you out. Now get lost before I change my mind.\""));
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
                SetButtonLabels("Visiting family, relax!", "Soft? Look who’s talking!", "Spain’s got edge, pal!", "Just passing through, easy!", "Focus on your own mess!", "Tanzania’s calling me!");
                break;
            case 2:
                SetButtonLabels("Thieves love this dump!", "You run this circus?", "Sticky fingers everywhere!", "What a disaster!", "Not my problem, mate!", "Respect’s a joke here!");
                break;
            case 3:
                SetButtonLabels("Where’s Alen and J?", "You’re the rude one!", "Tell me more, now!", "I’ll find them myself!", "Keep your gossip!", "Useless? That’s you!");
                break;
            case 4:
                SetButtonLabels("What’s your issue, huh?", "Stop being a jerk!", "Calm down, we’re fine!", "I’ve got this, genius!", "You’re useless anyway!", "Use your brain first!");
                break;
            case 5:
                SetButtonLabels("Funny guy, eh?", "Weak insult, try again!", "Drop the attitude!", "Alright, you’re off the hook!", "You’re the real clown!", "Take it easy, drama!");
                break;
            case 6:
                SetButtonLabels("Thanks, your highness!", "All talk, no help!", "Finally, some sense!", "Michael it is, cheers!", "Bad day? Boo-hoo!", "No games? Doubt it!");
                break;
            case 7:
                SetButtonLabels("Spain contacts? Nice!", "Don’t tease, deliver!", "Make it happen, big shot!", "Cool, I’ll wait!", "Matias and David, huh?", "Prove it, loudmouth!");
                break;
            case 8:
                SetButtonLabels("Hook me up, then!", "Stop dangling carrots!", "Spain’s my goal, help!", "Sounds promising!", "Favor? Use it!", "Connections? Show me!");
                break;
            case 9:
                SetButtonLabels("Sweet, call them!", "Don’t mess this up!", "Spain’s waiting, do it!", "Appreciate it, maybe!", "Nice move, Emanuel!", "Still think you’re bluffing!");
                break;
            case 10:
                SetButtonLabels("Great, I’m out!", "Soap opera’s over!", "Thanks, I guess!", "Spain here I come!", "Don’t back out now!", "Later, big talker!");
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
        if (audioSource != null && buttonPressClip != null)
        {
            audioSource.PlayOneShot(buttonPressClip);
        }

        StopTimer();
        if (timer > 0f)
        {
            Debug.Log("Player replied in time! Player gains 5 trust points.");
            emanuelTrust.AdjustTrust(5);
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
        AdjustTrustBasedOnPath(path);
        yield return StartCoroutine(EmmanuelResponse(path));
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
        if (emanuelTrust == null)
        {
            Debug.LogError("emanuelTrust reference missing!");
            return;
        }
        switch (dialogueLayer)
        {
            case 1:
                if (path == 1) emanuelTrust.AdjustTrust(90);
                else if (path == 2) emanuelTrust.AdjustTrust(-5);
                else if (path == 3) emanuelTrust.AdjustTrust(5);
                else if (path == 4) emanuelTrust.AdjustTrust(-10);
                else if (path == 5) emanuelTrust.AdjustTrust(2);
                else if (path == 6) emanuelTrust.AdjustTrust(-7);
                break;
            case 2:
                if (path == 1) emanuelTrust.AdjustTrust(70);
                else if (path == 2) emanuelTrust.AdjustTrust(-30);
                else if (path == 3) emanuelTrust.AdjustTrust(30);
                else if (path == 4) emanuelTrust.AdjustTrust(-80);
                else if (path == 5) emanuelTrust.AdjustTrust(30);
                else if (path == 6) emanuelTrust.AdjustTrust(-50);
                break;
            case 3:
                if (path == 1) emanuelTrust.AdjustTrust(5);
                else if (path == 2) emanuelTrust.AdjustTrust(-10);
                else if (path == 3) emanuelTrust.AdjustTrust(20);
                else if (path == 4) emanuelTrust.AdjustTrust(60);
                else if (path == 5) emanuelTrust.AdjustTrust(-40);
                else if (path == 6) emanuelTrust.AdjustTrust(-90);
                break;
            case 4:
                if (path == 1) emanuelTrust.AdjustTrust(-80);
                else if (path == 2) emanuelTrust.AdjustTrust(-40);
                else if (path == 3) emanuelTrust.AdjustTrust(60);
                else if (path == 4) emanuelTrust.AdjustTrust(70);
                else if (path == 5) emanuelTrust.AdjustTrust(-30);
                else if (path == 6) emanuelTrust.AdjustTrust(-40);
                break;
            case 5:
                if (path == 1) emanuelTrust.AdjustTrust(-60);
                else if (path == 2) emanuelTrust.AdjustTrust(-80);
                else if (path == 3) emanuelTrust.AdjustTrust(60);
                else if (path == 4) emanuelTrust.AdjustTrust(30);
                else if (path == 5) emanuelTrust.AdjustTrust(-20);
                else if (path == 6) emanuelTrust.AdjustTrust(-12);
                break;
            case 6:
                if (path == 1) emanuelTrust.AdjustTrust(12);
                else if (path == 2) emanuelTrust.AdjustTrust(60);
                else if (path == 3) emanuelTrust.AdjustTrust(80);
                else if (path == 4) emanuelTrust.AdjustTrust(10);
                else if (path == 5) emanuelTrust.AdjustTrust(-50);
                else if (path == 6) emanuelTrust.AdjustTrust(-15);
                break;
            case 7:
                if (path == 1) emanuelTrust.AdjustTrust(12);
                else if (path == 2) emanuelTrust.AdjustTrust(60);
                else if (path == 3) emanuelTrust.AdjustTrust(80);
                else if (path == 4) emanuelTrust.AdjustTrust(10);
                else if (path == 5) emanuelTrust.AdjustTrust(-50);
                else if (path == 6) emanuelTrust.AdjustTrust(-15);
                break;
            case 8:
                if (path == 1) emanuelTrust.AdjustTrust(12);
                else if (path == 2) emanuelTrust.AdjustTrust(60);
                else if (path == 3) emanuelTrust.AdjustTrust(80);
                else if (path == 4) emanuelTrust.AdjustTrust(10);
                else if (path == 5) emanuelTrust.AdjustTrust(-50);
                else if (path == 6) emanuelTrust.AdjustTrust(-15);
                break;
            case 9:
                if (path == 1) emanuelTrust.AdjustTrust(12);
                else if (path == 2) emanuelTrust.AdjustTrust(60);
                else if (path == 3) emanuelTrust.AdjustTrust(80);
                else if (path == 4) emanuelTrust.AdjustTrust(10);
                else if (path == 5) emanuelTrust.AdjustTrust(-50);
                else if (path == 6) emanuelTrust.AdjustTrust(-15);
                break;
            case 10:
                if (path == 1) emanuelTrust.AdjustTrust(12);
                else if (path == 2) emanuelTrust.AdjustTrust(60);
                else if (path == 3) emanuelTrust.AdjustTrust(80);
                else if (path == 4) emanuelTrust.AdjustTrust(10);
                else if (path == 5) emanuelTrust.AdjustTrust(-50);
                else if (path == 6) emanuelTrust.AdjustTrust(-15);
                break;
            default:
                Debug.LogWarning("Invalid dialogue layer for trust adjustment.");
                break;
        }
    }

    IEnumerator EmmanuelResponse(int path)
    {
        int trustChange = 0;
        switch (dialogueLayer)
        {
            case 1: trustChange = path == 1 ? 10 : path == 2 ? -5 : path == 3 ? 5 : path == 4 ? -10 : path == 5 ? 2 : -7; break;
            case 2: trustChange = path == 1 ? 7 : path == 2 ? -3 : path == 3 ? 3 : path == 4 ? -8 : path == 5 ? 1 : -5; break;
            case 3: trustChange = path == 1 ? 5 : path == 2 ? -10 : path == 3 ? 2 : path == 4 ? 6 : path == 5 ? -4 : -9; break;
            case 4: trustChange = path == 1 ? -8 : path == 2 ? -4 : path == 3 ? 6 : path == 4 ? 7 : path == 5 ? -3 : -10; break;
            case 5: trustChange = path == 1 ? -6 : path == 2 ? -8 : path == 3 ? 4 : path == 4 ? 9 : path == 5 ? -2 : -12; break;
            case 6: trustChange = path == 1 ? 12 : path == 2 ? 6 : path == 3 ? 8 : path == 4 ? 10 : path == 5 ? -5 : -15; break;
            case 7: trustChange = path == 1 ? 15 : path == 2 ? -5 : path == 3 ? 8 : path == 4 ? 3 : path == 5 ? 5 : -10; break;
            case 8: trustChange = path == 1 ? 10 : path == 2 ? -8 : path == 3 ? 7 : path == 4 ? 5 : path == 5 ? 2 : -12; break;
            case 9: trustChange = path == 1 ? 12 : path == 2 ? -10 : path == 3 ? 8 : path == 4 ? 6 : path == 5 ? 4 : -15; break;
            case 10: trustChange = path == 1 ? 10 : path == 2 ? 5 : path == 3 ? 7 : path == 4 ? 15 : path == 5 ? -5 : -10; break;
        }

        string[] positiveResponses = new string[]
        {
            "Emanuel: \"Huh, not bad for a rookie. Don’t let it get to your head still plenty of room to flop.\"",
            "Emanuel: \"Well, well, you’ve got some fire. Maybe you’re not a complete waste of my time.\"",
            "Emanuel: \"Alright, I’ll bite you’re sharper than you look. Don’t ruin it now.\"",
            "Emanuel: \"Fine, you’ve earned a nod. Keep it up, and I might not regret this.\"",
            "Emanuel: \"Not terrible, I suppose. You’re almost worth the air you’re breathing.\""
        };

        string[] negativeResponses = new string[]
        {
            "Emanuel: \"Wow, you’re denser than a brick wall. Try not to embarrass yourself next time.\"",
            "Emanuel: \"Pathetic. You’re a walking waste of space get out of my sight.\"",
            "Emanuel: \"Brilliant, another idiot move. You’re making this too easy for me.\"",
            "Emanuel: \"Oh, please, spare me your nonsense. You’re hopeless end of story.\"",
            "Emanuel: \"What a letdown. You’re about as useful as a broken sandal.\""
        };

        if (trustChange > 0)
        {
            int randomIndex = Random.Range(0, positiveResponses.Length);
            yield return StartCoroutine(TypeSentence(positiveResponses[randomIndex]));
        }
        else if (trustChange < 0)
        {
            int randomIndex = Random.Range(0, negativeResponses.Length);
            yield return StartCoroutine(TypeSentence(negativeResponses[randomIndex]));
        }
        else
        {
            yield return StartCoroutine(TypeSentence("Emanuel: \"…Right. Whatever.\""));
        }
    }

    IEnumerator HandleLayer1(int path)
    {
        switch (path)
        {
            case 1: yield return StartCoroutine(TypeSentence("Player: \"Helping my tía, señor!\"")); break;
            case 2: yield return StartCoroutine(TypeSentence("Player: \"Soft? You’re a potato!\"")); break;
            case 3: yield return StartCoroutine(TypeSentence("Player: \"Spain’s wild just like me!\"")); break;
            case 4: yield return StartCoroutine(TypeSentence("Player: \"Passing through, chill out!\"")); break;
            case 5: yield return StartCoroutine(TypeSentence("Player: \"Mind your mangoes, man!\"")); break;
            case 6: yield return StartCoroutine(TypeSentence("Player: \"Tanzania’s my playground!\"")); break;
        }
    }

    IEnumerator HandleLayer2(int path)
    {
        switch (path)
        {
            case 1: yield return StartCoroutine(TypeSentence("Player: \"Heard it’s a thief magnet!\"")); break;
            case 2: yield return StartCoroutine(TypeSentence("Player: \"You’re the ringmaster here!\"")); break;
            case 3: yield return StartCoroutine(TypeSentence("Player: \"Thieves love your menu!\"")); break;
            case 4: yield return StartCoroutine(TypeSentence("Player: \"What a clown show!\"")); break;
            case 5: yield return StartCoroutine(TypeSentence("Player: \"Not my circus, not my monkeys!\"")); break;
            case 6: yield return StartCoroutine(TypeSentence("Player: \"Respect? You’re joking!\"")); break;
        }
    }

    IEnumerator HandleLayer3(int path)
    {
        switch (path)
        {
            case 1: yield return StartCoroutine(TypeSentence("Player: \"Where’s the bald duo?\"")); break;
            case 2: yield return StartCoroutine(TypeSentence("Player: \"Harsh? You’re a bully!\"")); break;
            case 3: yield return StartCoroutine(TypeSentence("Player: \"Spill the tea, Emmanuel!\"")); break;
            case 4: yield return StartCoroutine(TypeSentence("Player: \"I’ll track ‘em solo!\"")); break;
            case 5: yield return StartCoroutine(TypeSentence("Player: \"Gossip’s for grannies!\"")); break;
            case 6: yield return StartCoroutine(TypeSentence("Player: \"Useless? Like you?\"")); break;
        }
    }

    IEnumerator HandleLayer4(int path)
    {
        switch (path)
        {
            case 1: yield return StartCoroutine(TypeSentence("Player: \"What’s your deal, drama king?\"")); break;
            case 2: yield return StartCoroutine(TypeSentence("Player: \"Rude much, babysitter?\"")); break;
            case 3: yield return StartCoroutine(TypeSentence("Player: \"Ease up, we’re cool!\"")); break;
            case 4: yield return StartCoroutine(TypeSentence("Player: \"I’ll navigate, genius!\"")); break;
            case 5: yield return StartCoroutine(TypeSentence("Player: \"You’re a total flop!\"")); break;
            case 6: yield return StartCoroutine(TypeSentence("Player: \"Brain? You first!\"")); break;
        }
    }

    IEnumerator HandleLayer5(int path)
    {
        switch (path)
        {
            case 1: yield return StartCoroutine(TypeSentence("Player: \"Hilarious, you clown!\"")); break;
            case 2: yield return StartCoroutine(TypeSentence("Player: \"Lame comeback, try harder!\"")); break;
            case 3: yield return StartCoroutine(TypeSentence("Player: \"Cut the act, diva!\"")); break;
            case 4: yield return StartCoroutine(TypeSentence("Player: \"Fine, you’re forgiven!\"")); break;
            case 5: yield return StartCoroutine(TypeSentence("Player: \"Jerk status: confirmed!\"")); break;
            case 6: yield return StartCoroutine(TypeSentence("Player: \"Chill, you big baby!\"")); break;
        }
    }

    IEnumerator HandleLayer6(int path)
    {
        switch (path)
        {
            case 1: yield return StartCoroutine(TypeSentence("Player: \"Thanks, drama queen!\"")); break;
            case 2: yield return StartCoroutine(TypeSentence("Player: \"You’re a walking soap opera!\"")); break;
            case 3: yield return StartCoroutine(TypeSentence("Player: \"Let’s bury this circus!\"")); break;
            case 4: yield return StartCoroutine(TypeSentence("Player: \"Michael’s my man, thanks!\"")); break;
            case 5: yield return StartCoroutine(TypeSentence("Player: \"Rough day? Cry me a river!\"")); break;
            case 6: yield return StartCoroutine(TypeSentence("Player: \"No tricks? I’ll believe it when pigs fly!\"")); break;
        }
    }
    IEnumerator HandleLayer7(int path)
    {
        switch (path)
        {
            case 1: yield return StartCoroutine(TypeSentence("Player: \"Spain contacts? Nice!\"")); break;
            case 2: yield return StartCoroutine(TypeSentence("Player: \"Don’t tease, deliver!\"")); break;
            case 3: yield return StartCoroutine(TypeSentence("Player: \"Make it happen, big shot!\"")); break;
            case 4: yield return StartCoroutine(TypeSentence("Player: \"Cool, I’ll wait!\"")); break;
            case 5: yield return StartCoroutine(TypeSentence("Player: \"Matias and David, huh?\"")); break;
            case 6: yield return StartCoroutine(TypeSentence("Player: \"Prove it, loudmouth!\"")); break;
        }
    }

    IEnumerator HandleLayer8(int path)
    {
        switch (path)
        {
            case 1: yield return StartCoroutine(TypeSentence("Player: \"Hook me up, then!\"")); break;
            case 2: yield return StartCoroutine(TypeSentence("Player: \"Stop dangling carrots!\"")); break;
            case 3: yield return StartCoroutine(TypeSentence("Player: \"Spain’s my goal, help!\"")); break;
            case 4: yield return StartCoroutine(TypeSentence("Player: \"Sounds promising!\"")); break;
            case 5: yield return StartCoroutine(TypeSentence("Player: \"Favor? Use it!\"")); break;
            case 6: yield return StartCoroutine(TypeSentence("Player: \"Connections? Show me!\"")); break;
        }
    }

    IEnumerator HandleLayer9(int path)
    {
        switch (path)
        {
            case 1: yield return StartCoroutine(TypeSentence("Player: \"Sweet, call them!\"")); break;
            case 2: yield return StartCoroutine(TypeSentence("Player: \"Don’t mess this up!\"")); break;
            case 3: yield return StartCoroutine(TypeSentence("Player: \"Spain’s waiting, do it!\"")); break;
            case 4: yield return StartCoroutine(TypeSentence("Player: \"Appreciate it, maybe!\"")); break;
            case 5: yield return StartCoroutine(TypeSentence("Player: \"Nice move, Emanuel!\"")); break;
            case 6: yield return StartCoroutine(TypeSentence("Player: \"Still think you’re bluffing!\"")); break;
        }
    }

    IEnumerator HandleLayer10(int path)
    {
        switch (path)
        {
            case 1: yield return StartCoroutine(TypeSentence("Player: \"Great, I’m out!\"")); break;
            case 2: yield return StartCoroutine(TypeSentence("Player: \"Soap opera’s over!\"")); break;
            case 3: yield return StartCoroutine(TypeSentence("Player: \"Thanks, I guess!\"")); break;
            case 4: yield return StartCoroutine(TypeSentence("Player: \"Spain here I come!\"")); break;
            case 5: yield return StartCoroutine(TypeSentence("Player: \"Don’t back out now!\"")); break;
            case 6: yield return StartCoroutine(TypeSentence("Player: \"Later, big talker!\"")); break;
        }
    }

    public void EndDialogue()
    {
        dialogueCanvas.gameObject.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        onDialogueEnd?.Invoke();
        isDialogueCompleted = true;
    }
}