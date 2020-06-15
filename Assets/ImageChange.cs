using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageChange : MonoBehaviour
{
    // Addition: changing the appearance of an item based on change in properties. Very basic and contains no error checking,
    // provided more for demonstration and added more visual effects to the game

    [SerializeField]
    private GameObject _imageToAdd;
    [SerializeField]
    private string _propertyName;
    [SerializeField]
    private string _propertyValue;
    private bool _done;

    private void Start()
    {
        _done = false;
        _imageToAdd.SetActive(false);
    }

    void Update()
    {
        if (!_done)
        {
            if (gameObject.GetComponent<GameItem>().GetProperty(_propertyName) != null
                && gameObject.GetComponent<GameItem>().GetProperty(_propertyName).value == _propertyValue)
            {
                _imageToAdd.SetActive(true);
                _done = true;
            }
        }
    }

}
