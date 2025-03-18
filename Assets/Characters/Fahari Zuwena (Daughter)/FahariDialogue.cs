using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class FahariDialogue : MonoBehaviour
{
    // Public variables for UI elements and dialogue control.
    public TMP_Text dialogueText; // Text element to display dialogue.
    public Button path1Button, path2Button, path3Button, path4Button, path5Button, path6Button; // Buttons for player choices.
    public Slider timerSlider; // Slider to visualize the dialogue timer.
    [SerializeField] private Canvas dialogueCanvas; // Canvas containing the dialogue UI.
    [SerializeField] private FahariTrust wealthyGirl; // Fahari's trust system.
    public UnityEvent onDialogueEnd; // Event triggered when dialogue ends.

    // Audio fields for typewriter and button press sounds.
    [SerializeField] private AudioSource audioSource; // Audio source for playing sounds.
    [SerializeField] private AudioClip typeSoundClip; // Sound for typewriter effect.
    [SerializeField] private AudioClip buttonPressClip; // Sound for button presses.

    // Private variables for dialogue state and control.
    private bool isTyping = false; // Flag to track if text is being typed.
    private string fullSentence = ""; // Stores the full sentence to be displayed.
    private bool isPlayerResponseComplete = false; // Flag to track if player response is complete.
    public int dialogueLayer = 1; // Current layer of the dialogue.
    private bool isPlayerTurn = false; // Flag to track if it's the player's turn.
    private int maxDialogueLayer = 6; // Maximum dialogue layer.
    private float timer = 20f; // Dialogue timer.
    private bool isTimerRunning = false; // Flag to track if the timer is running.
    private bool isDialogueCompleted = false; // Track dialogue completion
    public bool isPlayerInRange = false; // Track if the player is in range

    void Start()
    {
        // Initially hide the dialogue canvas.
        dialogueCanvas.gameObject.SetActive(false);

        // Add listeners to the path buttons.
        path1Button.onClick.AddListener(() => ChoosePath(1));
        path2Button.onClick.AddListener(() => ChoosePath(2));
        path3Button.onClick.AddListener(() => ChoosePath(3));
        path4Button.onClick.AddListener(() => ChoosePath(4));
        path5Button.onClick.AddListener(() => ChoosePath(5));
        path6Button.onClick.AddListener(() => ChoosePath(6));

        // Initialize the timer slider.
        timerSlider.maxValue = timer;
        timerSlider.value = timer;
    }

    public void ResetDialogue()
    {
        // Reset dialogue layer and completion status, then start dialogue.
        dialogueLayer = 1;
        isDialogueCompleted = false;
        StartDialogue();
    }

    public void SetPlayerInRange(bool inRange)
    {
        // Set the player in range flag.
        isPlayerInRange = inRange;
    }

    void Update()
    {
        // Start dialogue if player is in range and presses 'E'.
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && !dialogueCanvas.gameObject.activeSelf)
        {
            StartDialogue();
        }

        // End dialogue if player is out of range.
        if (dialogueCanvas.gameObject.activeSelf && !isPlayerInRange)
        {
            EndDialogue();
        }

        // Skip typing animation if space is pressed.
        if (isTyping && Input.GetKeyDown(KeyCode.Space))
        {
            isTyping = false;
            dialogueText.text = fullSentence;
        }

        // Proceed to next dialogue layer if space is pressed after player response.
        if (isPlayerResponseComplete && Input.GetKeyDown(KeyCode.Space))
        {
            isPlayerResponseComplete = false;
            dialogueLayer++;
            isPlayerTurn = false;
            SetDialogueLayer();
        }

        // Update timer and handle timer expiration.
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
        // Handle timer expiration: adjust trust, display message, and proceed.
        Debug.Log("Timer expired! Fahari’s patience snaps—lose 6 trust.");
        wealthyGirl.AdjustTrust(-15);
        StartCoroutine(TypeSentence("Fahari: \"Silent as a dead fish? Pathetic. My time’s worth more than your excuses!\""));
        ProceedToNextLayer();
    }

    void StartTimer()
    {
        // Start the dialogue timer.
        timer = 20f;
        timerSlider.value = timer;
        isTimerRunning = true;
    }

    void StopTimer()
    {
        // Stop the dialogue timer.
        isTimerRunning = false;
    }

    private IEnumerator TypeSentence(string sentence)
    {
        // Set typing flag to true.
        isTyping = true;
        // Store the full sentence.
        fullSentence = sentence;
        // Clear the dialogue text.
        dialogueText.text = "";
        // Iterate through each character in the sentence.
        foreach (char letter in sentence.ToCharArray())
        {
            // If typing is interrupted, set the full sentence and break.
            if (!isTyping)
            {
                dialogueText.text = sentence;
                break;
            }
            // Append the current letter to the dialogue text.
            dialogueText.text += letter;

            // Play typewriter sound for non-space characters.
            if (letter != ' ' && audioSource != null && typeSoundClip != null)
            {
                audioSource.PlayOneShot(typeSoundClip);
            }
            // Wait for a short duration.
            yield return new WaitForSeconds(0.05f);
        }
        // Set typing flag to false.
        isTyping = false;
        // If it's the player's turn, set the response complete flag.
        if (isPlayerTurn) isPlayerResponseComplete = true;
    }

    public void StartDialogue()
    {
        // If dialogue is already completed, exit.
        if (isDialogueCompleted) return;
        // Show the dialogue canvas.
        dialogueCanvas.gameObject.SetActive(true);
        // Make the cursor visible and unlock it.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        // Set player turn to false.
        isPlayerTurn = false;
        // Set the dialogue layer.
        SetDialogueLayer();
    }

    public bool IsDialogueComplete()
    {
        // Return the dialogue completion status.
        return isDialogueCompleted;
    }

    void SetDialogueLayer()
    {
        // If it's not the player's turn.
        if (!isPlayerTurn)
        {
            // If the dialogue layer exceeds the maximum layer, end the dialogue.
            if (dialogueLayer > maxDialogueLayer)
            {
                EndDialogue();
                return;
            }

            // Set the dialogue text based on the current layer.
            switch (dialogueLayer)
            {
                case 1:
                    StartCoroutine(TypeSentence("Fahari: \"Well, look at this—two dusty tourists crashing my restaurant. Who are you, and why are you ruining my vibe?\""));
                    break;
                case 2:
                    StartCoroutine(TypeSentence("Fahari: \"So you’ve got a story? Great. Explain why you’re here, or I’ll have you tossed out faster than cheap wine!\""));
                    break;
                case 3:
                    StartCoroutine(TypeSentence("Fahari: \"How’d you even slink past my guards? Spill it, or I’ll assume you’re thieves too!\""));
                    break;
                case 4:
                    StartCoroutine(TypeSentence("Fahari: \"What’s your game? You’re after something—don’t lie to me, I’m not your grandma!\""));
                    break;
                case 5:
                    StartCoroutine(TypeSentence("Fahari: \"This is my turf. You’re begging for trouble—give me one reason not to call the cops!\""));
                    break;
                case 6:
                    StartCoroutine(TypeSentence("Fahari: \"Time’s up, geniuses. What’s your next move before I ruin your little holiday?\""));
                    break;
            }
            // Wait for the NPC dialogue to finish.
            StartCoroutine(WaitForNPCDialogue());
        }
    }

    IEnumerator WaitForNPCDialogue()
    {
        // Wait until the NPC dialogue is finished typing.
        yield return new WaitUntil(() => !isTyping);
        // Set the player's turn.
        SetPlayerTurn();
    }

    void SetPlayerTurn()
    {
        isPlayerTurn = true;
        switch (dialogueLayer)
        {
            case 1:
                SetButtonLabels("David: My phone got stolen—I’m tracking it!",
                                "Matias: Chill, we’re just lost tourists!",
                                "David: This is my homeland, I belong here!",
                                "Matias: Your vibe? We’re the spice!",
                                "David: Someone here knows something!",
                                "Matias: We’re here for the drama!");
                break;
            case 2:
                SetButtonLabels("David: My phone’s in here—help me!",
                                "Matias: Looking for a thrill, found you!",
                                "David: I’m reclaiming my roots, deal with it!",
                                "Matias: Your wine’s safe, relax!",
                                "David: Thieves hit us—check your staff!",
                                "Matias: To outsmart the locals!");
                break;
            case 3:
                SetButtonLabels("David: I’m sneaky—it’s personal!",
                                "Matias: Your guards? Napping!",
                                "David: Years of dodging trouble!",
                                "Matias: We’re pros at chaos!",
                                "David: I know this place’s secrets!",
                                "Matias: Walked in like we own it!");
                break;
            case 4:
                SetButtonLabels("David: My phone, my mission!",
                                "Matias: A game? You’re the prize!",
                                "David: Justice for my heritage!",
                                "Matias: To mess with your head!",
                                "David: Answers, not your silverware!",
                                "Matias: Fame in Tanzania!");
                break;
            case 5:
                SetButtonLabels("David: Worth it for my phone!",
                                "Matias: Trouble’s my middle name!",
                                "David: I’ll fight for my roots!",
                                "Matias: Cops? I dare you!",
                                "David: You’re hiding something!",
                                "Matias: I thrive on this chaos!");
                break;
            case 6:
                SetButtonLabels("David: Get my phone, I’m out!",
                                "Matias: Outrun you and win!",
                                "David: Prove I belong here!",
                                "Matias: Laugh and vanish!",
                                "David: Expose your shady staff!",
                                "Matias: Epic exit, no regrets!");
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
        // Set the text labels for each button using TMP_Text components.
        path1Button.GetComponentInChildren<TMP_Text>().text = path1Text;
        path2Button.GetComponentInChildren<TMP_Text>().text = path2Text;
        path3Button.GetComponentInChildren<TMP_Text>().text = path3Text;
        path4Button.GetComponentInChildren<TMP_Text>().text = path4Text;
        path5Button.GetComponentInChildren<TMP_Text>().text = path5Text;
        path6Button.GetComponentInChildren<TMP_Text>().text = path6Text;
    }

    void ChoosePath(int path)
    {
        // Play button press sound effect.
        if (audioSource != null && buttonPressClip != null)
        {
            audioSource.PlayOneShot(buttonPressClip);
        }

        // Stop the dialogue timer.
        StopTimer();
        // If there's time left on the timer, adjust Fahari's trust.
        if (timer > 0f)
        {
            Debug.Log("Quick reply! Fahari respects it—gain 5 trust.");
            wealthyGirl.AdjustTrust(5);
        }

        // Hide all path buttons.
        path1Button.gameObject.SetActive(false);
        path2Button.gameObject.SetActive(false);
        path3Button.gameObject.SetActive(false);
        path4Button.gameObject.SetActive(false);
        path5Button.gameObject.SetActive(false);
        path6Button.gameObject.SetActive(false);

        // Start coroutine to handle dialogue layer with player response.
        StartCoroutine(HandleLayerWithResponse(path));
    }

    IEnumerator HandleLayerWithResponse(int path)
    {
        // Handle the current dialogue layer based on the player's chosen path.
        switch (dialogueLayer)
        {
            case 1: yield return StartCoroutine(HandleLayer1(path)); break;
            case 2: yield return StartCoroutine(HandleLayer2(path)); break;
            case 3: yield return StartCoroutine(HandleLayer3(path)); break;
            case 4: yield return StartCoroutine(HandleLayer4(path)); break;
            case 5: yield return StartCoroutine(HandleLayer5(path)); break;
            case 6: yield return StartCoroutine(HandleLayer6(path)); break;
        }
        // Adjust Fahari's trust based on the player's path.
        AdjustTrustBasedOnPath(path);
        // Display Fahari's response based on the player's path.
        yield return StartCoroutine(FahariResponse(path));
        // Proceed to the next dialogue layer.
        ProceedToNextLayer();
    }

    void ProceedToNextLayer()
    {
        // Increment the dialogue layer and handle dialogue ending.
        dialogueLayer++;
        if (dialogueLayer > maxDialogueLayer) EndDialogue();
        else SetDialogueLayer();
    }

    void AdjustTrustBasedOnPath(int path)
    {
        if (wealthyGirl == null) { Debug.LogError("WealthyGirl reference missing!"); return; }
        switch (dialogueLayer)
        {
            case 1:
                if (path == 1) wealthyGirl.AdjustTrust(60);     // David’s focus
                else if (path == 2) wealthyGirl.AdjustTrust(-55); // Matias’ flippancy
                else if (path == 3) wealthyGirl.AdjustTrust(-30);  // David’s pride
                else if (path == 4) wealthyGirl.AdjustTrust(-10);// Matias’ sass
                else if (path == 5) wealthyGirl.AdjustTrust(20);  // David’s hint
                else if (path == 6) wealthyGirl.AdjustTrust(-27); // Matias’ drama
                break;
            case 2:
                if (path == 1) wealthyGirl.AdjustTrust(70);
                else if (path == 2) wealthyGirl.AdjustTrust(-3);
                else if (path == 3) wealthyGirl.AdjustTrust(3);
                else if (path == 4) wealthyGirl.AdjustTrust(-38);
                else if (path == 5) wealthyGirl.AdjustTrust(10);
                else if (path == 6) wealthyGirl.AdjustTrust(-5);
                break;
            case 3:
                if (path == 1) wealthyGirl.AdjustTrust(50);
                else if (path == 2) wealthyGirl.AdjustTrust(10);
                else if (path == 3) wealthyGirl.AdjustTrust(20);
                else if (path == 4) wealthyGirl.AdjustTrust(60);
                else if (path == 5) wealthyGirl.AdjustTrust(-40);
                else if (path == 6) wealthyGirl.AdjustTrust(-90);
                break;
            case 4:
                if (path == 1) wealthyGirl.AdjustTrust(80);
                else if (path == 2) wealthyGirl.AdjustTrust(-40);
                else if (path == 3) wealthyGirl.AdjustTrust(60);
                else if (path == 4) wealthyGirl.AdjustTrust(-70);
                else if (path == 5) wealthyGirl.AdjustTrust(30);
                else if (path == 6) wealthyGirl.AdjustTrust(-10);
                break;
            case 5:
                if (path == 1) wealthyGirl.AdjustTrust(60);
                else if (path == 2) wealthyGirl.AdjustTrust(80);
                else if (path == 3) wealthyGirl.AdjustTrust(40);
                else if (path == 4) wealthyGirl.AdjustTrust(-90);
                else if (path == 5) wealthyGirl.AdjustTrust(20);
                else if (path == 6) wealthyGirl.AdjustTrust(12);
                break;
            case 6:
                if (path == 1) wealthyGirl.AdjustTrust(12);
                else if (path == 2) wealthyGirl.AdjustTrust(60);
                else if (path == 3) wealthyGirl.AdjustTrust(80);
                else if (path == 4) wealthyGirl.AdjustTrust(-10);
                else if (path == 5) wealthyGirl.AdjustTrust(50);
                else if (path == 6) wealthyGirl.AdjustTrust(15);
                break;
            default:
                Debug.LogWarning("Invalid dialogue layer!");
                break;
        }
    }

    IEnumerator FahariResponse(int path)
    {
        int trustChange = wealthyGirl.GetLastTrustChange();
        string[] positiveResponses = {
            "Fahari: \"Huh, not total losers. You might survive this dump!\"",
            "Fahari: \"Sharp! I’d hire you if you weren’t so scruffy!\"",
            "Fahari: \"Okay, I’m impressed—don’t let it go to your head!\""
        };
        string[] negativeResponses = {
            "Fahari: \"Wow, dumber than a bag of rocks. Get out!\"",
            "Fahari: \"Useless as a broken fork—waste my time again!\"",
            "Fahari: \"Pathetic! Even my dog’s got better lines!\""
        };
        if (trustChange > 0)
            yield return StartCoroutine(TypeSentence(positiveResponses[Random.Range(0, positiveResponses.Length)]));
        else if (trustChange < 0)
            yield return StartCoroutine(TypeSentence(negativeResponses[Random.Range(0, negativeResponses.Length)]));
    }

    IEnumerator HandleLayer1(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"My phone got stolen—I’m tracking it here!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Chill, we’re just lost tourists—don’t freak!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"This is my homeland, I belong here more than you!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Your vibe? We’re the spice in this snooze-fest!\""));
        else if (path == 5) yield return StartCoroutine(TypeSentence("David: \"Someone here knows something—spill it!\""));
        else if (path == 6) yield return StartCoroutine(TypeSentence("Matias: \"We’re here for the drama—thanks for starring!\""));
    }

    IEnumerator HandleLayer2(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"My phone’s in here—help me or step aside!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Looking for a thrill, and I found you, princess!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"I’m reclaiming my roots—deal with it!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Your wine’s safe, relax, it’s not my type!\""));
        else if (path == 5) yield return StartCoroutine(TypeSentence("David: \"Thieves hit us—check your shady staff!\""));
        else if (path == 6) yield return StartCoroutine(TypeSentence("Matias: \"To outsmart the locals—you’re first!\""));
    }

    IEnumerator HandleLayer3(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"I’m sneaky—it’s personal, not your business!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Your guards? Napping on the job!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Years of dodging trouble—try me!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"We’re pros at chaos—deal with it!\""));
        else if (path == 5) yield return StartCoroutine(TypeSentence("David: \"I know this place’s secrets—do you?\""));
        else if (path == 6) yield return StartCoroutine(TypeSentence("Matias: \"Walked in like we own it—because we do!\""));
    }

    IEnumerator HandleLayer4(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"My phone, my mission—back off!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"A game? You’re the prize, sweetheart!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Justice for my heritage—step up or shut up!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"To mess with your head—winning so far!\""));
        else if (path == 5) yield return StartCoroutine(TypeSentence("David: \"Answers, not your silverware—relax!\""));
        else if (path == 6) yield return StartCoroutine(TypeSentence("Matias: \"Fame in Tanzania—watch me shine!\""));
    }

    IEnumerator HandleLayer5(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Worth it for my phone—try me!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Trouble’s my middle name—bring it!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"I’ll fight for my roots—you’re nothing!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Cops? I dare you—make my day!\""));
        else if (path == 5) yield return StartCoroutine(TypeSentence("David: \"You’re hiding something—spill it!\""));
        else if (path == 6) yield return StartCoroutine(TypeSentence("Matias: \"I thrive on this chaos—thanks!\""));
    }

    IEnumerator HandleLayer6(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"Get my phone, I’m out—done here!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"Outrun you and win—watch me!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"Prove I belong here—deal with it!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"Laugh and vanish—bye, princess!\""));
        else if (path == 5) yield return StartCoroutine(TypeSentence("David: \"Expose your shady staff—truth hurts!\""));
        else if (path == 6) yield return StartCoroutine(TypeSentence("Matias: \"Epic exit, no regrets—later!\""));
    }

    public void EndDialogue()
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

        if (wealthyGirl != null) wealthyGirl.CheckTrustLevelAtEnd();
        else Debug.LogError("WealthyGirl reference missing!");
        onDialogueEnd?.Invoke();
        isDialogueCompleted = true;
    }
}
