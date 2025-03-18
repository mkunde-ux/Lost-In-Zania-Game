using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class EmanuelHTDialogue : MonoBehaviour
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

    public EmanuelTrust emanuelTrust;

    public UnityEvent onDialogueEnd;

    private bool dialogueActive = false;
    private Transform player;
    [SerializeField] private float rotationSpeed = 5f;

    private EmanuelMovement emanuelMovement;

    void Start()
    {
        path1Button.onClick.AddListener(() => ChoosePath(1));
        path2Button.onClick.AddListener(() => ChoosePath(2));
        path3Button.onClick.AddListener(() => ChoosePath(3));
        dialogueCanvas.gameObject.SetActive(false);

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        emanuelMovement = GetComponent<EmanuelMovement>();
    }

    void Update()
    {
        if (isTyping && Input.GetKeyDown(KeyCode.Space))
        {
            isTyping = false;
            dialogueText.text = fullSentence;
        }

        if (dialogueActive && player != null)
        {
            StartCoroutine(SmoothLookAt(player.position));
        }
    }

    private IEnumerator SmoothLookAt(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        yield return null;
    }

    public void ActivateDialogue()
    {
        if (!dialogueActive)
        {
            gameObject.SetActive(true);
            dialogueCanvas.gameObject.SetActive(true);
            dialogueActive = true;
            StartDialogue();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (emanuelMovement != null)
            {
                emanuelMovement.InteractWithPlayer(true);
            }
        }
    }

    public void DeactivateDialogue()
    {
        if (dialogueActive)
        {
            dialogueActive = false;
            EndDialogue();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (emanuelMovement != null)
            {
                emanuelMovement.InteractWithPlayer(false);
                emanuelMovement.EndInteraction();
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
                    StartCoroutine(TypeSentence("Emanuel: Okay, I trust you are telling the truth, so here, I can give you access to check some of the rooms, but not mess around!"));
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
                SetButtonLabels("Thank you", "", "");
                break;
        }

        path1Button.gameObject.SetActive(true);
        path2Button.gameObject.SetActive(false);
        path3Button.gameObject.SetActive(false);
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
        yield return new WaitForSeconds(2f);
        dialogueLayer++;
        isPlayerTurn = false;
        SetDialogueLayer();
    }

    void HandleLayer1(int path)
    {
        if (path == 1)
            StartCoroutine(TypeSentence("Player: Thank you."));
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