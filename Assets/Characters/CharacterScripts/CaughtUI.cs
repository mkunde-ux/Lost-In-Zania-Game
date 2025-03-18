using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
public class CaughtUIManager : MonoBehaviour
{
    // Reference to the Canvas component for the Caught UI.
    [SerializeField] private Canvas caughtUICanvas;

    // Tracks the state of the Caught UI.
    private bool isCaught = false;

    // Called when the script instance is being loaded.
    void Start()
    {
        // Initially disable the canvas GameObject.
        if (caughtUICanvas != null)
        {
            caughtUICanvas.gameObject.SetActive(false);
        }
        // Log an error if the Caught UI Canvas is not assigned in the inspector.
        else
        {
            Debug.LogError("Caught UI Canvas is not assigned in the inspector!");
        }
    }

    // Call this method when the player is caught.
    public void ShowCaughtUI()
    {
        // Enable the Caught UI Canvas and pause the game.
        if (caughtUICanvas != null)
        {
            caughtUICanvas.gameObject.SetActive(true);
            isCaught = true;
            Time.timeScale = 0f; // Pause the game
        }
    }

    // Call this method to hide the Caught UI.
    public void HideCaughtUI()
    {
        // Disable the Caught UI Canvas and resume the game.
        if (caughtUICanvas != null)
        {
            caughtUICanvas.gameObject.SetActive(false);
            isCaught = false;
            Time.timeScale = 1f; // Resume the game
        }
    }

    // Example functionality for restart button.
    public void RestartLevel()
    {
        // Ensure the game is running before reloading.
        Time.timeScale = 1f;
        // Reload the current scene.
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    // Example functionality for quit button.
    public void QuitToMainMenu()
    {
        // Reset game time before changing scenes.
        Time.timeScale = 1f;
        // Load the Main Menu scene.
        SceneManager.LoadScene("MainMenu");
    }
}