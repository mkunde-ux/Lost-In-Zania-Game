using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

[System.Serializable]
public class DialogueLayerData
{
    // Text for the NPC to say.
    public string npcText;
    // Array of player responses.
    public string[] playerResponses;
}

public class NesDialogue : MonoBehaviour
{
    [Header("UI References")]
    // Text component to display dialogue.
    public TMP_Text dialogueText;
    // Buttons for player response choices.
    public Button path1Button, path2Button, path3Button, path4Button, path5Button, path6Button;
    // Slider to display the timer.
    public Slider timerSlider;
    // Canvas to hold the dialogue UI.
    [SerializeField] private Canvas dialogueCanvas;

    [Header("Dialogue Settings")]
    // Maximum number of dialogue layers.
    [SerializeField] private int maxDialogueLayer = 6;
    // Duration of the timer.
    [SerializeField] private float timerDuration = 20f;
    // Speed at which the text is typed.
    [SerializeField] private float typingSpeed = 0.05f;

    [Header("Events")]
    // Event to be triggered when the dialogue ends.
    public UnityEvent onDialogueEnd;

    [Header("Dependencies")]
    // Reference to the NesInteractionController.
    public NesInteractionController nesInteractionController;

    // Flag to track if the text is currently being typed.
    private bool isTyping = false;
    // Full sentence to be typed.
    private string fullSentence = "";
    // Flag to track if the player response is complete.
    private bool isPlayerResponseComplete = false;
    // Current dialogue layer.
    private int dialogueLayer = 1;
    // Flag to track if it's the player's turn to respond.
    private bool isPlayerTurn = false;
    // Flag to track if the dialogue is completed.
    private bool isDialogueCompleted = false;
    // Flag to track if the player is in range.
    private bool isPlayerInRange = false;
    // Current timer value.
    private float timer;
    // Flag to track if the timer is running.
    private bool isTimerRunning = false;

    // Array of dialogue layers.
    public DialogueLayerData[] dialogueLayers;

    private void Start()
    {
        // Initialize buttons.
        InitializeButtons();
        // Initialize timer.
        InitializeTimer();
        // Initialize UI.
        InitializeUI();
    }

    private void InitializeButtons()
    {
        // Add listeners to the buttons.
        path1Button.onClick.AddListener(() => ChoosePath(1));
        path2Button.onClick.AddListener(() => ChoosePath(2));
        path3Button.onClick.AddListener(() => ChoosePath(3));
        path4Button.onClick.AddListener(() => ChoosePath(4));
        path5Button.onClick.AddListener(() => ChoosePath(5));
        path6Button.onClick.AddListener(() => ChoosePath(6));
    }

    private void InitializeTimer()
    {
        // Set the max value of the timer slider.
        timerSlider.maxValue = timerDuration;
        // Set the initial value of the timer slider.
        timerSlider.value = timerDuration;
    }

    private void InitializeUI()
    {
        // Hide the dialogue canvas.
        dialogueCanvas.gameObject.SetActive(false);
    }

    public void ResetDialogue()
    {
        // Reset dialogue layer.
        dialogueLayer = 1;
        // Reset dialogue completion flag.
        isDialogueCompleted = false;
        // Reset player turn flag.
        isPlayerTurn = false;
        // Start the dialogue.
        StartDialogue();
    }

    private void Update()
    {
        // Handle player input.
        HandlePlayerInput();
        // Handle timer.
        HandleTimer();
    }

    private void HandlePlayerInput()
    {
        // Check for player interaction input.
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Check if player is in range, dialogue is not completed, and player can interact with Nes.
            if (isPlayerInRange && !isDialogueCompleted && CanInteractWithNes())
            {
                // Start the dialogue.
                StartDialogue();
            }
        }

        // Check for skip typing input.
        if (isTyping && Input.GetKeyDown(KeyCode.Space))
        {
            // Skip typing and display the full sentence.
            isTyping = false;
            dialogueText.text = fullSentence;
        }

