using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events;
using UnityEditor;

public class DalilaInteraction : MonoBehaviour
{
    //Check Adila interaction script for comments

    [Header("References")]
    [SerializeField] private DalilaDialogue dalilaDialogue;
    [SerializeField] private Canvas interactionCanvas;
    [SerializeField] private Canvas dialogueCanvas;
    [SerializeField] private DalilaHeadLook dalilaHeadLookAt;
    [SerializeField] private Transform _playerHead;  

    [Header("Settings")]
    [SerializeField] private float interactionRadius = 5f;
    [SerializeField] private float timeToResetDialogue = 5f;
    [SerializeField] private float lookAtHeightOffset = 1.7f;
    [SerializeField] private float interactionCooldown = 1f;

    [Header("Audio Settings (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip interactionClip;  

    private Transform _playerTransform;
    private bool _isEngaged;
    private bool _playerInRange;
    private float _timeOutsideRadius;
    private float _lastInteractionTime;
    private bool _canInteract = true;

    [SerializeField] private DalilaMovement dalilaMovement;
    [SerializeField] private CharacterAI characterAI;   
    private void Awake()
    {
        ValidateReferences();
        InitializeUI();
        _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        dalilaDialogue.onDialogueEnd.AddListener(OnDialogueEnd);      
    }

    private void Update()
    {
        HandlePlayerDetection();
        UpdateHeadTracking();
        HandleDialogueTimeout();

        if (_playerInRange && Input.GetKeyDown(KeyCode.E) && _canInteract && !_isEngaged && !characterAI.IsBusyWithItem)
        {
            StartInteraction();
        }
    }

    private void ValidateReferences()
    {
        if (!dalilaHeadLookAt) dalilaHeadLookAt = GetComponent<DalilaHeadLook>();
        Debug.Assert(dalilaHeadLookAt != null, "HeadLook component is missing from NPC", this);
        Debug.Assert(dalilaDialogue != null, "dalilaDialogue reference is missing", this);
        Debug.Assert(interactionCanvas != null, "Interaction canvas reference is missing", this);
        Debug.Assert(dialogueCanvas != null, "Dialogue canvas reference is missing", this);
        if (dalilaMovement == null) dalilaMovement = GetComponent<DalilaMovement>();
        Debug.Assert(dalilaMovement != null, "dalilaMovement reference is missing", this);   
        Debug.Assert(characterAI != null, "CharacterAI reference is missing", this);
    }

    private void OnDialogueEnd()
    {
        ResetDialogueSystem();      
    }

    private void InitializeUI()
    {
        interactionCanvas.gameObject.SetActive(false);
        dialogueCanvas.gameObject.SetActive(false);
    }

    private void HandlePlayerDetection()
    {
        bool wasInRange = _playerInRange;
        _playerInRange = Physics.CheckSphere(transform.position, interactionRadius, LayerMask.GetMask("Player"));

        if (_playerInRange && !wasInRange)
        {
            Debug.Log("Player entered Adila's range!");
        }
        else if (!_playerInRange && wasInRange)
        {
            Debug.Log("Player exited Adila's range!");
        }

        interactionCanvas.gameObject.SetActive(_playerInRange && !_isEngaged);

        if (_playerInRange && _playerTransform == null)
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;          
        }
    }

    private void ResetDialogueSystem()
    {
        _isEngaged = false;
        _timeOutsideRadius = 0f;
        ToggleUI(false);
        LockCursor(true);        
        dalilaHeadLookAt.StopLooking();
        dalilaMovement.EndInteraction();     
    }



    private void UpdateHeadTracking()
    {
        if (_playerInRange && _playerTransform && !_isEngaged)
        {
            Vector3 lookPosition = GetPlayerHeadPosition();
            dalilaHeadLookAt.LookAtPosition(lookPosition);
        }
        else
        {
            dalilaHeadLookAt.StopLooking();
        }
    }

    private void HandleDialogueTimeout()
    {
        if (_isEngaged && !_playerInRange)
        {
            _timeOutsideRadius += Time.deltaTime;
            if (_timeOutsideRadius >= timeToResetDialogue)
            {
                ResetDialogueSystem();
            }
        }
    }

    private void StartInteraction()
    {       
        if (dalilaMovement != null)
        {
            dalilaMovement.InteractWithPlayer(true);
        }
        _isEngaged = true;
        ToggleUI(true);          
        interactionCanvas.gameObject.SetActive(false);  
        LockCursor(false);

        if (audioSource != null && interactionClip != null)
        {
            audioSource.PlayOneShot(interactionClip);
        }

        dalilaDialogue.enabled = true;
        dalilaDialogue.ResetDialogue();
        dalilaHeadLookAt.LookAtPosition(GetPlayerHeadPosition());

        StartCoroutine(InteractionCooldown());
    }

    public void StartInteractionWithPlayer(Transform interactorTransform)
    {
        StartInteraction();
    }

    private Vector3 GetPlayerHeadPosition()
    {
        return _playerHead ? _playerHead.position : _playerTransform.position + Vector3.up * lookAtHeightOffset;
    }

    private void ToggleUI(bool dialogueActive)
    {
        interactionCanvas.gameObject.SetActive(false);
        dialogueCanvas.gameObject.SetActive(dialogueActive);
    }

    private void LockCursor(bool locked)
    {
        Cursor.visible = !locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private IEnumerator InteractionCooldown()
    {
        _canInteract = false;
        yield return new WaitForSeconds(interactionCooldown);
        _canInteract = _playerInRange;      }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}