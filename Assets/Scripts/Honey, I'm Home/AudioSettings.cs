using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class AudioSettings : MonoBehaviour
{
    public float defaultVolume;
    public GameObject unmutedButton;
    public GameObject mutedButton;
    public Slider volumeSlide;
    public GameObject sliderParent;
    public bool slider;

    // Start is called before the first frame update
    void Start()
    {
        AudioListener.volume = defaultVolume;
        if (!slider)
            sliderParent.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("Audio");
        if (AudioListener.volume == 0)
        {
            mutedButton.SetActive(true);
            unmutedButton.SetActive(false);
        }
        else
        {
            mutedButton.SetActive(false);
            unmutedButton.SetActive(true);
        }
        if(slider)
            volumeSlide.value = AudioListener.volume;
    }

    public void MuteSound()
    {
        PlayerPrefs.SetFloat("Audio", 0f);
        mutedButton.SetActive(true);
        unmutedButton.SetActive(false);
    }

    public void UnmuteSound()
    {
        ChangeVolume(defaultVolume);
        mutedButton.SetActive(false);
        unmutedButton.SetActive(true);
    }

    public void ChangeVolume(float volume)
    {
        if (volume > 1f)
            volume = 1f;
        PlayerPrefs.SetFloat("Audio", volume);
    }

    public void ChangeVolume()
    {
        ChangeVolume(volumeSlide.value);
    }

    public float GetVolume()
    {
        return PlayerPrefs.GetFloat("Audio", defaultVolume);
    }
}
