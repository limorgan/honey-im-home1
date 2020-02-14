using UnityEngine;
using System.Collections;

public class GameArea : MonoBehaviour {
    public GameItem[] itemsInArea;
    public Area area;
    [SerializeField]
    private GameObject[] _spawnPoints;
    private int _index = 0;

    void Awake() {
        itemsInArea = this.GetComponentsInChildren<GameItem>();
        //Debug.Log(this.toString());
        _index = Random.Range(0, _spawnPoints.Length);
    }

    public Vector3 getNextSpawnPt() {
        _index++;
        if (_index > _spawnPoints.Length - 1)
            _index = 0;
        return _spawnPoints[_index].transform.position;
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
