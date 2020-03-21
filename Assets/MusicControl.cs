using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicControl : MonoBehaviour
{
    [SerializeField]
    private AudioSource music;

    private GameItem item;
    private bool muted = false;
    private bool _volumeDown = false;

    [SerializeField]
    private string area;

    private void Start()
    {
        music.volume = 0.6f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.Instance.areaText.text.ToLower() == area.ToLower())
        {
            item = this.gameObject.GetComponent<GameItem>();
            if (item.GetProperty("ison") != null && item.GetProperty("ison").value == "True")
                if (!music.isPlaying && muted == false)
                {
                    music.Play();
                    TurnOnVolume();
                }
            if (music.isPlaying && muted == false)
            {
                if (_volumeDown)
                    TurnOnVolume();
                Player.Instance.gameObject.GetComponent<AudioSource>().volume = 0.05f;
            }
            else
                Player.Instance.gameObject.GetComponent<AudioSource>().volume = 1f;
        }
        else
            TurnDownVolume();
    }

    public void TurnDownVolume()
    {
        music.volume = 0f;
        _volumeDown = true;
    }

    public void TurnOnVolume()
    {
        music.volume = 0.6f;
        _volumeDown = false;
    }

    public void StopPlaying()
    {
        music.Stop();
    }

    public void Mute()
    {
        muted = true;
    }

    public bool IsMuted()
    {
        return muted;
    }

    public void Unmute()
    {
        TurnOnVolume();
        muted = false;
    }
}
