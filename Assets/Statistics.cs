using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Statistics : MonoBehaviour
{
    private static Statistics _instance;
    public static Statistics Instance { get { return _instance; } }

    private List<Area> _areasDiscovered = new List<Area>();
    private Area _currentPuzzleArea;
    private Area _currentPlayerArea;
    private int _currentPuzzle;

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

        

        TestTimers();               //testing timers
        if (_timeSinceClick >= timeOut)
            TriggerPause();
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

    public void UpdateTimePlayer()
    {
        UpdateTime(_currentPlayerArea, (int)_timerPlayer, true);
    }

    public void UpdateTimePuzzle()
    {
        UpdateTime(_currentPuzzleArea, (int)_timerPuzzle, false);
    }

    private void UpdateTime(Area area, int seconds, bool player)        //Area as key, time in seconds and distinction between player and puzzle area; TODO
    {
        if (!timePerArea.ContainsKey(area))
            timePerArea.Add(area, seconds);
        else
            timePerArea[area] += seconds;
    }

    public Dictionary<string, int> GetAllStats(Area area)
    {
        Dictionary<string, int> fullStats = new Dictionary<string, int>();
        fullStats.Add("Actions", actionsPerArea[area]);
        fullStats.Add("Inspections", inspectsPerArea[area]);
        fullStats.Add("Time", timePerArea[area]);
        return fullStats;
    }

    public void AddArea(Area area)
    {
        if(!_areasDiscovered.Contains(area))
            _areasDiscovered.Add(area);
    }

    public List<Area> GetAreasDiscovered()
    {
        return _areasDiscovered;
    }
    
    public void SetCurrentPuzzleArea(Area area)
    {
        _currentPuzzleArea = area;
    }

    public void SetCurrentPlayerArea(Area area)
    {
        _currentPlayerArea = area;
    }

    public void PauseTimer()            //used when the game is paused
    {
        _timerOn = false;
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
        timerText.text = "Total: " + _timerTotal + "\nArea: " + _timerPlayer + "\nInactivity: " + _timeSinceClick;
    }

    /*public void UpdateArea(Area area, bool player, bool puzzle)
    {
        if (player)
            _currentPlayerArea = area;
        if (puzzle)
            _currentPuzzleArea = area;
    }*/
}
