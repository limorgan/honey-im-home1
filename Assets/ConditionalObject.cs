using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionalObject : MonoBehaviour
{
    [SerializeField]
    public Item condition;    //if this game item is used in the current puzzle, the attached game object should be activated
    [SerializeField]
    public Area area;             //if in this area
    [SerializeField]
    public GameObject affectedItem;
}
