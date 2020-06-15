using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.SceneManagement;


[System.Serializable]
public class Intro : MonoBehaviour
{
    [TextArea(3,10)]
    public string[] sentences;
    public Text textBox;
    private int index;
    public bool lastPart;
    public GameObject nextInterface;
    public GameObject currentInterface;
    public float characterPrintDelay;
    public GameObject continueButton;
    public AudioSource typingAudio;

    private bool _isTyping = false;

    public void Start()
    {
        index = 0;
        NextSentence();
        
    }

    public void previousSentence()
    {
        if (index - 1 >= 0)
        {
            index--;
            textBox.text = sentences[index];
            index++;
        }
    }

    public void NextSentence()
    {
        if (_isTyping)
        {
            StopAllCoroutines();
            textBox.text = sentences[index-1];
            _isTyping = false;
        }
        else
        {
            if (index < sentences.Length)
            {
                //textBox.text = sentences[index];
                StopAllCoroutines();
                StartCoroutine(TypeSentenceSlowly(sentences[index], textBox, characterPrintDelay));
                index++;
            }
            else if (!lastPart)
                TriggerNextInterface(nextInterface);
        }
    }

    /*public void SkipAll()
    {
        index = sentences.Length;
        end = true;
        TriggerNextInterface(nextInterface);
    }*/

    public void TriggerNextInterface(GameObject nextInterface)
    {
        nextInterface.SetActive(true);
        currentInterface.SetActive(false);
    }

    public void SwitchButton(GameObject newButton)
    {
        if (index >= sentences.Length)
        {
            newButton.SetActive(true);
            continueButton.SetActive(false);
        }
    }

    public void EndIntro()
    {
        nextInterface.SetActive(true);
        currentInterface.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    IEnumerator TypeSentenceSlowly(string sentence, Text goal)
    {//Brackeys, How to make a Dialogue System in Unity https://www.youtube.com/watch?v=_nRzoTzeyxU
        goal.text = "";
        foreach (char c in sentence.ToCharArray())
        {
            goal.text += c;
            typingAudio.Play();
            yield return null;
        }
    }

    IEnumerator TypeSentenceSlowly(string sentence, Text goal, float delay)
    {//Brackeys, How to make a Dialogue System in Unity https://www.youtube.com/watch?v=_nRzoTzeyxU
        _isTyping = true;
        if (delay == 0)
            TypeSentenceSlowly(sentence, goal);
        goal.text = "";
        bool odd = true;
        foreach (char c in sentence.ToCharArray())
        {
            goal.text += c;
            if (odd)
            {
                if(typingAudio != null)
                    typingAudio.Play();
                odd = false;
            }
            else
                odd = true;
            yield return new WaitForSeconds(delay);
        }
        _isTyping = false;
    }
}
