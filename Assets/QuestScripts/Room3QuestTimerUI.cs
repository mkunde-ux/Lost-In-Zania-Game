using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class Room3QuestTimerUI : MonoBehaviour
{
    // UI references for the timer display.
    [Header("UI References")]
    public Canvas timerCanvas;
    public TextMeshProUGUI timerText;

    // Settings for the timer.
    [Header("Timer Settings")]
    public UnityEvent onTimerComplete;

    // Current value of the timer.
    private float currentTimer = 0f;
    // Flag to track if the timer is running.
    private bool isTimerRunning = false;

    // Called when the script instance is being loaded.
    void Start()
    {
        // Disable the timer canvas initially.
        if (timerCanvas != null)
        {
            timerCanvas.gameObject.SetActive(false);
        }
        // Log an error if the timer canvas is not assigned.
        else
        {
            Debug.LogError("Timer Canvas is not assigned in the Inspector!");
        }

        // Log an error if the timer text is not assigned.
        if (timerText == null)
        {
            Debug.LogError("Timer Text (TextMeshProUGUI) is not assigned in the Inspector!");
        }
    }

    // Called once per frame.
    void Update()
    {
        // Update the timer if it's running.
        if (isTimerRunning)
        {
            currentTimer -= Time.deltaTime;
            UpdateTimerDisplay();

            // Stop the timer when it reaches zero.
            if (currentTimer <= 0f)
            {
                StopTimer();
            }
        }
    }

    // Starts the timer with a specified duration.
    public void StartTimer(float duration)
    {
        // Log an error if the duration is not positive.
        if (duration <= 0)
        {
            Debug.LogError("Timer duration must be greater than 0!");
            return;
        }

        // Initialize the timer and set the running flag.
        currentTimer = duration;
        isTimerRunning = true;

        // Enable the timer canvas and log a message.
        if (timerCanvas != null)
        {
            timerCanvas.gameObject.SetActive(true);
            Debug.Log("Timer UI activated.");
        }

        // Log a message indicating the timer has started.
        Debug.Log($"Timer started: {duration} seconds.");
    }

    // Stops the timer and triggers the timer complete event.
    private void StopTimer()
    {
        // Set the running flag to false.
        isTimerRunning = false;

        // Disable the timer canvas and log a message.
        if (timerCanvas != null)
        {
            timerCanvas.gameObject.SetActive(false);
            Debug.Log("Timer UI deactivated.");
        }

        // Invoke the timer complete event and log a message.
        if (onTimerComplete != null)
        {
            onTimerComplete.Invoke();
            Debug.Log("Timer complete event triggered.");
        }

        // Log a message indicating the timer has completed.
        Debug.Log("Timer completed!");
    }

    // Public method to stop the timer.
    public void PublicStopTimer()
    {
        StopTimer();
    }

    // Updates the timer display with the current time.
    private void UpdateTimerDisplay()
    {
        // Update the timer text if it's assigned.
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTimer / 60);
            int seconds = Mathf.FloorToInt(currentTimer % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        // Log an error if the timer text is not assigned.
        else
        {
            Debug.LogError("Timer Text (TextMeshProUGUI) is not assigned!");
        }
    }
}