using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUi;
    public GameObject pauseButton;
    public GameObject inventoryButton;

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
        pauseButton.SetActive(true);
        if (inventoryButton != null)
            inventoryButton.SetActive(true);
    }

    public void Pause()
    {
        pauseMenuUi.SetActive(true);
        pauseButton.SetActive(false);
        if (inventoryButton != null)
            inventoryButton.SetActive(false);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }
}
