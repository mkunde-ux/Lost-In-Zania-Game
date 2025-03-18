using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("DialogueSystem");
    }
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}