using UnityEngine;
using System.Collections;

public class StudyRoom : MonoBehaviour
{
    // Tag used to identify the player GameObject.
    public string playerTag = "Player";
    // Tag used to identify the Room 3 hint item GameObject.
    public string room3HintItemTag = "Room3item";
    // Duration of the timer in seconds.
    public float timerDuration = 60f;
    // Reference to the guard AI script.
    public MonoBehaviour guardAI;

    // Reference to the QuestDirection script.
    public QuestDirection questDirection;

    // Flag to track if the timer is running.
    private bool timerRunning = false;
    // Current value of the timer.
    private float timer = 0f;
    // Flag to track if the Room 3 hint item has been closed.
    private bool room3HintItemClosed = false;
    // Reference to the Room3HintItem script.
    private Room3HintItem room3HintItem;

    // Called when the script instance is being loaded.
    private void Start()
    {
        // Find the Room 3 hint item GameObject and get its Room3HintItem component.
        GameObject room3HintItemObject = GameObject.FindGameObjectWithTag(room3HintItemTag);
        if (room3HintItemObject != null)
        {
            room3HintItem = room3HintItemObject.GetComponent<Room3HintItem>();
            // Log an error if the Room3HintItem component is not found.
            if (room3HintItem == null)
            {
                Debug.LogError("StudyRoom: Room3HintItem component not found on GameObject with tag " + room3HintItemTag);
            }
        }
        // Log an error if the Room 3 hint item GameObject is not found.
        else
        {
            Debug.LogError("StudyRoom: GameObject with tag " + room3HintItemTag + " not found.");
        }
    }

    // Called once per frame.
    private void Update()
    {
        // Update the timer if it's running.
        if (timerRunning)
        {
            timer += Time.deltaTime;
            // Stop the timer and notify security when the timer reaches its duration.
            if (timer >= timerDuration)
            {
                timerRunning = false;
                timer = 0f;
                NotifySecurity();
            }
        }
    }

    // Called when another collider enters the trigger.
    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering collider is the player.
        if (other.CompareTag(playerTag))
        {
            // Check the Room 3 hint item and start the timer if necessary.
            CheckRoom3HintItemAndStartTimer();

            // Advance the quest stage if the QuestDirection script is assigned.
            if (questDirection != null)
            {
                questDirection.AdvanceQuestStage();
            }
            // Log an error if the QuestDirection script is not assigned.
            else
            {
                Debug.LogError("StudyRoom: QuestDirection reference not set.");
            }
        }
    }

    // Checks the Room 3 hint item and starts the timer if it's closed.
    private void CheckRoom3HintItemAndStartTimer()
    {
        // Check if the Room 3 hint item reference is not null.
        if (room3HintItem != null)
        {
            // Check if the Room 3 hint item's menu UI is not active.
            room3HintItemClosed = !room3HintItem.IsMenuUIActive();
            // Start the timer if the Room 3 hint item is closed.
            if (room3HintItemClosed)
            {
                StartTimer();
            }
        }
        // Log an error if the Room 3 hint item reference is null.
        else
        {
            Debug.LogError("StudyRoom: Room3HintItem reference is null.");
        }
    }

    // Starts the timer.
    private void StartTimer()
    {
        // Set the timer running flag and reset the timer.
        timerRunning = true;
        timer = 0f;
        Debug.Log("StudyRoom: Timer started.");
    }

    // Notifies the security guard to chase the player.
    private void NotifySecurity()
    {
        // Find the player's transform.
        Transform playerTransform = GameObject.FindGameObjectWithTag(playerTag)?.transform;
        // Notify the appropriate guard AI to chase the player.
        if (playerTransform != null)
        {
            if (guardAI is SecurityAI)
            {
                ((SecurityAI)guardAI).StartChasingPlayer(playerTransform);
                Debug.Log("StudyRoom: SecurityAI notified to chase the player.");
            }
            else if (guardAI is EuodiaAI)
            {
                ((EuodiaAI)guardAI).StartChasingPlayer(playerTransform);
                Debug.Log("StudyRoom: EuodiaAI notified to chase the player.");
            }
            else if (guardAI is AlenAI)
            {
                ((AlenAI)guardAI).StartChasingPlayer(playerTransform);
                Debug.Log("StudyRoom: AlenAI notified to chase the player.");
            }
        }
        // Log an error if the player's transform is not found.
        else
        {
            Debug.LogError("StudyRoom: Player transform not found.");
        }

        // Log an error if the guard AI reference is not set.
        if (guardAI == null)
        {
            Debug.LogError("StudyRoom: GuardAI reference not set.");
        }
    }
}