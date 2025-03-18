using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events;

public class EmanuelInteraction : MonoBehaviour
{
    //Check the Fahari Interaction Script for comments

    [Header("References")]
    [SerializeField] private EmanuelDT emanuelDialogue;
    [SerializeField] private Canvas interactionCanvas;
    [SerializeField] private Canvas dialogueCanvas;
    [SerializeField] private EmanuelHeadLook emanuelHeadLook;
    [SerializeField] private Transform _playerHead;

    [Header("Settings")]
    [SerializeField] private float interactionRadius = 5f;
    [SerializeField] private float interactionCooldown = 1f;
    [SerializeField] private KeyCode interactionKey = KeyCode.E; 

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip interactionStartClip; 

    private Transform _playerTransform;
    private bool _isEngaged;
    private bool _playerInRange;
    private bool _canInteract = true;

    public UnityEvent OnDialogueCompleted = new UnityEvent();

    [SerializeField] private EmanuelMovement emanuelMovement;

    private void Awake()
    {
        ValidateReferences();
        InitializeUI();
        emanuelMovement = GetComponent<EmanuelMovement>();

        if (emanuelMovement == null)
        {
            Debug.LogError("emanuelMovement component not found on NPC!");
        }
    }

    private void ValidateReferences()
    {
        if (!emanuelHeadLook) emanuelHeadLook = GetComponent<EmanuelHeadLook>();
        Debug.Assert(emanuelHeadLook != null, "HeadLook component is missing from NPC", this);
        Debug.Assert(emanuelDialogue != null, "emanuelDialogue reference is missing", this);
        Debug.Assert(interactionCanvas != null, "Interaction canvas reference is missing", this);
        Debug.Assert(dialogueCanvas != null, "Dialogue canvas reference is missing", this);
    }

    private void InitializeUI()
    {
        interactionCanvas.gameObject.SetActive(false);
        dialogueCanvas.gameObject.SetActive(false);
    }

    private void Update()
    {
        HandlePlayerDetection();
        UpdateHeadTracking();

        if (_playerInRange && Input.GetKeyDown(interactionKey))
        {
            if (_canInteract && !_isEngaged)
            {
                StartInteraction();
            }
        }
    }

    private void HandlePlayerDetection()
    {
        bool wasInRange = _playerInRange;
        _playerInRange = Physics.CheckSphere(transform.position, interactionRadius, LayerMask.GetMask("Player"));

        if (_playerInRange)
        {
            UpdatePlayerReference();
            HandleInRangeBehavior(!wasInRange);

            if (!wasInRange)
            {
                if (emanuelDialogue != null)
                {
                    emanuelDialogue.SetPlayerInRange(true);
                    Debug.Log("Player entered range. SetPlayerInRange(true) called.");
                }
            }
        }
        else if (wasInRange)
        {
            HandleOutOfRangeBehavior();

            if (emanuelDialogue != null)
            {
                emanuelDialogue.SetPlayerInRange(false);
                Debug.Log("Player exited range. SetPlayerInRange(false) called.");
            }
        }
    }

    private void OnPlayerEnterRange()
    {
        if (!_isEngaged)
        {
            interactionCanvas.gameObject.SetActive(true);
        }
    }

    private void UpdatePlayerReference()
    {
        if (_playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
        }
    }

    private void HandleInRangeBehavior(bool firstEnter)
    {
        if (firstEnter) OnPlayerEnterRange();
        interactionCanvas.gameObject.SetActive(!_isEngaged);
    }

    private void HandleOutOfRangeBehavior()
    {
        interactionCanvas.gameObject.SetActive(false);
        if (_isEngaged) ResetDialogueSystem();
    }

    private void UpdateHeadTracking()
    {
        if (_playerInRange && _playerTransform != null && !_isEngaged)
        {
            Vector3 lookPosition = GetPlayerHeadPosition();
            emanuelHeadLook.LookAtPosition(lookPosition);
        }
        else if (!_playerInRange)
        {
            emanuelHeadLook.StopLooking();
        }
    }

    private void StartInteraction()
    {
        _isEngaged = true;
        ToggleUI(true);
        LockCursor(false);

        if (audioSource != null && interactionStartClip != null)
        {
            audioSource.PlayOneShot(interactionStartClip);
        }

        emanuelDialogue.StartDialogue();
        emanuelHeadLook.LookAtPosition(GetPlayerHeadPosition());
        StartCoroutine(InteractionCooldown());

        if (emanuelMovement != null)
        {
            emanuelMovement.InteractWithPlayer(true);
        }
    }

    private Vector3 GetPlayerHeadPosition()
    {
        if (_playerHead != null)
        {
            return _playerHead.position;
        }
        else
        {
            Debug.LogWarning("Player head reference is missing. Using default head height.");
            return _playerTransform.position + Vector3.up * 1.7f;
        }
    }

    private void ToggleUI(bool dialogueActive)
    {
        interactionCanvas.gameObject.SetActive(!dialogueActive);
        dialogueCanvas.gameObject.SetActive(dialogueActive);
    }

    private void LockCursor(bool locked)
    {
        Cursor.visible = !locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    }

    public bool IsDialogueComplete()
    {
        if (emanuelDialogue == null)
        {
            Debug.LogError("EmanuelDialogue reference is missing!");
            return false;
        }
        return emanuelDialogue.IsDialogueComplete();
    }

    private void ResetDialogueSystem()
    {
        _isEngaged = false;
        ToggleUI(false);
        LockCursor(true);
        emanuelHeadLook.StopLooking();

        if (emanuelMovement != null)
        {
            emanuelMovement.InteractWithPlayer(false);
            emanuelMovement.EndInteraction();
        }

        if (IsDialogueComplete())
        {
            Debug.Log("Emanuel dialogue completed. Invoking OnDialogueCompleted event.");
            OnDialogueCompleted?.Invoke();
        }
        else
        {
            Debug.Log("Dialogue not complete. Resetting dialogue.");
            emanuelDialogue.ResetDialogue();
        }
    }

    private IEnumerator InteractionCooldown()
    {
        _canInteract = false;
        yield return new WaitForSeconds(interactionCooldown);
        _canInteract = _playerInRange;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}