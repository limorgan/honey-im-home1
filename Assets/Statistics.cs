using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statistics : MonoBehaviour
{
    private static Statistics _instance;
    public static Statistics Instance { get { return _instance; } }

    public Dictionary<Area, int> actionsPerArea = new Dictionary<Area, int>();
    public Dictionary<Area, int> inspectsPerArea = new Dictionary<Area, int>();
    public Dictionary<Area, int> timePerArea = new Dictionary<Area, int>();

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.Log("Destroying Statistics. ");
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            Debug.Log("Statistics instance created. ");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateActions(Area area)       //can be called to register a player interaction; updates number of actions only
    {
        if (!actionsPerArea.ContainsKey(area))
            actionsPerArea.Add(area, 0);
        actionsPerArea[area]++;
    }

    public void UpdateInspects(Area area)      //can be called to register an inspection, which is also counted as an action
    {
        if (!inspectsPerArea.ContainsKey(area))
            inspectsPerArea.Add(area, 0);
        inspectsPerArea[area]++;
        UpdateActions(area);
    }

    void UpdateTime(Area area, int seconds)
    {
        if (!timePerArea.ContainsKey(area))
            timePerArea.Add(area, seconds);
        else
            timePerArea[area] = seconds;
    }

    public Dictionary<string, int> GetAllStats(Area area)
    {
        Dictionary<string, int> fullStats = new Dictionary<string, int>();
        fullStats.Add("Actions", actionsPerArea[area]);
        fullStats.Add("Inspections", inspectsPerArea[area]);
        fullStats.Add("Time", timePerArea[area]);
  
    }
}
