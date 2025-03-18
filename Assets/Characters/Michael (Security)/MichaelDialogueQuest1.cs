using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

public class MichaelDialogueQuest1 : MonoBehaviour
{
    [Header("UI References")]
    // Reference to the TextMeshPro text component for displaying dialogue.
    public TMP_Text dialogueText;
    // Reference to the Button component for the first dialogue path.
    public Button path1Button;
    // Reference to the Button component for the second dialogue path.
    public Button path2Button;
    // Reference to the Canvas component for the dialogue UI.
    [SerializeField] private Canvas dialogueCanvas;

    [Header("Audio Settings (Optional)")]
    // Reference to the AudioSource component for playing audio clips.
    [SerializeField] private AudioSource audioSource;
    // Audio clip for typing sound effects.
    [SerializeField] private AudioClip typeSoundClip;
    // Audio clip for button press sound effects.
    [SerializeField] private AudioClip buttonPressClip;

    [Header("Dialogue Settings")]
    // Current layer of the dialogue.
    public int dialogueLayer = 0;
    // Flag to indicate if the dialogue text is currently being typed.
    private bool isTyping = false;
    // Full sentence to be displayed in the dialogue text.
    private string fullSentence = "";
    // Flag to indicate if the player's response is complete.
    private bool isPlayerResponseComplete = false;
    // Flag to indicate if it's the player's turn to respond.
    private bool isPlayerTurn = false;

    // UnityEvent invoked when the dialogue ends.
    public UnityEvent onDialogueEnd;
    // Selected option for the first dialogue path.
    private int selectedOption1 = 0;
    // Selected option for the second dialogue path.
    private int selectedOption2 = 0;

    // Reference to the PopupTrigger component.
    [SerializeField] private PopupTrigger popupTrigger;

    // Reference to the QuestDirection script.
    public QuestDirection questDirection;

    // Coroutine reference for typing animation.
    private Coroutine typingCoroutine;

    // Called when the script instance is being loaded.
    void Start()
    {
        // Add listeners to the path buttons.
        path1Button.onClick.AddListener(() => ChoosePath(1));
        path2Button.onClick.AddListener(() => ChoosePath(2));

        // Initially hide the path buttons.
        path1Button.gameObject.SetActive(false);
        path2Button.gameObject.SetActive(false);
        // Initially hide the dialogue canvas.
        if (dialogueCanvas != null)
        {
            dialogueCanvas.gameObject.SetActive(false);
        }
    }

    // Resets the dialogue to the beginning.
    public void ResetDialogue()
    {
        // Reset dialogue layer and start dialogue again.
        dialogueLayer = 0;
        StartDialogue();
    }

    // Called once per frame.
    void Update()
    {
        // Skip typing animation when space key is pressed.
        if (isTyping && Input.GetKeyDown(KeyCode.Space))
        {
            isTyping = false;
            dialogueText.text = fullSentence;
        }

        // Advance to the next dialogue layer when space key is pressed after player response.
        if (isPlayerResponseComplete && Input.GetKeyDown(KeyCode.Space))
        {
            isPlayerResponseComplete = false;
            AdvanceDialogueLayer();
        }
    }

    // Checks if the dialogue can be started.
    private bool CanStartDialogue()
    {
        // Dialogue can start if the popup trigger exists and its menu is activated and closed.
        bool canStart = popupTrigger != null && popupTrigger.IsMenuActivatedAndClosed();
        Debug.Log($"CanStartDialogue: {canStart} (popupTrigger exists: {popupTrigger != null})");
        return canStart;
    }

    // Coroutine to animate typing of the dialogue sentence.
    private IEnumerator TypeSentence(string sentence)
    {
        // Set typing flag and initialize dialogue text.
        isTyping = true;
        fullSentence = sentence;
        dialogueText.text = "";
        // Iterate through each character in the sentence.
        foreach (char letter in sentence.ToCharArray())
        {
            // Stop typing if the typing flag is false.
            if (!isTyping)
            {
                dialogueText.text = sentence;
                break;
            }
            // Append the current letter to the dialogue text.
            dialogueText.text += letter;
            // Play typing sound effect if available.
            if (letter != ' ' && audioSource != null && typeSoundClip != null)
            {
                audioSource.PlayOneShot(typeSoundClip);
            }
            // Wait for a short duration before typing the next letter.
            yield return new WaitForSeconds(0.05f);
        }
        // Reset typing flag and set player response flag if it's the player's turn.
        isTyping = false;
        if (isPlayerTurn) isPlayerResponseComplete = true;
    }

