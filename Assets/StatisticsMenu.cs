using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatisticsMenu : MonoBehaviour
{
    [SerializeField]
    public GameObject statsMenu;
    [SerializeField]
    public GameObject statsHeaderTemplate;
    [SerializeField]
    public GameObject statsBoxTemplate;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DisplayStats()
    {
        List<Area> areasDiscovered = Statistics.Instance.GetAreasDiscovered();
        foreach (Area area in areasDiscovered)
        {
            DisplayStatsPerArea(area);
        }
    }

    void DisplayStatsPerArea(Area area)
    {
        Dictionary<string, int> stats = Statistics.Instance.GetAllStats(area);
        GameObject statsBox = GameObject.Instantiate(statsBoxTemplate);
        //GameObject statsHeader = GameObject.Instantiate(statsHeaderTemplate);
        StatsBox.CreateComponent(statsBox, area, stats);
        statsBox.transform.SetParent(statsMenu.transform);
    }
}
