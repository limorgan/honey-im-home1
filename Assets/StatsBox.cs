using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsBox : MonoBehaviour
{
    private GameItem _gameItem;
    [SerializeField]
    public Text textTemplate;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static StatsBox CreateComponent(GameObject where, Area area, Dictionary<string, int> stats)
    {
        StatsBox statsBox = where.AddComponent<StatsBox>();
        string statsText = area.name;
        foreach (KeyValuePair<string, int> stat in stats)
        {
            statsText += "\n" + stat.Key + ":\t" + stat.Value;
        }
        where.GetComponentInChildren<Text>().text = statsText;
        return statsBox;
    }
}