    // Starts the typing animation coroutine.
    private void StartTypeSentence(string sentence)
    {
        // Stop the current typing coroutine if it's running.
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        // Start a new typing coroutine.
        typingCoroutine = StartCoroutine(TypeSentence(sentence));
    }

    // Starts the dialogue.
    public void StartDialogue()
    {
        // Return if the dialogue cannot be started.
        if (!CanStartDialogue()) return;
        // Reset player turn and dialogue layer.
        isPlayerTurn = false;
        dialogueLayer = 0;
        // Show the dialogue canvas.
        if (dialogueCanvas != null)
        {
            dialogueCanvas.gameObject.SetActive(true);
        }
        // Show the cursor and unlock it.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        // Set the dialogue layer to display the appropriate dialogue.
        SetDialogueLayer();
    }


    void SetDialogueLayer()
    {
        // Hide dialogue path buttons and reset player turn.
        path1Button.gameObject.SetActive(false);
        path2Button.gameObject.SetActive(false);
        isPlayerTurn = false;

        // Switch statement to handle different dialogue layers.
        switch (dialogueLayer)
        {
            // Dialogue layer 0: Initial NPC dialogue.
            case 0:
                // Start typing the NPC's sentence.
                StartCoroutine(TypeSentence("David: Security! Look—here’s proof that our items were stolen by a group called 'Oops! It’s Ours Now?'"));
                // Wait for the NPC dialogue to finish and advance to the next layer.
                StartCoroutine(WaitForNPCDialogue(2));
                break;
            // Dialogue layer 1: NPC asks a question.
            case 1:
                // Start typing the NPC's question.
                StartCoroutine(TypeSentence("Michael: 'Oops?' Seriously? That's what they went with? Ha! But… this is interesting. Hmm… where did you find it?"));
                // Wait for the NPC dialogue to finish and enable player turn.
                StartCoroutine(WaitAndEnablePlayerTurn());
                break;
            // Dialogue layer 2: Player's first choice.
            case 2:
                // Set up player's turn for the first choice.
                SetPlayerTurnForChoice1();
                break;
            // Dialogue layer 3: NPC's response to player's first choice.
            case 3:
                // NPC response based on player's first choice.
                if (selectedOption1 == 1)
                {
                    StartCoroutine(TypeSentence("Michael: You have three minutes to look around, then."));
                }
                else if (selectedOption1 == 2)
                {
                    StartCoroutine(TypeSentence("Michael: You have three minutes to check room 3, then."));
                }
                // Wait and advance to the next layer after NPC's response.
                StartCoroutine(WaitAndAdvance(4));
                break;
            // Dialogue layer 4: Player's second choice.
            case 4:
                // Set up player's turn for the second choice.
                SetPlayerTurnForChoice2();
                break;
            // Dialogue layer 5: NPC's response to player's second choice.
            case 5:
                // NPC response based on player's second choice.
                if (selectedOption2 == 1)
                {
                    StartCoroutine(TypeSentence("Michael: You have six minutes."));
                }
                else if (selectedOption2 == 2)
                {
                    StartCoroutine(TypeSentence("Michael: I won’t repeat myself. Three minutes. That’s it."));
                }
                // Wait and advance to the next layer after NPC's response.
                StartCoroutine(WaitAndAdvance(6));
                break;
            // Dialogue layer 6: End of dialogue.
            case 6:
                // End the dialogue.
                EndDialogue();
                break;
        }
    }

    // Coroutine to wait for NPC dialogue to finish and advance to the next layer.
    IEnumerator WaitForNPCDialogue(int nextLayer)
    {
        // Wait until the NPC dialogue typing is finished.
        yield return new WaitUntil(() => !isTyping);
        // Wait for a short delay.
        yield return new WaitForSeconds(0.5f);
        // Set the dialogue layer to the next layer and update dialogue.
        dialogueLayer = nextLayer;
        SetDialogueLayer();
    }

    // Coroutine to wait for dialogue to finish and player input to advance.
    IEnumerator WaitAndAdvance(int nextLayer)
    {
        // Wait until the dialogue typing is finished.
        yield return new WaitUntil(() => !isTyping);
        // Wait until the space key is pressed.
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        // Advance to the next dialogue layer.
        AdvanceDialogueLayer(nextLayer);
    }

