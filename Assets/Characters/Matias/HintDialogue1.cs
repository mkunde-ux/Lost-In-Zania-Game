using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HintDialogue1 : MonoBehaviour
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
                    StartCoroutine(TypeSentence("David: ¡Mira! What the hell, man?! Our phones got stolen by those hijos de puta with estúpidos names!"));
                    break;
                case 2:
                    StartCoroutine(TypeSentence("Matias: Hahaha, ¡pinches bastardos también! They really think they can get away with this? Mo and Willow?!"));
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
        dialogueLayer++;
        SetDialogueLayer();
    }

    // Sets the dialogue to player turn and displays choice buttons.
    void SetPlayerTurn()
    {
        isPlayerTurn = true;
        SetButtonLabels("Tell Security", "Use as Proof", "Check for Líder");
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
                playerResponse = "David: ¡Dios! Should we tell security about this?";
                matiasReply = "Matias: Mierda, I don’t like that pinche security guy much… but we can give it a shot.";
                break;
            case 2:
                playerResponse = "David: I think we use this as proof to get the owner to let us into some rooms—especially Room 3!";
                matiasReply = "Matias: Proof? That might actually work… But first, we need them to trust us.";
                break;
            case 3:
                playerResponse = "David: Let’s just hope that líder guy hasn’t shown up yet…";
                matiasReply = "Matias: Probably not. But if he has, he’ll be acting raro as fuck. Let’s keep an eye on that cabrón.";
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