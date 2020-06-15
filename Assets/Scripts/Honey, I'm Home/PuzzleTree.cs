using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleTree : MonoBehaviour
{
    // Script to generate all possible puzzle trees from given databases
    private string _path = "Assets/TestingResults/puzzlefull" + System.DateTime.Now.ToLongDateString() + ".txt";
    private string _pathChosen = "Assets/TestingResults/puzzlechosen" + System.DateTime.Now.ToLongDateString() + ".txt";

    private static PuzzleTree _instance;
    public static PuzzleTree Instance { get { return _instance; } }

    // Start is called before the first frame update
    void Start()
    {
        if (_instance != null && _instance != this)
        {
            Debug.Log("Destroying PuzzleTree. ");
            Destroy(this.gameObject);
        }
        else
            _instance = this;
        GetInfo();
        FullPuzzleTree();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GetInfo()
    {
        string[] overview = { "OVERVIEW " + System.DateTime.Today.ToLongDateString(), "Areas: " + PuzzleManager.Instance._areaAssets.Length,
            "Rules: " + PuzzleManager.Instance._ruleAssets.Length, "Items: " + PuzzleManager.Instance._itemAssets.Length};
        System.IO.File.WriteAllLines(_path, overview);

        /*foreach (Area area in PuzzleManager.Instance._areaAssets)
            GetSubtreeArea(area);*/        
    }

    void GetSubtreeArea(Area area, bool full)
    {
        using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(GetPath(full), true))
        {
            file.WriteLine("AREA: " + area.name.ToUpper());
            //string subtree = 
        }
    }

    public void WriteAreaTree(Area area, bool full)
    {
        using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(GetPath(full), true))
        {
            file.WriteLine("AREA: " + area.name.ToUpper());
        }
    }
    public void WriteTree(string content, int depth, bool full)
    {
        using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(GetPath(full), true))
        {
            file.WriteLine(Tabs(depth) + content + "\t(depth " + depth + ")");
        }
    }

    public void WriteTree(string content, bool full)
    {
        using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(GetPath(full), true))
        {
            file.WriteLine(content);
        }
    }

    string Tabs(int n)
    {
        return new string('\t', n);
    }

    public void FullPuzzleTree()        //generates tree for all possible puzzles for each area
    {
        foreach (Area area in PuzzleManager.Instance._areaAssets)
        {
            WriteAreaTree(area, true);
            List<Item> itemsInTheScene = new List<Item>();
            GameItem[] existingGameItems = GameObject.Find(area.name).GetComponentsInChildren<GameItem>();
            for (int i = 0; i < existingGameItems.Length; i++)
            {
                Debug.Log("Existing Game Items: " + existingGameItems[i].name);
                itemsInTheScene.Add(existingGameItems[i].dbItem);
            }
            foreach (GameItem item in Player.Instance.GetInventory())
            {
                itemsInTheScene.Add(item.dbItem);
            }
            int goalCount = 0;
            foreach (Term goal in area.goals)
            {
                Rule root = new Rule(); 
                goalCount++;
                GetFullTree(goal, area, goalCount, root, 0, area.connectedTo, itemsInTheScene);
            }
        }
    }

    public void GetFullTree(Term goal, Area area, int goalCount, Rule parentRule, int depth, List<Area> accessibleAreas, List<Item> itemsInTheScene)
    {
        WriteTree("Goal " + goalCount + ": " + goal.ToString(), true);
        GetFullTree(goal, depth++);
    }

    public void GetFullTree(Term startTerm, int depth)
    {
        List<Rule> possibleRules = GetAllRules(startTerm, depth);
        if (possibleRules.Count == 0)
            WriteTree(new string('\t', depth) + startTerm.ToString().ToUpper(), true);
        else
        {
            foreach (Rule rule in possibleRules)
            {
                WriteTree(rule.ToShortString(), true);
                foreach (Term term in rule.inputs)
                    GetFullTree(term, ++depth);
            }
        }
    }

    string GetPath(bool full)
    {
        if (full)
            return _path;
        else
            return _pathChosen;
    }

    List<Rule> GetAllRules(Term startTerm, int depth)
    {
        List<Rule> possibleRules = new List<Rule>();
        foreach (Rule rule in PuzzleManager.Instance.GetAllRules())
        {       //Previously: foreach (Rule rule in RuleDatabase.GetAllObjects()) {
            //Debug.Log("Main Output: " + rule.outputs[0].name + " vs Start Term: " + startTerm.name + " Main Output? " + rule.MainOutputIs(startTerm));
            if (rule.MainOutputIs(startTerm))
            {
                if (startTerm.dbItem != null)
                    PuzzleTree.Instance.WriteTree(rule.ToString(), depth, true);
                else
                {
                    PuzzleTree.Instance.WriteTree(rule.ToString(), depth, true);
                }
                possibleRules.Add(rule);
            }
        }
        return possibleRules;
    }
}
