using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUi;
    public GameObject confirmQuit;
    public GameObject confirmMenu;
    public GameObject interactionMenu;
    public GameObject transcriptMenu;
    public GameObject objectiveMenu;
    public GameObject settingsMenu;
    public GameObject statisticsMenu;
    public Text transcriptText;
    public Text objectiveText;
    public Text statisticsText;

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
        transcriptMenu.SetActive(false);
        objectiveMenu.SetActive(false);
        statisticsMenu.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        interactionMenu.SetActive(true);
        Player.Instance.hintSystem.SetActive(true);
        Player.Instance.CloseHint();
        Statistics.Instance.ResumeTimer();
        Player.Instance.PauseMenuStatus(false);
    }

    public void Pause()
    {
        pauseMenuUi.SetActive(true);
        interactionMenu.SetActive(false);
        Player.Instance.hintSystem.SetActive(false);
        Player.Instance.PauseMenuStatus(true);
        Time.timeScale = 0f;
        Statistics.Instance.PauseTimer();
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

    public void ClosePause()
    {
        pauseMenuUi.SetActive(false);
    }

    public void OpenTranscript()
    {
        transcriptMenu.SetActive(true);
        transcriptText.text = Player.Instance.GetTranscript();
        pauseMenuUi.SetActive(false);
    }

    public void CloseTranscript()
    {
        transcriptMenu.SetActive(false);
        pauseMenuUi.SetActive(true);
    }

    public void SetTranscript()
    {
        transcriptText.text = Player.Instance.GetTranscript();
    }

    public void OpenObjective()
    {
        pauseMenuUi.SetActive(false);
        objectiveMenu.SetActive(true);
        SetObjective();
    }

    public void CloseObjective()
    {
        objectiveMenu.SetActive(false);
        pauseMenuUi.SetActive(true);
    }

    public void SetObjective()
    {
        objectiveText.text = PuzzleManager.Instance.GetCurrentArea().GetObjective();
    }

    public void OpenStatistics()
    {
        pauseMenuUi.SetActive(false);
        statisticsMenu.SetActive(true);
        SetStatistics();
    }

    public void CloseStatistics()
    {
        statisticsMenu.SetActive(false);
        pauseMenuUi.SetActive(true);
    }

    public void SetStatistics()
    {
        statisticsText.text = Statistics.Instance.GetAllStatsAsString();
    }
}
