using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Randomiser : MonoBehaviour
{
    public GameObject[] prefabs;
        
    // Start is called before the first frame update
    void Start()
    {
        if (prefabs.Length > 0)
        {
            GameObject spawn = prefabs[Random.Range(0, prefabs.Length)];
            GameObject.Instantiate(spawn, this.transform.position, Quaternion.identity,this.transform);
        }
    }
}
