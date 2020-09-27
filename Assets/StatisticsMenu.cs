using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatisticsMenu : MonoBehaviour
{
    public List<Area> areasDiscovered = new List<Area>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GetStats()
    {
        areasDiscovered.ForEach(delegate (Area area)
        {
            GetStatsPerArea(area);
        });
    }

    void GetStatsPerArea(Area area)
    {
        Dictionary<string, int> stats = Statistics.Instance.GetAllStats(area);
    }

    void AddArea(Area area)
    {

    }
}
