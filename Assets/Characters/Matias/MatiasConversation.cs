using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatiasConversation : MonoBehaviour
{
    // UI elements for dialogue display.
    public TMP_Text dialogueText;
    public Button path1Button;
    public Button path2Button;
    public Button path3Button;
    [SerializeField] private Canvas dialogueCanvas;
    // Delay before dialogue box appears.
    public float dialogueDelay = 2f;

    // Flags to manage dialogue flow.
    private bool isTyping = false;
    private string fullSentence = "";
    private int dialogueLayer = 1;
    private bool isPlayerTurn = false;

    // Called when the script instance is being loaded.
    void Start()
    {
        // Add listeners to choice buttons.
        path1Button.onClick.AddListener(() => ChoosePath(1));
        path2Button.onClick.AddListener(() => ChoosePath(2));
        path3Button.onClick.AddListener(() => ChoosePath(3));

        // Start the dialogue sequence after a delay.
        StartCoroutine(ShowDialogueAfterDelay());
    }

    // Coroutine to delay the start of the dialogue.
    private IEnumerator ShowDialogueAfterDelay()
    {
        yield return new WaitForSeconds(dialogueDelay);
        ShowDialogueCanvas(true);
        StartDialogue();
    }

    // Shows or hides the dialogue canvas and cursor.
    void ShowDialogueCanvas(bool active)
    {
        dialogueCanvas.gameObject.SetActive(active);
        Cursor.visible = active;
        Cursor.lockState = active ? CursorLockMode.None : CursorLockMode.Locked;
    }

    // Resets dialogue to the beginning.
    public void ResetDialogue()
    {
        dialogueLayer = 1;
        StartCoroutine(ShowDialogueAfterDelay());
    }

    // Called once per frame.
    void Update()
    {
        // Skip typing animation if space is pressed.
        if (isTyping && Input.GetKeyDown(KeyCode.Space))
        {
            isTyping = false;
            dialogueText.text = fullSentence;
        }
    }

    // Coroutine to type out a sentence character by character.
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
            yield return new WaitForSeconds(0.05f);
        }

        isTyping = false;
    }

    // Starts the dialogue flow.
    void StartDialogue()
    {
        isPlayerTurn = false;
        SetDialogueLayer();
    }

    // Sets the dialogue layer and displays corresponding text.
    void SetDialogueLayer()
    {
        if (!isPlayerTurn)
        {
            switch (dialogueLayer)
            {
                case 1:
                    StartCoroutine(TypeSentence("Matias: Yo, David, I think we should look around for clues! If that doesn't work, maybe we should ask around."));
                    break;
                default:
                    EndDialogue();
                    return;
            }
            StartCoroutine(WaitForNPCDialogue());
        }
    }

    // Coroutine to wait for NPC dialogue to finish.
    IEnumerator WaitForNPCDialogue()
    {
        yield return new WaitUntil(() => !isTyping);
        SetPlayerTurn();
    }

    // Sets the dialogue to player turn and displays choice buttons.
    void SetPlayerTurn()
    {
        isPlayerTurn = true;
        SetButtonLabels("Yeah, we should look around.", "Want to talk first?", "Not sure yet, maybe both?");
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
    void ChoosePath(int path)
    {
        path1Button.gameObject.SetActive(false);
        path2Button.gameObject.SetActive(false);
        path3Button.gameObject.SetActive(false);

        switch (path)
        {
            case 1:
                StartCoroutine(TypeSentence("Matias: Cool, lead the way bro!"));
                break;
            case 2:
                StartCoroutine(TypeSentence("Matias: Sure, just don't be weird."));
                break;
            case 3:
                StartCoroutine(TypeSentence("Matias: Mhhhhhhhhh, sure, why not."));
                break;
        }
        StartCoroutine(WaitForPlayerResponse());
    }

    // Coroutine to wait for player response to finish.
    IEnumerator WaitForPlayerResponse()
    {
        yield return new WaitUntil(() => !isTyping);
        yield return new WaitForSeconds(2f);
        EndDialogue();
    }

    // Ends the dialogue and hides UI elements.
    public void EndDialogue()
    {
        path1Button.gameObject.SetActive(false);
        path2Button.gameObject.SetActive(false);
        path3Button.gameObject.SetActive(false);
        ShowDialogueCanvas(false);
    }
}