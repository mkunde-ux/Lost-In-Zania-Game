using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class FahariHTDialogue : MonoBehaviour
{
    public TMP_Text dialogueText;
    public Button path1Button;
    public Button path2Button;
    public Button path3Button;
    [SerializeField] private Canvas dialogueCanvas;

    private bool isTyping = false;
    private string fullSentence = "";

    public int dialogueLayer = 1;
    private bool isPlayerTurn = false;

    public FahariTrust wealthyGirl;

    public UnityEvent onDialogueEnd;

    private bool dialogueActive = false; // Track if the dialogue is currently active

    private Transform player; // Add a reference to the player's transform
    [SerializeField] private float rotationSpeed = 5f; // Add rotation speed

    private FahariMovement fahariMovement; // Reference to FahariMovement

    void Start()
    {
        path1Button.onClick.AddListener(() => ChoosePath(1));
        path2Button.onClick.AddListener(() => ChoosePath(2));
        path3Button.onClick.AddListener(() => ChoosePath(3));
        dialogueCanvas.gameObject.SetActive(false); // Start with canvas disabled

        player = GameObject.FindGameObjectWithTag("Player")?.transform; // Find the player in Start
        fahariMovement = GetComponent<FahariMovement>(); // Get the FahariMovement component
    }

    void Update()
    {
        if (isTyping && Input.GetKeyDown(KeyCode.Space))
        {
            isTyping = false;
            dialogueText.text = fullSentence;
        }

        if (dialogueActive && player != null) // Only rotate if dialogue is active and player exists
        {
            StartCoroutine(SmoothLookAt(player.position)); // Call SmoothLookAt every frame
        }
    }

    private IEnumerator SmoothLookAt(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Keep the NPC upright
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        yield return null; // Important to return null to make this work smoothly over frames.
    }

    public void ActivateDialogue()
    {
        if (!dialogueActive)
        {
            gameObject.SetActive(true); // Activate GameObject first
            dialogueCanvas.gameObject.SetActive(true); // Show canvas
            dialogueActive = true;
            StartDialogue();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (fahariMovement != null)
            {
                fahariMovement.InteractWithPlayer(true); // Pass true to indicate dialogue is active
            }
        }
    }

    public void DeactivateDialogue() // New function to deactivate the dialogue
    {
        if (dialogueActive)
        {
            dialogueActive = false;
            EndDialogue();
            // Disable mouse cursor when dialogue is inactive
            Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
            Cursor.visible = false; // Hide the cursor

            if (fahariMovement != null)
            {
                fahariMovement.InteractWithPlayer(false); // Pass false to indicate dialogue is inactive
                fahariMovement.EndInteraction(); // Call EndInteraction to resume patrolling
            }
        }
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
            yield return new WaitForSeconds(0.05f);
        }

        isTyping = false;
    }

    public void StartDialogue()
    {
        dialogueLayer = 1;
        isPlayerTurn = false;
        SetDialogueLayer();
    }

    void SetDialogueLayer()
    {
        if (!isPlayerTurn)
        {
            switch (dialogueLayer)
            {
                case 1:
                    StartCoroutine(TypeSentence("Fahari: Okay, I trust you are telling the truth, so here, I can give you access to check some of the rooms, but not mess around!"));
                    break;
                default:
                    EndDialogue();
                    return;
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
                SetButtonLabels("Asante", "", ""); // "Asante" means "Thank you" in Swahili
                break;
        }

        path1Button.gameObject.SetActive(true);
        path2Button.gameObject.SetActive(false); // Deactivate other buttons
        path3Button.gameObject.SetActive(false); // Deactivate other buttons
    }

    void SetButtonLabels(string path1Text, string path2Text, string path3Text)
    {
        path1Button.GetComponentInChildren<TMP_Text>().text = path1Text;
        path2Button.GetComponentInChildren<TMP_Text>().text = path2Text;
        path3Button.GetComponentInChildren<TMP_Text>().text = path3Text;
    }

    void ChoosePath(int path)
    {
        path1Button.gameObject.SetActive(false);
        path2Button.gameObject.SetActive(false);
        path3Button.gameObject.SetActive(false);

        switch (dialogueLayer)
        {
            case 1:
                HandleLayer1(path);
                break;
        }

        StartCoroutine(WaitForPlayerResponse());
    }

    IEnumerator WaitForPlayerResponse()
    {
        yield return new WaitUntil(() => !isTyping);
        yield return new WaitForSeconds(2f); // Small delay
        dialogueLayer++;
        isPlayerTurn = false;
        SetDialogueLayer();
    }

    void HandleLayer1(int path)
    {
        if (path == 1)
            StartCoroutine(TypeSentence("Player: Asante."));
    }

    public void EndDialogue()
    {
        path1Button.gameObject.SetActive(false);
        path2Button.gameObject.SetActive(false);
        path3Button.gameObject.SetActive(false);
        dialogueCanvas.gameObject.SetActive(false);

        if (onDialogueEnd != null)
        {
            onDialogueEnd.Invoke();
        }
    }
}