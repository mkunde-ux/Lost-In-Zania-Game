using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;


public class PauseMenu : MonoBehaviour
{
    [SerializeField] public GameObject pauseMenuUI;
    public static bool isPaused = false;

    void Start()
    {
        pauseMenuUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }

        }
    }
    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void QuitToMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}