using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class AdilaDialogueTree : MonoBehaviour
{
    [Header("UI References")]
    // Text component to display dialogue.
    public TMP_Text dialogueText;
    // Buttons for player dialogue choices.
    public Button path1Button, path2Button, path3Button, path4Button;
    // Slider to display dialogue timer.
    public Slider timerSlider;
    // Canvas containing the dialogue UI.
    [SerializeField] private Canvas dialogueCanvas;

    [Header("Audio Settings (Optional)")]
    // Audio source for playing sounds.
    [SerializeField] private AudioSource audioSource;
    // Audio clip for typing sound.
    [SerializeField] private AudioClip typeSoundClip;
    // Audio clip for button press sound.
    [SerializeField] private AudioClip buttonPressClip;

    // Flag indicating if dialogue is being typed.
    private bool isTyping = false;
    // Full sentence to be typed.
    private string fullSentence = "";
    // Flag indicating if player response is complete.
    private bool isPlayerResponseComplete = false;

    // Current dialogue layer.
    public int dialogueLayer = 1;
    // Flag indicating if it is the player's turn to respond.
    private bool isPlayerTurn = false;

    // Reference to AdilaMiziki script.
    public AdilaMiziki adilaMiziki;
    // Event triggered when dialogue ends.
    public UnityEvent onDialogueEnd;

    // Maximum dialogue layer.
    private int maxDialogueLayer = 8;
    // Timer duration.
    private float timer = 15f;
    // Flag indicating if timer is running.
    private bool isTimerRunning = false;

    // Flag indicating if dialogue is completed.
    private bool isDialogueCompleted = false;
    // Flag indicating if player is in range.
    private bool isPlayerInRange = false;

    // Flag indicating if interaction is latched.
    private bool interactionLatched = false;
    // Property to access the dialogue canvas.
    public Canvas DialogueCanvas => dialogueCanvas;

    // Called when the script starts.
    void Start()
    {
        // Add listeners to dialogue choice buttons.
        path1Button.onClick.AddListener(() => ChoosePath(1));
        path2Button.onClick.AddListener(() => ChoosePath(2));
        path3Button.onClick.AddListener(() => ChoosePath(3));
        path4Button.onClick.AddListener(() => ChoosePath(4));

        // Initialize the timer slider.
        timerSlider.maxValue = timer;
        timerSlider.value = timer;

        // Hide the dialogue canvas initially.
        dialogueCanvas.gameObject.SetActive(false);
    }

    // Resets the dialogue tree.
    public void ResetDialogue()
    {
        // Stop all running coroutines.
        StopAllCoroutines();
        // Reset dialogue layer.
        dialogueLayer = 1;
        // Reset dialogue completion flag.
        isDialogueCompleted = false;
        // Reset player turn flag.
        isPlayerTurn = false;
        // Start the dialogue.
        StartDialogue();
    }

    // Called every frame.
    void Update()
    {
        // Start dialogue if player is in range and presses 'E'.
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && !dialogueCanvas.gameObject.activeSelf)
        {
            StartDialogue();
        }

        // Skip typing animation if 'Space' is pressed.
        if (isTyping && Input.GetKeyDown(KeyCode.Space))
        {
            isTyping = false;
            dialogueText.text = fullSentence;
        }

        // Proceed to next dialogue layer if player response is complete and 'Space' is pressed.
        if (isPlayerResponseComplete && Input.GetKeyDown(KeyCode.Space))
        {
            isPlayerResponseComplete = false;
            dialogueLayer++;
            isPlayerTurn = false;
            SetDialogueLayer();
        }

        // Update timer if it is running.
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

    // Called when the dialogue timer expires.
    void OnTimerExpired()
    {
        // Adjust Miziki level if AdilaMiziki script is assigned.
        if (adilaMiziki != null)
        {
            adilaMiziki.AdjustMiziki(-10);
        }
        // Display a default message.
        StartCoroutine(TypeSentence("Adila: \"Hmph, wasting my time with silence? I don’t play games with strangers speak or leave!\""));
        // Proceed to the next dialogue layer.
        ProceedToNextLayer();
    }

    // Starts the dialogue timer.
    void StartTimer()
    {
        timer = 15f;
        timerSlider.value = timer;
        isTimerRunning = true;
    }

    // Stops the dialogue timer.
    void StopTimer()
    {
        isTimerRunning = false;
    }

    // Starts the dialogue.
    public void StartDialogue()
    {
        // Return if dialogue is already completed.
        if (isDialogueCompleted) return;
        // Show the dialogue canvas.
        dialogueCanvas.gameObject.SetActive(true);
        // Show the cursor.
        Cursor.visible = true;
        // Unlock the cursor.
        Cursor.lockState = CursorLockMode.None;
        // Set player turn to false.
        isPlayerTurn = false;
        // Set the initial dialogue layer.
        SetDialogueLayer();
    }


    // Coroutine to type out a sentence character by character.
    private IEnumerator TypeSentence(string sentence)
    {
        // Set the typing flag to true.
        isTyping = true;
        // Store the full sentence.
        fullSentence = sentence;
        // Clear the dialogue text.
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }
        else
        {
            Debug.LogError("dialogueText is null!");
            yield break;
        }
        // Iterate through each character in the sentence.
        foreach (char letter in sentence.ToCharArray())
        {
            // If typing is interrupted, display the full sentence and exit the loop.
            if (!isTyping)
            {
                dialogueText.text = sentence;
                break;
            }
            // Append the current letter to the dialogue text.
            dialogueText.text += letter;
            // Play typing sound for non-space characters.
            if (letter != ' ' && audioSource != null && typeSoundClip != null)
            {
                audioSource.PlayOneShot(typeSoundClip);
            }
            // Wait for a short duration before typing the next character.
            yield return new WaitForSeconds(0.05f);
        }
        // Set the typing flag to false.
        isTyping = false;
        // Set player response complete flag if it's the player's turn.
        if (isPlayerTurn) isPlayerResponseComplete = true;
    }

    // Method to check if the dialogue is complete.
    public bool IsDialogueComplete()
    {
        return isDialogueCompleted;
    }

    // Method to set whether the player is in range of the NPC.
    public void SetPlayerInRange(bool inRange)
    {
        // Set the player in range flag.
        isPlayerInRange = inRange;
        Debug.Log("Player in range: " + inRange);

        // Latch interaction if player enters range.
        if (inRange)
        {
            interactionLatched = true;
        }
        // Unlatch interaction and end dialogue if player exits range.
        else
        {
            interactionLatched = false;
            if (dialogueCanvas.gameObject.activeSelf)
            {
                EndDialogue();
            }
        }
    }

    // Method to set the current dialogue layer and display the corresponding dialogue.
    void SetDialogueLayer()
    {
        // Check if it's the NPC's turn to speak.
        if (!isPlayerTurn)
        {
            // End dialogue if the current layer exceeds the maximum layer.
            if (dialogueLayer > maxDialogueLayer)
            {
                EndDialogue();
                return;
            }

            // Display dialogue based on the current layer.
            switch (dialogueLayer)
            {
                case 1:
                    Debug.Log("Adila first line triggered!");
                    StartCoroutine(TypeSentence("Adila: \"You two! I’ve seen your kind skulking about. Don’t think I don’t notice foreigners sniffing around my friends’ restaurant. What’s your game?\""));
                    break;
                case 2:
                    StartCoroutine(TypeSentence("Adila: \"Lost your phones, did you? Convenient story too convenient. I hear whispers of gangs here. Are you their errand boys, or just fools caught in the net?\""));
                    break;
                case 3:
                    StartCoroutine(TypeSentence("Adila: \"Hmph, you’ve got quick tongues for outsiders. But I’m not Zuwena I don’t trust so easily. Tell me, who sent you to this market in the first place?\""));
                    break;
                case 4:
                    StartCoroutine(TypeSentence("Adila: \"A holiday, is it? Or a cover for something shadier? I’ve got eyes everywhere this restaurant isn’t your playground. What do you know of the thieves here?\""));
                    break;
                case 5:
                    StartCoroutine(TypeSentence("Adila: \"Fine, I’ll bite maybe you’re not masterminds. But ignorance doesn’t make you clean. I’m hunting the rats behind these thefts. Spill something useful, or I’ll dig deeper into you.\""));
                    break;
                case 6:
                    StartCoroutine(TypeSentence("Adila: \"So you claim innocence, but I’ve heard better lies from street rats. If you’re not with the gangs, prove it tell me what you saw in that market before the theft.\""));
                    break;
                case 7:
                    StartCoroutine(TypeSentence("Adila: \"Details, yes, but I’m still watching you. My instincts don’t fail me I’ll root out the filth here. Will you help me, or stay on my blacklist?\""));
                    break;
                case 8:
                    StartCoroutine(TypeSentence("Adila: \"Time’s up for games. I’ll find the truth with or without you. Choose your side now work with me, or I’ll assume the worst and make your stay here miserable.\""));
                    break;
            }
            // Wait for the NPC's dialogue to finish.
            StartCoroutine(WaitForNPCDialogue());
        }
    }

    // Coroutine to wait for the NPC's dialogue to finish typing.
    IEnumerator WaitForNPCDialogue()
    {
        // Wait until the typing animation is finished.
        yield return new WaitUntil(() => !isTyping);
        // Wait for a short delay before setting the player's turn.
        yield return new WaitForSeconds(0.5f);
        // Set the player's turn.
        SetPlayerTurn();
    }

    void SetPlayerTurn()
    {
        isPlayerTurn = true;
        switch (dialogueLayer)
        {
            case 1:
                SetButtonLabels("David: We’re just travelers, ma’am no game here!",
                                "Matias: Hey, chill! We’re not up to anything shady!",
                                "David: I’m Tanzanian, reconnecting with my roots!",
                                "Matias: What’s with the hostility? We’re victims too!");
                break;
            case 2:
                SetButtonLabels("David: We were robbed, not running errands!",
                                "Matias: Gangs? We’re the ones who got hit, lady!",
                                "David: I swear, we’re clean just trying to get help!",
                                "Matias: If you know something, why not help us out?");
                break;
            case 3:
                SetButtonLabels("David: No one sent us just exploring Zania!",
                                "Matias: It was my idea, okay? No big conspiracy!",
                                "David: We came to eat and enjoy, not cause trouble!",
                                "Matias: Why assume the worst? We’re not your enemy!");
                break;
            case 4:
                SetButtonLabels("David: Nothing shady just a holiday gone wrong!",
                                "Matias: Thieves? We’re looking for ours, not theirs!",
                                "David: I don’t know anything, but I’ll help if I can!",
                                "Matias: Why not team up? We’ve got a common foe!");
                break;
            case 5:
                SetButtonLabels("David: I’ll tell you what I know let’s work together!",
                                "Matias: Dig into us? We’ve got nothing to hide!",
                                "David: I saw some shady types maybe I can help?",
                                "Matias: Look, we’re not rats give us a break!");
                break;
            case 6:
                SetButtonLabels("David: Saw a guy in red lurking suspicious type!",
                                "Matias: It was chaos just a blur before the theft!",
                                "David: I’ll describe what I noticed calm down, okay?",
                                "Matias: Why’d I lie? I’ve got no reason to protect anyone!");
                break;
            case 7:
                SetButtonLabels("David: I’ll help you I don’t want trouble!",
                                "Matias: Blacklist? Geez, we’re on the same side!",
                                "David: Let me prove I’m legit give me a chance!",
                                "Matias: Fine, I’ll pitch in just stop glaring at us!");
                break;
            case 8:
                SetButtonLabels("David: I’m with you let’s find the real crooks!",
                                "Matias: Work together? Sure, but ease up on us!",
                                "David: I’ll cooperate just don’t ruin my trip!",
                                "Matias: I’d rather help than deal with your wrath!");
                break;
        }
        path1Button.gameObject.SetActive(true);
        path2Button.gameObject.SetActive(true);
        path3Button.gameObject.SetActive(true);
        path4Button.gameObject.SetActive(true);
        StartTimer();
    }

    // Method to set the text labels of the dialogue choice buttons.
    void SetButtonLabels(string path1Text, string path2Text, string path3Text, string path4Text)
    {
        // Set the text of each button's child TMP_Text component.
        path1Button.GetComponentInChildren<TMP_Text>().text = path1Text;
        path2Button.GetComponentInChildren<TMP_Text>().text = path2Text;
        path3Button.GetComponentInChildren<TMP_Text>().text = path3Text;
        path4Button.GetComponentInChildren<TMP_Text>().text = path4Text;
    }

    // Method to handle player's choice of dialogue path.
    void ChoosePath(int path)
    {
        // Play button press sound effect.
        if (audioSource != null && buttonPressClip != null)
        {
            audioSource.PlayOneShot(buttonPressClip);
        }

        // Stop the dialogue timer.
        StopTimer();
        // Adjust Miziki level if the player replied quickly.
        if (timer > 0f && adilaMiziki != null)
        {
            Debug.Log("Quick reply! Adila appreciates it gain 5 Miziki.");
            adilaMiziki.AdjustMiziki(5);
        }

        // Hide all dialogue choice buttons.
        path1Button.gameObject.SetActive(false);
        path2Button.gameObject.SetActive(false);
        path3Button.gameObject.SetActive(false);
        path4Button.gameObject.SetActive(false);

        // Start coroutine to handle dialogue layer with player response.
        StartCoroutine(HandleLayerWithResponse(path));
    }

    // Coroutine to handle dialogue layer with player response.
    IEnumerator HandleLayerWithResponse(int path)
    {
        // Call the corresponding layer handling coroutine based on the current dialogue layer.
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
        // Wait for 3 seconds before continuing.
        yield return new WaitForSeconds(3);
        // Adjust Miziki level based on player's chosen path.
        AdjustMizikiBasedOnPath(path);
        // Start coroutine to display Adila's response.
        yield return StartCoroutine(AdilaResponse(path));
        // Proceed to the next dialogue layer.
        ProceedToNextLayer();
    }

    // Method to proceed to the next dialogue layer.
    void ProceedToNextLayer()
    {
        // Increment the dialogue layer.
        dialogueLayer++;
        // End dialogue if the current layer exceeds the maximum layer.
        if (dialogueLayer > maxDialogueLayer)
            EndDialogue();
        // Otherwise, set the next dialogue layer.
        else
            SetDialogueLayer();
    }

    void AdjustMizikiBasedOnPath(int path)
    {
        if (adilaMiziki == null)
        {
            Debug.LogError("AdilaMiziki reference missing!");
            return;
        }

        int currentLayer = dialogueLayer; 

        switch (dialogueLayer)
        {
            case 1:
                if (path == 1) adilaMiziki.AdjustMiziki(-15);
                else if (path == 2) adilaMiziki.AdjustMiziki(-10);
                else if (path == 3) adilaMiziki.AdjustMiziki(15);
                else if (path == 4) adilaMiziki.AdjustMiziki(-18);
                break;
            case 2:
                if (path == 1) adilaMiziki.AdjustMiziki(23);
                else if (path == 2) adilaMiziki.AdjustMiziki(-25);
                else if (path == 3) adilaMiziki.AdjustMiziki(27);
                else if (path == 4) adilaMiziki.AdjustMiziki(22);
                break;
            case 3:
                if (path == 1) adilaMiziki.AdjustMiziki(34);
                else if (path == 2) adilaMiziki.AdjustMiziki(32);
                else if (path == 3) adilaMiziki.AdjustMiziki(-33);
                else if (path == 4) adilaMiziki.AdjustMiziki(-37);
                break;
            case 4:
                if (path == 1) adilaMiziki.AdjustMiziki(45);
                else if (path == 2) adilaMiziki.AdjustMiziki(43);
                else if (path == 3) adilaMiziki.AdjustMiziki(48);
                else if (path == 4) adilaMiziki.AdjustMiziki(46);
                break;
            case 5:
                if (path == 1) adilaMiziki.AdjustMiziki(50);
                else if (path == 2) adilaMiziki.AdjustMiziki(-52);
                else if (path == 3) adilaMiziki.AdjustMiziki(57);
                else if (path == 4) adilaMiziki.AdjustMiziki(-55);
                break;
            case 6:
                if (path == 1) adilaMiziki.AdjustMiziki(42);
                else if (path == 2) adilaMiziki.AdjustMiziki(40);
                else if (path == 3) adilaMiziki.AdjustMiziki(48);
                else if (path == 4) adilaMiziki.AdjustMiziki(44);
                break;
            case 7:
                if (path == 1) adilaMiziki.AdjustMiziki(10);
                else if (path == 2) adilaMiziki.AdjustMiziki(-55);
                else if (path == 3) adilaMiziki.AdjustMiziki(27);
                else if (path == 4) adilaMiziki.AdjustMiziki(28);
                break;
            case 8:
                if (path == 1) adilaMiziki.AdjustMiziki(45);
                else if (path == 2) adilaMiziki.AdjustMiziki(20);
                else if (path == 3) adilaMiziki.AdjustMiziki(50);
                else if (path == 4) adilaMiziki.AdjustMiziki(48);
                break;
        }
    }

    IEnumerator AdilaResponse(int path)
    {
        if (adilaMiziki == null)
        {
            Debug.LogError("AdilaMiziki reference missing!");
            yield break;
        }

        int mizikiChange = adilaMiziki.GetLastMizikiChange();
        string[] positiveResponses = new string[]
        {
            "Adila: \"Well now, that’s a start maybe you’re not all bad. Keep talking!\"",
            "Adila: \"Hmm… you might just be useful after all. Don’t disappoint me!\"",
            "Adila: \"Fine, I’ll give you a sliver of trust for now. Prove your worth!\""
        };
        string[] negativeResponses = new string[]
        {
            "Adila: \"Don’t waste my time with excuses I’m watching you closely!\"",
            "Adila: \"Tch, I’ve heard better from liars. You’re on thin ice, boys!\"",
            "Adila: \"Pathetic. If you’re hiding something, I’ll sniff it out soon enough!\""
        };

        if (mizikiChange > 0)
            yield return StartCoroutine(TypeSentence(positiveResponses[Random.Range(0, positiveResponses.Length)]));
        else if (mizikiChange < 0)
            yield return StartCoroutine(TypeSentence(negativeResponses[Random.Range(0, negativeResponses.Length)]));
    }

    IEnumerator HandleLayer1(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("Player: \"We’re just travelers, ma’am no game here!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Player: \"Hey, chill! We’re not up to anything shady!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("Player: \"I’m Tanzanian, reconnecting with my roots!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Player: \"What’s with the hostility? We’re victims too!\""));
    }

    IEnumerator HandleLayer2(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("Player: \"We were robbed, not running errands!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Player: \"Gangs? We’re the ones who got hit, lady!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("Player: \"I swear, we’re clean just trying to get help!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Player: \"If you know something, why not help us out?\""));
    }

    IEnumerator HandleLayer3(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("Player: \"No one sent us just exploring Zania!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Player: \"It was my idea, okay? No big conspiracy!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("Player: \"We came to eat and enjoy, not cause trouble!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Player: \"Why assume the worst? We’re not your enemy!\""));
    }

    IEnumerator HandleLayer4(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("Player: \"Nothing shady just a holiday gone wrong!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Player: \"Thieves? We’re looking for ours, not theirs!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("Player: \"I don’t know anything, but I’ll help if I can!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Player: \"Why not team up? We’ve got a common foe!\""));
    }

    IEnumerator HandleLayer5(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("Player: \"I’ll tell you what I know let’s work together!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Player: \"Dig into us? We’ve got nothing to hide!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("Player: \"I saw some shady types maybe I can help?\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Player: \"Look, we’re not rats give us a break!\""));
    }

    IEnumerator HandleLayer6(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("Player: \"Saw a guy in red lurking suspicious type!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Player: \"It was chaos just a blur before the theft!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("Player: \"I’ll describe what I noticed calm down, okay?\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Player: \"Why’d I lie? I’ve got no reason to protect anyone!\""));
    }

    IEnumerator HandleLayer7(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("Player: \"I’ll help you I don’t want trouble!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Player: \"Blacklist? Geez, we’re on the same side!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("Player: \"Let me prove I’m legit give me a chance!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Player: \"Fine, I’ll pitch in just stop glaring at us!\""));
    }

    IEnumerator HandleLayer8(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("Player: \"I’m with you let’s find the real crooks!\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Player: \"Work together? Sure, but ease up on us!\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("Player: \"I’ll cooperate just don’t ruin my trip!\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Player: \"I’d rather help than deal with your wrath!\""));
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

            if (adilaMiziki != null)
            {
                adilaMiziki.CheckMizikiLevelAtEnd();
            }
            else
            {
                Debug.LogError("AdilaMiziki reference missing!");
            }

            onDialogueEnd?.Invoke();
            isDialogueCompleted = true;
        }
    }
}