    // Coroutine to wait for dialogue to finish and enable player turn.
    IEnumerator WaitAndEnablePlayerTurn()
    {
        // Wait until the dialogue typing is finished.
        yield return new WaitUntil(() => !isTyping);
        // Enable player turn and set the dialogue layer.
        isPlayerTurn = true;
        dialogueLayer = 2;
        SetDialogueLayer();
    }

    // Advances the dialogue layer.
    void AdvanceDialogueLayer(int nextLayer = -1)
    {
        // Set the dialogue layer to the specified layer or increment it.
        dialogueLayer = (nextLayer >= 0) ? nextLayer : dialogueLayer + 1;
        SetDialogueLayer();
    }

    // Sets up player turn for the first choice.
    void SetPlayerTurnForChoice1()
    {
        // Enable player turn and set button labels.
        isPlayerTurn = true;
        SetButtonLabels("Stupide group name right...", "We need to check Room 3");
        // Show the dialogue path buttons.
        path1Button.gameObject.SetActive(true);
        path2Button.gameObject.SetActive(true);
    }

    // Sets up player turn for the second choice.
    void SetPlayerTurnForChoice2()
    {
        // Enable player turn and set button labels.
        isPlayerTurn = true;
        SetButtonLabels("3 minutes?!!", "What 3 minutes!");
        // Show the dialogue path buttons.
        path1Button.gameObject.SetActive(true);
        path2Button.gameObject.SetActive(true);
    }

    // Sets the labels for the dialogue path buttons.
    void SetButtonLabels(string text1, string text2)
    {
        // Set the text of the first button to text1.
        path1Button.GetComponentInChildren<TMP_Text>().text = text1;
        // Set the text of the second button to text2.
        path2Button.GetComponentInChildren<TMP_Text>().text = text2;
    }

    // Handles the player's choice in the dialogue.
    void ChoosePath(int path)
    {
        // Return if it's not the player's turn.
        if (!isPlayerTurn) return;

        // Play button press sound if available.
        if (audioSource != null && buttonPressClip != null)
        {
            audioSource.PlayOneShot(buttonPressClip);
        }

        // Hide the dialogue path buttons.
        path1Button.gameObject.SetActive(false);
        path2Button.gameObject.SetActive(false);

        // Initialize the player response string.
        string playerResponse = "";
        // Handle the player's choice based on the current dialogue layer.
        if (dialogueLayer == 2)
        {
            // Store the selected option for the first choice.
            selectedOption1 = path;
            // Set the player response based on the selected path.
            if (path == 1)
            {
                playerResponse = "Player: Stupide group name right...";
            }
            else
            {
                playerResponse = "Player: We need to check Room 3";
            }
        }
        else if (dialogueLayer == 4)
        {
            // Store the selected option for the second choice.
            selectedOption2 = path;
            // Set the player response based on the selected path.
            if (path == 1)
            {
                playerResponse = "Player: 3 minutes?!!";
            }
            else
            {
                playerResponse = "Player: What 3 minutes!";
            }
        }

        // Show the player's response in the dialogue.
        StartCoroutine(ShowPlayerResponse(playerResponse));
    }

    // Coroutine to show the player's response in the dialogue text.
    IEnumerator ShowPlayerResponse(string response)
    {
        // Wait until the current dialogue typing is finished.
        yield return new WaitUntil(() => !isTyping);
        // Type the player's response in the dialogue text.
        yield return StartCoroutine(TypeSentence(response));
        // Set the player response complete flag.
        isPlayerResponseComplete = true;
    }

    // Gets the selected option from the player.
    public int GetSelectedOption()
    {
        // Return the selected option for the second choice if available, otherwise return the first choice.
        return selectedOption2 != 0 ? selectedOption2 : selectedOption1;
    }

    // Ends the dialogue.
    public void EndDialogue()
    {
        // Hide the dialogue path buttons.
        path1Button.gameObject.SetActive(false);
        path2Button.gameObject.SetActive(false);
        // Hide the dialogue canvas.
        if (dialogueCanvas != null)
        {
            dialogueCanvas.gameObject.SetActive(false);
        }
        // Invoke the onDialogueEnd event if it's not null.
        if (onDialogueEnd != null)
        {
            onDialogueEnd.Invoke();
        }
        // Hide the cursor and lock it.
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        // Reset the dialogue layer.
        dialogueLayer = 0;

        // Advance the quest stage if the QuestDirection script is assigned.
        if (questDirection != null)
        {
            questDirection.AdvanceQuestStage();
        }
        else
        {
            // Log an error if the QuestDirection script is missing.
            Debug.LogError("QuestDirection reference missing!");
        }
    }
}