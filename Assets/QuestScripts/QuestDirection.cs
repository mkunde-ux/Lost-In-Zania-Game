using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class QuestDirection : MonoBehaviour
{
    // Define quest stages for clarity.
    public enum QuestStage
    {
        InitialObjective,          // Stage 0: Upon entering the restaurant.
        ClueDiscovery,              // Stage 1: When the player interacts with a clue.
        SecurityGuardInteraction,   // Stage 2: After dialogue with Michael.
        Room3Discovery,             // Stage 3: When the clue in Room 3 is found.
        FinalObjective              // Stage 4: Upon entering the Study Room.
    }

    // UI references for displaying the checklist.
    [Header("UI References")]
    public TMP_Text checklistText;
    public CanvasGroup checklistCanvasGroup;

    // Animation settings for fading the checklist.
    [Header("Animation Settings")]
    public float fadeDuration = 1f;

    // Events to trigger when the quest stage is updated.
    [Header("Events")]
    public UnityEvent<QuestStage> onQuestUpdated;

    // Array of objectives corresponding to each quest stage.
    private readonly string[] objectives = new string[]
    {
        "Look for clues regarding David and Matias's stolen phones.",
        "Find and speak with the security guard, Michael.",
        "Locate room 3 and find a clue.",
        "Proceed to the Study Room.",
        "Quickly locate David and Matias's phones and sneak out before security arrives!"
    };

    // Current quest stage.
    public QuestStage currentStage = QuestStage.InitialObjective;

    // Called when the script instance is being loaded.
    void Start()
    {
        // Initialize the checklist UI.
        UpdateChecklist();
    }

    // Sets the quest stage to a specific stage.
    public void SetQuestStage(QuestStage newStage)
    {
        // Update the current stage and checklist.
        currentStage = newStage;
        UpdateChecklist();

        // Invoke the quest updated event.
        if (onQuestUpdated != null)
        {
            onQuestUpdated.Invoke(newStage);
        }
    }

    // Advances the quest stage to the next stage.
    public void AdvanceQuestStage()
    {
        // Calculate the next stage index.
        int nextStageIndex = (int)currentStage + 1;
        // Check if the next stage index is within the bounds of the objectives array.
        if (nextStageIndex < objectives.Length)
        {
            // Set the quest stage to the next stage.
            SetQuestStage((QuestStage)nextStageIndex);
        }
        // Log a message if the quest is completed.
        else
        {
            Debug.Log("Quest completed!");
        }
    }

    // Updates the checklist UI with the current objective.
    private void UpdateChecklist()
    {
        // Check if the checklist text and canvas group are assigned and the current stage is within bounds.
        if (checklistText != null && objectives.Length > (int)currentStage)
        {
            // Update the checklist text.
            checklistText.text = objectives[(int)currentStage];
            // Start the fade-in animation if the canvas group is assigned.
            if (checklistCanvasGroup != null)
            {
                StopAllCoroutines();
                StartCoroutine(FadeIn());
            }
        }
    }

    // Coroutine for fading in the checklist.
    private IEnumerator FadeIn()
    {
        // Initialize variables for the fade animation.
        float elapsed = 0f;
        checklistCanvasGroup.alpha = 0f;
        // Loop until the fade duration is reached.
        while (elapsed < fadeDuration)
        {
            // Update the elapsed time and alpha value.
            elapsed += Time.deltaTime;
            checklistCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        // Set the alpha to 1 when the fade is complete.
        checklistCanvasGroup.alpha = 1f;
    }
}