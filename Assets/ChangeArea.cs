using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeArea : MonoBehaviour
{
    public GameObject spawnPoint;
    public bool zoomOut = false;
    public bool locked = true;
    //public GameObject currentArea;
    //public GameObject nextArea;
     
    
    // Start is called before the first frame update
    void Start()
    {
        //this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (locked)
            locked = checkLock();
    }   

    private bool checkLock()
    {
        if (this.GetComponent<GameItem>() != null)
        {
            Property p = this.GetComponent<GameItem>().GetProperty("locked");
            if (p != null)
            {
                if (p.value == "False")
                    return false;
                else
                    return true;
            }
            return false;
        }
        else
            return false;
    }

}
