using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatisticsMenu : MonoBehaviour
{
    public GameObject statsMenu;
    public GameObject areaStats;
    public GameObject puzzleStats;
    
    [SerializeField]
    private List<StatsBox> _areaBoxes = new List<StatsBox>();
    [SerializeField]
    private List<StatsBox> _puzzleBoxes = new List<StatsBox>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayAreaStats()
    {
        areaStats.SetActive(true);
        List<Area> areasDiscovered = Statistics.Instance.GetAreasDiscovered();
        int index = 0;
        foreach (Area area in areasDiscovered)
        {
            DisplayStatsPerArea(area, index);
            index++;
        }
        for(; index < Statistics.Instance.GetNumberOfAreas(); index++)
        {
            DisplayStatsUndiscoveredArea(index);
        }
    }

    public void CloseAreaStats()
    {
        areaStats.SetActive(false);
    }

    public void DisplayPuzzleStats()
    {
        puzzleStats.SetActive(true);
        List<Area> puzzlesDiscovered = Statistics.Instance.GetPuzzlesDiscovered();
        int index = 0;
        foreach (Area area in puzzlesDiscovered)
        {
            DisplayStatsPerPuzzle(area, index);
            index++;
        }
        for (; index < Statistics.Instance.GetNumberOfAreas(); index++)
        {
            DisplayStatsUndiscoveredPuzzle(index);
        }
    }

    public void ClosePuzzleStats()
    {
        puzzleStats.SetActive(false);
    }

    public void DisplayStatsPerArea(Area area, int index)
    {
        /*Dictionary<string, int> stats = Statistics.Instance.GetAllStats(area);
        GameObject statsBox = GameObject.Instantiate(statsBoxTemplate);
        //GameObject statsHeader = GameObject.Instantiate(statsHeaderTemplate);
        StatsBox.CreateComponent(statsBox, area, stats);
        statsBox.transform.SetParent(statsMenu.transform);*/
        if(_areaBoxes.Count <= index)
        {
            Debug.Log("Insufficient area boxes");
            return;
        }
        _areaBoxes[index].DisplayBoxArea(area);
    }

    public void DisplayStatsUndiscoveredArea(int index)
    {
        if (_areaBoxes.Count <= index)
        {
            Debug.Log("Insufficient area boxes");
            return;
        }
        _areaBoxes[index].DisplayBoxUndiscovered(false);
    }

    public void DisplayStatsPerPuzzle(Area area, int index)
    {
        if (_puzzleBoxes.Count <= index)
        {
            Debug.Log("Insufficient puzzle boxes");
            return;
        }
        _puzzleBoxes[index].DisplayBoxPuzzle(area, index + 1);
    }

    public void DisplayStatsUndiscoveredPuzzle(int index)
    {
        if (_puzzleBoxes.Count <= index)
        {
            Debug.Log("Insufficient puzzle boxes");
            return;
        }
        _puzzleBoxes[index].DisplayBoxUndiscovered(true, index + 1);
    }
}
