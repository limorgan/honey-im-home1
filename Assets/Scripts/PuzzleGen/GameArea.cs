using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameArea : MonoBehaviour {
    public GameItem[] itemsInArea;
    public Area area;
    public GameObject areaContent;              // 05/03 using for activation on starting/entering
    [SerializeField]
    private GameObject[] _spawnPoints;
    [SerializeField]
    private List<GameObject> _specificSpawnPoints = new List<GameObject>();
    [SerializeField]
    private List<Item> _spawnItems = new List<Item>();
    [SerializeField]
    private GameObject[] _NPCSpawnPoints;       // 04/03 adding option of specific NPC spawn points

    private int _index = 0;
    private int _NPCindex = 0;

    void Awake() {
        itemsInArea = this.GetComponentsInChildren<GameItem>();
        _index = Random.Range(0, _spawnPoints.Length);
        _NPCindex = Random.Range(0, _NPCSpawnPoints.Length);
        area.areaObject = areaContent;                // 05/03 associating actual content with the area
    }

    public Vector3 GetNextSpawnPt(bool NPC)     //if item to be spawned is an NPC
    {
        if (NPC)
        {
            if (_NPCSpawnPoints.Length == 0)    //if no NPC spawn points are given, use regular spawn points
                return GetNextSpawnPt();
            _NPCindex++;
            if (_NPCindex > _NPCSpawnPoints.Length - 1)
                _NPCindex = 0;
            return _NPCSpawnPoints[_NPCindex].transform.position;

        }
        else
            return GetNextSpawnPt();
    }

    public Vector3 GetNextSpawnPt(Item item)        //for specific spawn point cases
    {
        if (item.specificSpawnPoints)
        {
            int idx = GetIndex(item);
            if (idx >= 0)
            {
                if (_spawnPoints.Length - 1 < idx)
                    return GetNextSpawnPt();
                return _spawnPoints[idx].transform.position;
            }
            else
                return GetNextSpawnPt();
            
        }
        return GetNextSpawnPt();
    }

    public Vector3 GetNextSpawnPt() {
        _index++;
        if (_index > _spawnPoints.Length - 1)
            _index = 0;
        return _spawnPoints[_index].transform.position;
    }

    public Vector3 GetRandomSpawnPt(bool NPC)
    {
        if (NPC)
        {
            if (_NPCSpawnPoints.Length > 0)
                return _NPCSpawnPoints[Random.Range(0, _NPCSpawnPoints.Length)].transform.position;
            else
                return getRandomSpawnPt();
        }
        else
            return getRandomSpawnPt();
    }

    public Vector3 getRandomSpawnPt() {
        return _spawnPoints[Random.Range(0, _spawnPoints.Length)].transform.position;
    }

    public override string ToString()
    {
        string debug_str = "area: " + area.name + " items ";
        foreach (GameItem g in itemsInArea)
            debug_str += g.ToString() + " ";
        return debug_str;
    }

    public int GetIndex(Item item)
    {
        if (_spawnItems.Contains(item))
            return _spawnItems.IndexOf(item);
        else
            return -1;
    }
    
    /*public string getHint()
    {
        return area.getCurrentGoal().description;   //returns the area goal only - not for the individual rules...
    }*/
}
