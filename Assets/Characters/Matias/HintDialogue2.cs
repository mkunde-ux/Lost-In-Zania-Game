using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HintDialogue2 : MonoBehaviour
{
    // UI elements for dialogue display.
    public TMP_Text dialogueText;
    public Button path1Button;
    public Button path2Button;
    public Button path3Button;
    [SerializeField] private Canvas dialogueCanvas;
    // Delay between dialogue appearances.
    public float dialogueDelay = 0.5f;

    // Flags to manage dialogue flow.
    private bool isTyping = false;
    private int dialogueLayer = 1;
    private bool isPlayerTurn = false;

    // Called when the script instance is being loaded.
    void Start()
    {
        InitializeDialogue();
    }

    // Initializes dialogue UI and button listeners.
    private void InitializeDialogue()
    {
        dialogueCanvas.gameObject.SetActive(false);
        path1Button.gameObject.SetActive(false);
        path2Button.gameObject.SetActive(false);
        path3Button.gameObject.SetActive(false);

        path1Button.onClick.AddListener(() => ChoosePath(1));
        path2Button.onClick.AddListener(() => ChoosePath(2));
        path3Button.onClick.AddListener(() => ChoosePath(3));
    }

    // Starts the dialogue sequence.
    public void StartDialogue()
    {
        dialogueLayer = 1;
        ShowDialogueCanvas(true);
        StartCoroutine(StartDialogueAfterInitialDelay());
    }

    // Coroutine to delay the start of the dialogue.
    private IEnumerator StartDialogueAfterInitialDelay()
    {
        yield return new WaitForSeconds(dialogueDelay);
        StartDialogueFlow();
    }

    // Starts the dialogue flow.
    void StartDialogueFlow()
    {
        isPlayerTurn = false;
        SetDialogueLayer();
    }

    // Shows or hides the dialogue canvas and cursor.
    void ShowDialogueCanvas(bool active)
    {
        dialogueCanvas.gameObject.SetActive(active);
        Cursor.visible = active;
        Cursor.lockState = active ? CursorLockMode.None : CursorLockMode.Locked;
    }

    // Sets the dialogue layer and displays corresponding text.
    void SetDialogueLayer()
    {
        if (!isPlayerTurn)
        {
            switch (dialogueLayer)
            {
                case 1:
                    StartCoroutine(TypeSentence("David: Looks like the bastards were having the time of their lives! and they didn't leave any of our phones here though"));
                    break;
                case 2:
                    StartCoroutine(TypeSentence("Matias: Shit!"));
                    break;
                default:
                    SetPlayerTurn();
                    return;
            }
            StartCoroutine(WaitForNPCDialogue());
        }
    }

    // Coroutine to wait for NPC dialogue to finish.
    IEnumerator WaitForNPCDialogue()
    {
        yield return new WaitUntil(() => !isTyping);
        yield return new WaitForSeconds(5f);
        dialogueLayer++;
        SetDialogueLayer();
    }

    // Sets the dialogue to player turn and displays choice buttons.
    void SetPlayerTurn()
    {
        isPlayerTurn = true;
        SetButtonLabels("it looks like they put the phones into a study room? ¡Maldición!", "We need to get into the study room, seems very private!", "Shit! I don't like study rooms much!! they might have left our phones in that room though");
        path1Button.gameObject.SetActive(true);
        path2Button.gameObject.SetActive(true);
        path3Button.gameObject.SetActive(true);
    }

    // Sets the labels for the choice buttons.
    void SetButtonLabels(string path1Text, string path2Text, string path3Text)
    {
        path1Button.GetComponentInChildren<TMP_Text>().text = path1Text;
        path2Button.GetComponentInChildren<TMP_Text>().text = path2Text;
        path3Button.GetComponentInChildren<TMP_Text>().text = path3Text;
    }

    // Handles player choice and displays corresponding responses.
    public void ChoosePath(int path)
    {
        path1Button.gameObject.SetActive(false);
        path2Button.gameObject.SetActive(false);
        path3Button.gameObject.SetActive(false);

        string playerResponse = "";
        string matiasReply = "";

        switch (path)
        {
            case 1:
                playerResponse = "David: it looks like they put the phones into a study room? ¡Maldición!";
                matiasReply = "Matias: Study and library are far from us! hahaha.. Dude remember that one time we were trapped in the library room with.. You know..";
                break;
            case 2:
                playerResponse = "David: We need to get into the study room, seems very private!";
                matiasReply = "Matias: We have history with stupid study rooms and libraries... why is there a study room in a restaurant anyway? Isn't that weird?";
                break;
            case 3:
                playerResponse = "David: Shit! I don't like study rooms much!! they might have left our phones in that room though";
                matiasReply = "Matias: mmmhhh, hahaha, these thieves might be playing with you; this might be a prank or something.";
                break;
        }

        StartCoroutine(ProcessPlayerChoice(playerResponse, matiasReply));
    }

    // Coroutine to process player choice and display responses.
    private IEnumerator ProcessPlayerChoice(string playerResponse, string matiasReply)
    {
        yield return StartCoroutine(TypeSentence(playerResponse));
        yield return new WaitForSeconds(5f);
        yield return StartCoroutine(TypeSentence(matiasReply));
        yield return new WaitForSeconds(3f);

        if (playerResponse == "David: it looks like they put the phones into a study room? ¡Maldición!")
        {
            yield return StartCoroutine(TypeSentence("David: hahaha… not the time bro, plus, She was into.. you know!"));
        }

        yield return new WaitForSeconds(1f);
        EndDialogue();
    }

    // Ends the dialogue and resets dialogue layer.
    public void EndDialogue()
    {
        ShowDialogueCanvas(false);
        dialogueLayer = 1;
    }

    // Coroutine to type out a sentence character by character.
    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }

        isTyping = false;
    }
}