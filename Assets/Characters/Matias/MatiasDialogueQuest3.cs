using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatiasDialogueQuest3 : MonoBehaviour
{
    // UI elements for dialogue display.
    public TMP_Text dialogueText;
    public Button path1Button;
    public Button path2Button;
    public Button path3Button;
    [SerializeField] private Canvas dialogueCanvas;
    // Delay before dialogue appearances.
    public float dialogueDelay = 0.5f;

    // Flags to manage dialogue flow.
    private bool isTyping = false;
    private int dialogueLayer = 1;
    private bool isPlayerTurn = false;

    // References to NPC trust levels.
    public ImaniTrust imaniTrust;
    public EmanuelTrust emanuelTrust;
    public FahariTrust fahariTrust;

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

    // Triggers the low trust dialogue sequence.
    public void TriggerLowTrustDialogue()
    {
        StartDialogue();
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
                    StartCoroutine(TypeSentence("David (Player): Looks like we didn't gain enough trust with Imani, Emanuel, and Fahari."));
                    break;
                case 2:
                    StartCoroutine(TypeSentence("Matias: Damn it! We really messed that up, didn't we?"));
                    break;
                case 3:
                    StartCoroutine(TypeSentence("David (Player): Now we need another way to get into that study room."));
                    break;
                case 4:
                    StartCoroutine(TypeSentence("Matias: Yeah, and it's probably where they stashed our phones."));
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
        SetButtonLabels("What now? We can't just barge in.", "Maybe there's a hidden passage or something.", "We need to find someone else who can help us.");
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
                playerResponse = "David (Player): What now? We can't just barge in.";
                matiasReply = "Matias: You're right. We need a plan. Maybe we can find a way to distract them.";
                break;
            case 2:
                playerResponse = "David (Player): Maybe there's a hidden passage or something.";
                matiasReply = "Matias: That's a good idea! This place is old, there might be some secret entrances.";
                break;
            case 3:
                playerResponse = "David (Player): We need to find someone else who can help us.";
                matiasReply = "Matias: Who else would help us? Everyone here seems to be on their side.";
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

        if (playerResponse == "David (Player): Maybe there's a hidden passage or something.")
        {
            yield return StartCoroutine(TypeSentence("David (Player): Remember that old rumor about the secret tunnels under the restaurant?"));
        }

        yield return new WaitForSeconds(1f);
        EndDialogue();
    }

    // Ends the dialogue and resets dialogue layer.
    public void EndDialogue()
    {
        ShowDialogueCanvas(false);
        dialogueLayer = 1;
        CheckNPCTrustLevels();
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

    // Checks and logs the trust levels of NPCs.
    private void CheckNPCTrustLevels()
    {
        if (imaniTrust != null)
        {
            Debug.Log("Imani's Trust Level: " + imaniTrust.currentTrust);
        }
        if (emanuelTrust != null)
        {
            Debug.Log("Emanuel's Trust Level: " + emanuelTrust.currentTrust);
        }
        if (fahariTrust != null)
        {
            Debug.Log("Fahari's Trust Level: " + fahariTrust.currentTrust);
        }
    }
}