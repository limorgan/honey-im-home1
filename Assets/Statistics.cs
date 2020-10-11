using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class Statistics : MonoBehaviour
{
    private static Statistics _instance;
    public static Statistics Instance { get { return _instance; } }

    private List<Area> _areasDiscovered = new List<Area>();
    private List<Area> _puzzlesDiscovered = new List<Area>();
    private List<Area> _puzzlesFinished = new List<Area>();
    //private List<Area> _currentPuzzleArea = new List<Area>();     //to be used if multiple puzzles are available concurrently
    private Area _currentPuzzleArea;
    private Area _currentPlayerArea;

    //testing Timer
    public Text timerText;
    private float _timerTotal;
    private float _timerPlayer;      //time spent in a game area
    private float _timerPuzzle;      //time spent on a puzzle
    private bool _timerOn = false;

    //inactivity measurement
    public float timeOut;
    private float _timeSinceClick;
    private Vector3 _mousePosition;
    public PauseMenu pauseMenu;

    public Dictionary<string, Dictionary<Area, int>> fullStats = new Dictionary<string, Dictionary<Area, int>>();

    public Dictionary<Area, int> actionsPerArea = new Dictionary<Area, int>();
    public Dictionary<Area, int> actionsPerPuzzle = new Dictionary<Area, int>();
    public Dictionary<Area, int> inspectsPerArea = new Dictionary<Area, int>();
    public Dictionary<Area, int> inspectsPerPuzzle = new Dictionary<Area, int>();
    public Dictionary<Area, int> timePerArea = new Dictionary<Area, int>();
    public Dictionary<Area, int> timePerPuzzle = new Dictionary<Area, int>();

    private int _totalActions;
    private int _totalInspects;
    private int _totalAreas;            //the current assumption is #areas = #puzzles

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

        _timerOn = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_timerOn)
        {
            _timerTotal += Time.deltaTime;
            _timerPlayer += Time.deltaTime;
            _timerPuzzle += Time.deltaTime;

            if (!Input.anyKey)                         //track inactivity: no keys down and no mouse movement
            {
                Vector3 currentMousePosition = Input.mousePosition;
                if (currentMousePosition == _mousePosition)
                    _timeSinceClick += Time.deltaTime;
                else
                {
                    _mousePosition = currentMousePosition;
                    _timeSinceClick = 0;
                }
            }
            else
                _timeSinceClick = 0;
        }

        

        //TestTimers();               //testing timers
        if (_timeSinceClick >= timeOut)
            TriggerPause();
    }

    // == COUNTING INTERACTIONS == 

    public void UpdateActions(Area area, bool puzzle, bool player)       //can be called to register a player interaction; updates number of actions only
    {
        if (!actionsPerArea.ContainsKey(area))
            actionsPerArea.Add(area, 0);
        actionsPerArea[area]++;
        if (!actionsPerPuzzle.ContainsKey(_currentPuzzleArea))
            actionsPerPuzzle.Add(_currentPuzzleArea, 0);
        actionsPerPuzzle[_currentPuzzleArea]++;
        _totalActions++;
    }

    public void UpdateActions()     //updates stats for actions both on puzzle and area level
    {
        UpdateActions(_currentPlayerArea, true, true);
    }

    public void UpdateInspects(Area area, bool puzzle, bool player)      //can be called to register an inspection, which is also counted as an action
    {
        if (!inspectsPerArea.ContainsKey(area))
            inspectsPerArea.Add(area, 0);
        inspectsPerArea[area]++;
        if (!inspectsPerPuzzle.ContainsKey(_currentPuzzleArea))
            inspectsPerPuzzle.Add(area, 0);
        inspectsPerPuzzle[_currentPuzzleArea]++;
        _totalInspects++;
    }

    public void UpdateInspects()        //updates stats for "inspect" actions both on puzzle and area level
    {
        UpdateInspects(_currentPlayerArea, true, true);
    }

    // == MEASURING TIME ==

    public void UpdateTimePlayer()
    {
        UpdateTime(_currentPlayerArea, (int)_timerPlayer, true);
        _timerPlayer = 0;
    }

    public void UpdateTimePuzzle()
    {
        UpdateTime(_currentPuzzleArea, (int)_timerPuzzle, false);
        _timerPuzzle = 0;
    }

    private void UpdateTime(Area area, int seconds, bool player)        //Area as key, time in seconds and distinction between player and puzzle area; TODO
    {
        if (player)
        {
            if (!timePerArea.ContainsKey(area))
                timePerArea.Add(area, seconds);
            else
                timePerArea[area] += seconds;
        }
        else
        {
            if (!timePerPuzzle.ContainsKey(area))
                timePerPuzzle.Add(area, seconds);
            else
                timePerPuzzle[area] += seconds;
        }
    }

    public void PauseTimer()            //used when the game is paused
    {
        _timerOn = false;
        UpdateTimePlayer();
        UpdateTimePuzzle();
    }

    public void ResumeTimer()           //used when the game is resumed
    {
        _timerOn = true;
        _timeSinceClick = 0;
    }

    public void TriggerPause()          //used to trigger pause menu after inactivity; TODO
    {
        PauseTimer();
        pauseMenu.Pause();
    }

    public void ResetPuzzleTimer()      //to be used when starting a new puzzle
    {
        _timerPuzzle = 0;
    }

    public void ChangePlayerTimer()     //to be used when changing areas
    {
        UpdateTimePlayer();
        _timerPlayer = 0;
    }

    public void ResetActivityTimer()    //to be used after any activity
    {
        _timeSinceClick = 0;
    }

    public void TestTimers()
    {
        timerText.text = "Puzzle Area: " + _currentPuzzleArea.name + " Player Area:" + _currentPlayerArea.name + "\nTotal: " + _timerTotal + "\nArea: " + _timerPlayer + "\nInactivity: " + _timeSinceClick;
    }

    
    // == AREA STATISTICS ==

    public void AddArea(Area area)      //when a game area has been entered for the first time
    {
        if(!_areasDiscovered.Contains(area))
            _areasDiscovered.Add(area);
        if (!inspectsPerArea.ContainsKey(area))
            inspectsPerArea.Add(area, 0);
        if (!actionsPerArea.ContainsKey(area))
            actionsPerArea.Add(area, 0);
        Debug.Log("Area " + area.name + " has been discovered. ");
    }

    public void AddPuzzle(Area area)    //when a new puzzle/task has been started
    {
        if (!_puzzlesDiscovered.Contains(area))
            _puzzlesDiscovered.Add(area);
        if (!inspectsPerPuzzle.ContainsKey(area))
            inspectsPerPuzzle.Add(area, 0);
        if (!actionsPerPuzzle.ContainsKey(area))
            actionsPerPuzzle.Add(area, 0);
        Debug.Log("A puzzle has been discovered in the " + area.name);
    }

    public void FinishPuzzle(Area area)
    {
        if (!_puzzlesFinished.Contains(area))
            _puzzlesFinished.Add(area);
    }

    public void SetNumberOfAreas(int areas) //setting the total number of available ares/puzzles, called in PuzzleManager on loading areas
    {
        _totalAreas = areas;
    }

    public int GetNumberOfAreas()
    {
        return _totalAreas;
    }

    public List<Area> GetAreasDiscovered()  //returns list of all areas that have been discovered
    {
        return _areasDiscovered;
    }

    public List<Area> GetPuzzlesDiscovered()
    {
        return _puzzlesDiscovered;
    }
    
    public void SetCurrentPuzzleArea(Area area) //sets the area with which the current puzzle is associated; assumption: one puzzle at a time
    {
        AddPuzzle(area);
        _currentPuzzleArea = area;
    }

    /*public void SetCurrentPuzzleArea(List<Area> areas)  //sets the area(s) associated with the current puzzle(s); assumption: 1+ puzzles at a time
    {
        foreach (Area area in areas)
        {
            AddPuzzle(area);
            _currentPuzzleArea.Add(area);
        }
    }*/

    public void SetCurrentPlayerArea(Area area) //sets the area where the player is currently located
    {
        AddArea(area);
        _currentPlayerArea = area;
    }

    public bool IsDiscovered(Area area) //has area been discovered?
    {
        return _areasDiscovered.Contains(area);
    }

    public bool IsCurrentArea(Area area)    //player is currently in this area
    {
        return area == _currentPlayerArea;
    }

    public bool IsFinished(Area area)    //has puzzle been finished?
    {
        return _puzzlesFinished.Contains(area); 
    }

    public bool IsCurrentPuzzle(Area area)
    {
        return _currentPuzzleArea == area;
    }

    // == GETTING STATISTICS ==

    public Dictionary<string, int> GetAllStats(Area area)
    {
        Dictionary<string, int> fullStats = new Dictionary<string, int>();
        fullStats.Add("Actions", actionsPerArea[area]);
        fullStats.Add("Inspections", inspectsPerArea[area]);
        fullStats.Add("Time", timePerArea[area]);                   
        return fullStats;
    }

    public Dictionary<string, Dictionary<string, int>> GetAllStats()
    {
        Dictionary<string, Dictionary<string, int>> fullStats = new Dictionary<string, Dictionary<string, int>>();
        foreach (Area area in _areasDiscovered)
            fullStats.Add(area.name, GetAllStats(area));
        return fullStats;
    }

    public string GetAllStatsAsString(Area area, bool puzzle)
    {
        return GetAllStatsAsString(area, puzzle, true);
    }

    public string GetAllStatsAsString(Area area, bool puzzle, bool full)   //area: puzzle/player area depending on value of "puzzle"; full = with or without name
    {
        string stats = "";
        if (full)
            stats += area.name;
        if (puzzle)
        {
            if (full)
            {
                stats += " Puzzle";
                if (_puzzlesFinished.Contains(area))
                    stats += " * COMPLETED *";
                if (_currentPuzzleArea == area)
                    stats += " * CURRENT *";
            }
            stats += "\nActions: " + actionsPerPuzzle[area] + "\nInspections: " + inspectsPerPuzzle[area] + "\nTime: " + TimeToString(timePerPuzzle[area]);            
        }
        else
        {
            if (_currentPlayerArea == area  && full)
                stats += " * YOU ARE HERE *";
            stats += "\nActions:" + actionsPerArea[area] + "\nInspections:" + inspectsPerArea[area] + "\nTime: " + TimeToString(timePerArea[area]);
        }
        return stats;
    }

    public string GetAllStatsAsString()
    {
        StringBuilder stats = new StringBuilder();
        //string allStats = "";
        stats.AppendLine("Areas Discovered: " + _areasDiscovered.Count + "/" + _totalAreas);
        foreach (Area area in _areasDiscovered)
            //allStats += GetAllStatsAsString(area, false) + "\n";
            stats.AppendLine(GetAllStatsAsString(area, false));
        stats.AppendLine("".PadLeft(30, '*'));
        stats.AppendLine("Puzzles Finished: " + _puzzlesFinished.Count + "/" + _totalAreas);    //assumption #puzzles = #areas
        foreach (Area area in _puzzlesDiscovered)
            //allStats += GetAllStatsAsString(area, true) + "\n";
            stats.AppendLine(GetAllStatsAsString(area, true));
        //return allStats;
        stats.AppendLine("".PadLeft(30, '*'));
        stats.AppendLine("Total\nActions: " + _totalActions + "\nInspections: " + _totalInspects + "\nTime: " + _timerTotal);
        return stats.ToString();
    }

    public new string ToString()
    {
        return GetAllStatsAsString();
    }

    public string GetBasicStatsAsString()
    {
        StringBuilder stats = new StringBuilder();
        stats.AppendLine("Areas Discovered: " + _areasDiscovered.Count + "/" + _totalAreas);
        stats.AppendLine("Puzzles Finished: " + _puzzlesFinished.Count + "/" + _totalAreas);    //assumption #puzzles = #areas
        stats.AppendLine("Total Actions: " + _totalActions + "\nTotal Inspections: " + _totalInspects 
            + "\nTotal Time: " + TimeToString((int)_timerTotal) + " (m:s)");
        return stats.ToString();
    }

    public string TimeToString(int time)
    {
        int seconds = time % 60;
        int minutes = time / 60;
        string timeString = "";
        if (minutes < 10)
            timeString += "0";
        timeString += minutes + ":";
        if (seconds < 10)
            timeString += "0";
        return timeString += seconds;
    }

}
