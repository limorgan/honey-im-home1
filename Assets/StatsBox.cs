using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsBox : MonoBehaviour
{
    public Text statsText;
    public Text header;
    public Text status;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void DisplayBoxArea(Area area)
    {
        if (Statistics.Instance.IsDiscovered(area))
        {
            if (Statistics.Instance.IsCurrentArea(area))
                status.text = "* YOU ARE HERE *";
            else
                status.text = "";
            header.text = area.name;
            statsText.text = Statistics.Instance.GetAllStatsAsString(area, false, false);
        }
        else
            DisplayBoxUndiscovered(false);
    }

    public void DisplayBoxPuzzle(Area area, int number)
    {
        header.text = "Puzzle #" + number;
        if (Statistics.Instance.IsCurrentPuzzle(area))
            status.text = "* CURRENT *";
        else if (Statistics.Instance.IsFinished(area))
            status.text = "* COMPLETED *";
        else
        {
            DisplayBoxUndiscovered(true);
            return;
        }
        statsText.text = Statistics.Instance.GetAllStatsAsString(area, true, false);
    }

    public void DisplayBoxUndiscovered(bool puzzle)
    {
        if (puzzle)
            status.text = "* NOT YET STARTED *";
        else
        {
            status.text = "* NOT YET DISCOVERED *";
            header.text = "???";
        }
        statsText.text = "";
    }

    public void DisplayBoxUndiscovered(bool puzzle, int index)
    {
        if (puzzle)
            header.text = "Puzzle #" + index;
        DisplayBoxUndiscovered(puzzle);
    }
}
