using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingTrigger : MonoBehaviour
{
    public void EndGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LowerVolume(float change)
    {
        Debug.Log("Volume: " + AudioListener.volume);
        if (AudioListener.volume - change >= 0f)
            AudioListener.volume -= change;
        else
            AudioListener.volume = 0f;
    }
}