        // Check for proceed to next layer input.
        if (isPlayerResponseComplete && Input.GetKeyDown(KeyCode.Space))
        {
            // Reset player response completion flag.
            isPlayerResponseComplete = false;
            // Increment dialogue layer.
            dialogueLayer++;
            // Reset player turn flag.
            isPlayerTurn = false;
            // Set the next dialogue layer.
            SetDialogueLayer();
        }
    }

    private void HandleTimer()
    {
        // Check if the timer is running.
        if (isTimerRunning)
        {
            // Decrement the timer.
            timer -= Time.deltaTime;
            // Update the timer slider value.
            timerSlider.value = timer;
            // Check if the timer has expired.
            if (timer <= 0f)
            {
                // Stop the timer.
                isTimerRunning = false;
                // Handle timer expiration.
                OnTimerExpired();
            }
        }
    }

    private void OnTimerExpired()
    {
        // Display timer expiration message.
        StartCoroutine(TypeSentence("Nes: \"Time waits for no one, child. Make haste!\""));
        // Proceed to the next dialogue layer.
        ProceedToNextLayer();
    }

    private void StartTimer()
    {
        // Set the timer value.
        timer = timerDuration;
        // Update the timer slider value.
        timerSlider.value = timer;
        // Start the timer.
        isTimerRunning = true;
    }


    private void StopTimer()
    {
        // Stop the timer.
        isTimerRunning = false;
    }

    public void StartDialogue()
    {
        // Check if the player is in range and can interact with Nes.
        if (!isPlayerInRange || !CanInteractWithNes())
        {
            // If not, exit the method.
            return;
        }

        // Show the dialogue canvas.
        dialogueCanvas.gameObject.SetActive(true);
        // Make the cursor visible.
        Cursor.visible = true;
        // Unlock the cursor.
        Cursor.lockState = CursorLockMode.None;
        // Set player turn to false.
        isPlayerTurn = false;
        // Set the dialogue layer.
        SetDialogueLayer();
        // Reset dialogue completion flag.
        isDialogueCompleted = false;
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
            // Check if typing is interrupted.
            if (!isTyping) { dialogueText.text = sentence; break; }
            // Append the letter to the dialogue text.
            dialogueText.text += letter;
            // Wait for the typing speed duration.
            yield return new WaitForSeconds(typingSpeed);
        }
        // Set typing flag to false.
        isTyping = false;
        // If it's the player's turn, set player response complete flag to true.
        if (isPlayerTurn) isPlayerResponseComplete = true;
    }

    public bool IsDialogueComplete()
    {
        // Return the dialogue completion flag.
        return isDialogueCompleted;
    }

    public void SetPlayerInRange(bool inRange)
    {
        // Set the player in range flag.
        isPlayerInRange = inRange;
        // If the player is out of range and the dialogue canvas is active, end the dialogue.
        if (!inRange && dialogueCanvas.gameObject.activeSelf)
        {
            EndDialogue();
        }
    }

    private void SetDialogueLayer()
    {
        if (!isPlayerTurn)
        {
            if (dialogueLayer > dialogueLayers.Length)
            {
                EndDialogue();
                return;
            }

            StartCoroutine(TypeSentence(dialogueLayers[dialogueLayer - 1].npcText));
            StartCoroutine(WaitForNPCDialogue());

            switch (dialogueLayer)
            {
                case 1:
                    StartCoroutine(TypeSentence("Nes: \"Ah, another wanderer seeking wisdom. What troubles your heart?\""));
                    break;
                case 2:
                    StartCoroutine(TypeSentence("Nes: \"Theft and deceit? These shadows plague us all. But why seek my aid?\""));
                    break;
                case 3:
                    StartCoroutine(TypeSentence("Nes: \"You slipped past the guardians? Impressive. What skills do you possess?\""));
                    break;
                case 4:
                    StartCoroutine(TypeSentence("Nes: \"A prize? What value do these stolen items hold for you?\""));
                    break;
                case 5:
                    StartCoroutine(TypeSentence("Nes: \"You stir the waters of fate. Are you ready to face the currents?\""));
                    break;
                case 6:
                    StartCoroutine(TypeSentence("Nes: \"The end draws near. Will you trust the path before you?\""));
                    break;
            }
            StartCoroutine(WaitForNPCDialogue());
        }
    }

    private IEnumerator WaitForNPCDialogue()
    {
        // Wait until the NPC dialogue typing is finished.
        yield return new WaitUntil(() => !isTyping);
        // Set the player's turn to respond.
        SetPlayerTurn();
    }

    private void SetPlayerTurn()
    {
        // Set the player turn flag to true.
        isPlayerTurn = true;
        // Set the button labels based on the player responses for the current dialogue layer.
        SetButtonLabels(dialogueLayers[dialogueLayer - 1].playerResponses);
        // Show the response buttons.
        ToggleButtons(true);
        // Start the timer.
        StartTimer();
    }

    private void SetButtonLabels(string[] labels)
    {
        // Set the text of each button based on the corresponding label, if available.
        if (labels.Length > 0) path1Button.GetComponentInChildren<TMP_Text>().text = labels[0];
        if (labels.Length > 1) path2Button.GetComponentInChildren<TMP_Text>().text = labels[1];
        if (labels.Length > 2) path3Button.GetComponentInChildren<TMP_Text>().text = labels[2];
        if (labels.Length > 3) path4Button.GetComponentInChildren<TMP_Text>().text = labels[3];
        if (labels.Length > 4) path5Button.GetComponentInChildren<TMP_Text>().text = labels[4];
        if (labels.Length > 5) path6Button.GetComponentInChildren<TMP_Text>().text = labels[5];
    }

    private void ToggleButtons(bool isVisible)
    {
        // Show or hide all response buttons based on the isVisible flag.
        path1Button.gameObject.SetActive(isVisible);
        path2Button.gameObject.SetActive(isVisible);
        path3Button.gameObject.SetActive(isVisible);
        path4Button.gameObject.SetActive(isVisible);
        path5Button.gameObject.SetActive(isVisible);
        path6Button.gameObject.SetActive(isVisible);
    }

    private void ChoosePath(int path)
    {
        // Stop the timer.
        StopTimer();
        // Hide the response buttons.
        ToggleButtons(false);
        // Start the coroutine to handle the dialogue layer with the player's response.
        StartCoroutine(HandleLayerWithResponse(path));
    }

    private IEnumerator HandleLayerWithResponse(int path)
    {
        // Handle the dialogue layer based on the player's chosen path.
        switch (dialogueLayer)
        {
            case 1: yield return StartCoroutine(HandleLayer1(path)); break;
            case 2: yield return StartCoroutine(HandleLayer2(path)); break;
            case 3: yield return StartCoroutine(HandleLayer3(path)); break;
            case 4: yield return StartCoroutine(HandleLayer4(path)); break;
            case 5: yield return StartCoroutine(HandleLayer5(path)); break;
            case 6: yield return StartCoroutine(HandleLayer6(path)); break;
        }
        // Wait for a short delay.
        yield return new WaitForSeconds(3);
        // Start the coroutine for Nes's response based on the player's chosen path.
        yield return StartCoroutine(NesResponse(path));
        // Proceed to the next dialogue layer.
        ProceedToNextLayer();
    }

    private void ProceedToNextLayer()
    {
        // Increment the dialogue layer.
        dialogueLayer++;
        // If the dialogue layer exceeds the maximum, end the dialogue.
        if (dialogueLayer > maxDialogueLayer) EndDialogue();
        // Otherwise, set the next dialogue layer.
        else SetDialogueLayer();
    }

    private IEnumerator NesResponse(int path)
    {
        string[] responses = {
            "Nes: \"Indeed, child.\"",
            "Nes: \"As you wish.\"",
            "Nes: \"I see.\"",
            "Nes: \"Very well.\"",
            "Nes: \"So it is.\"",
            "Nes: \"Your path is clear.\""
        };

        yield return StartCoroutine(TypeSentence(responses[Random.Range(0, responses.Length)]));
    }

    private IEnumerator HandleLayer1(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"We need your help, wise one.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"What's it to you, ancient one?\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"We're lost, and need guidance.\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"We're on a quest, and seek your aid.\""));
        else if (path == 5) yield return StartCoroutine(TypeSentence("David: \"We're in danger, and need your protection.\""));
        else if (path == 6) yield return StartCoroutine(TypeSentence("Matias: \"We're adventurers, and seek your wisdom.\""));
    }

    private IEnumerator HandleLayer2(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"We need justice, and seek your counsel.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"It's none of your business, old one.\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"We're desperate, and need your help.\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"We're resourceful, and seek your insight.\""));
        else if (path == 5) yield return StartCoroutine(TypeSentence("David: \"We're betrayed, and need your guidance.\""));
        else if (path == 6) yield return StartCoroutine(TypeSentence("Matias: \"We're explorers, and seek your knowledge.\""));
    }

    private IEnumerator HandleLayer3(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"I'm skilled, and can aid you.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"We're resourceful, and can find our way.\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"I'm quick, and can move unseen.\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"We're adaptable, and can overcome challenges.\""));
        else if (path == 5) yield return StartCoroutine(TypeSentence("David: \"I'm brave, and can face any danger.\""));
        else if (path == 6) yield return StartCoroutine(TypeSentence("Matias: \"We're cunning, and can outsmart our foes.\""));
    }

    private IEnumerator HandleLayer4(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"They're valuable, and hold great worth.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"They're a means to an end, and hold great power.\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"They're important, and hold great significance.\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"They're a challenge, and hold great intrigue.\""));
        else if (path == 5) yield return StartCoroutine(TypeSentence("David: \"They're our lives, and hold great meaning.\""));
        else if (path == 6) yield return StartCoroutine(TypeSentence("Matias: \"They're our prize, and hold great reward.\""));
    }

    private IEnumerator HandleLayer5(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"We're ready, and will face any trial.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"We're always ready, and fear no challenge.\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"We're prepared, and will overcome any obstacle.\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"We're fearless, and will conquer any foe.\""));
        else if (path == 5) yield return StartCoroutine(TypeSentence("David: \"We're strong, and will endure any hardship.\""));
        else if (path == 6) yield return StartCoroutine(TypeSentence("Matias: \"We're unstoppable, and will achieve our goals.\""));
    }

    private IEnumerator HandleLayer6(int path)
    {
        if (path == 1) yield return StartCoroutine(TypeSentence("David: \"We trust you, and place our faith in you.\""));
        else if (path == 2) yield return StartCoroutine(TypeSentence("Matias: \"We trust ourselves, and rely on our own strength.\""));
        else if (path == 3) yield return StartCoroutine(TypeSentence("David: \"We trust the path, and follow where it leads.\""));
        else if (path == 4) yield return StartCoroutine(TypeSentence("Matias: \"We trust our instincts, and listen to our inner voice.\""));
        else if (path == 5) yield return StartCoroutine(TypeSentence("David: \"We trust fate, and accept what comes our way.\""));
        else if (path == 6) yield return StartCoroutine(TypeSentence("Matias: \"We trust our luck, and take our chances.\""));
    }

    public void EndDialogue()
    {
        // Check if the dialogue layer has exceeded the maximum dialogue layer.
        if (dialogueLayer > maxDialogueLayer)
        {
            // Hide the response buttons.
            ToggleButtons(false);
            // Hide the dialogue canvas.
            dialogueCanvas.gameObject.SetActive(false);
            // Make the cursor invisible.
            Cursor.visible = false;
            // Lock the cursor.
            Cursor.lockState = CursorLockMode.Locked;

            // Invoke the onDialogueEnd event, if it's not null.
            onDialogueEnd?.Invoke();
            // Set the dialogue completed flag to true.
            isDialogueCompleted = true;
        }
    }

    private bool CanInteractWithNes()
    {
        // Check if the NesInteractionController reference is missing.
        if (nesInteractionController == null)
        {
            // Log an error message if the reference is missing.
            Debug.LogError("NesInteractionController reference is missing!");
            // Return false to indicate that interaction is not possible.
            return false;
        }

        // Evaluate trust levels using the NesInteractionController and return the result.
        return nesInteractionController.EvaluateTrustLevels();
    }
}