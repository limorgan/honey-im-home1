using UnityEngine;
using System.Collections;

public class GameArea : MonoBehaviour {
    public GameItem[] itemsInArea;
    public Area area;
    [SerializeField]
    private GameObject[] _spawnPoints;
    [SerializeField]
    private GameObject[] _NPCSpawnPoints;       // 04/03 adding option of specific NPC spawn points
    private int _index = 0;
    private int _NPCindex = 0;

    void Awake() {
        itemsInArea = this.GetComponentsInChildren<GameItem>();
        //Debug.Log(this.toString());
        _index = Random.Range(0, _spawnPoints.Length);
    }

    public Vector3 getNextSpawnPt(bool NPC)     //if item to be spawned is an NPC
    {
        if (NPC)
        {
            if (_NPCSpawnPoints.Length == 0)    //if no NPC spawn points are given, use regular spawn points
                return getNextSpawnPt();
            _NPCindex++;
            if (_NPCindex > _NPCSpawnPoints.Length - 1)
                _NPCindex = 0;
            return _NPCSpawnPoints[_NPCindex].transform.position;

        }
        else
            return getNextSpawnPt();
    }

    public Vector3 getNextSpawnPt() {
        if (_spawnPoints.Length == 0)
            Debug.Log("no spawn points");
        _index++;
        if (_index > _spawnPoints.Length - 1)
            _index = 0;
        Debug.Log("Spawn point " + _index + ": " + _spawnPoints[_index].transform.position);
        return _spawnPoints[_index].transform.position;
    }

    public Vector3 getRandomSpawnPt(bool NPC)
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

    public string toString()
    {
        string debug_str = "area: " + area.name + " items ";
        foreach (GameItem g in itemsInArea)
            debug_str += g.toString() + " ";
        return debug_str;
    }
}
