using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Ending : MonoBehaviour
{
    public GameObject endScreen;
    public GameObject credits;

    public void Start()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("Audio", 0.3f);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting");
        Application.Quit();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void Restart()
    {
        SceneManager.LoadScene(1);
    }

    public void ShowCredits()
    {
        endScreen.SetActive(false);
        credits.SetActive(true);
    }

    public void ReturnToEndScreen()
    {
        credits.SetActive(false);
        endScreen.SetActive(true);
    }
}
