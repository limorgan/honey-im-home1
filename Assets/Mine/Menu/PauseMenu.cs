using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUi;
    public GameObject confirmQuit;
    public GameObject confirmMenu;
    public GameObject interactionMenu;

    void Start()
    {
        pauseMenuUi.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (GameIsPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUi.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        interactionMenu.SetActive(true);
        Player.Instance.hintSystem.SetActive(true);
        Player.Instance.CloseHint();
        Player.Instance.PauseMenuStatus(false);
    }

    public void Pause()
    {
        pauseMenuUi.SetActive(true);
        interactionMenu.SetActive(false);
        Player.Instance.hintSystem.SetActive(false);
        Player.Instance.PauseMenuStatus(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void ConfirmOption(bool quit)        
    {
        if (quit)
            confirmQuit.SetActive(true);
        else
            confirmMenu.SetActive(true);
        pauseMenuUi.SetActive(false);
    }

    public void CloseConfirm()
    {
        confirmQuit.SetActive(false);
        confirmMenu.SetActive(false);
        interactionMenu.SetActive(true);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting");
        Application.Quit();
    }
}
