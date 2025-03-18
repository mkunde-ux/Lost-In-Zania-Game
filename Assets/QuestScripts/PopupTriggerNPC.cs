using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupTriggerNPC : MonoBehaviour
{
    // Reference to the GameObject of the popup panel.
    [SerializeField] private GameObject popupPanel;
    // Reference to the UI Text element for displaying the popup message.
    [SerializeField] private Text popupText;

    // Method to show a popup with the specified message.
    public void ShowPopup(string message)
    {
        // Check if the popup panel and text are assigned.
        if (popupPanel != null && popupText != null)
        {
            // Set the text of the popup message.
            popupText.text = message;
            // Activate the popup panel.
            popupPanel.SetActive(true);
            // Schedule the HidePopup method to be called after a 2-second delay.
            Invoke("HidePopup", 2f);
        }
        else
        {
            // Log a warning if the popup panel or text is not assigned.
            Debug.LogWarning("PopupPanel or PopupText is not assigned in PopupTrigger!");
        }
    }

    // Method to hide the popup panel.
    private void HidePopup()
    {
        // Check if the popup panel is assigned.
        if (popupPanel != null)
        {
            // Deactivate the popup panel.
            popupPanel.SetActive(false);
        }
    }
}