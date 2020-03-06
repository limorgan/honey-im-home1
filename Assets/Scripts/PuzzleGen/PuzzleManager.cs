using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PuzzleManager : MonoBehaviour {

    public Area startArea;
    public bool useAllRules;
    [SerializeField]
    private Dictionary<Area, List<Rule>> _leaves = new Dictionary<Area, List<Rule>>();
    private Dictionary<Area, List<Rule>> _puzzleRules = new Dictionary<Area, List<Rule>>();
    private Area _currentArea;
    [SerializeField]
    private List<Rule> _gameOverRules = new List<Rule>();
    private List<Area> _accessibleAreas = new List<Area>();

    private static PuzzleManager _instance;
    public static PuzzleManager Instance { get { return _instance; } }

    void Awake() {
        if (_instance != null & _instance != this)
            Destroy(this.gameObject);
        else {
            _instance = this;
            Debug.Log("PuzzleManager instance created. ");
        }
    }

    void Start() {
        //_gameOverRules = RuleDatabase.GetRulesWithOutput(new Term("Player"));
        GenerateForArea(startArea);
    }

    public void GenerateForArea(Area area) {
        area.areaObject.SetActive(true);
        Debug.Log("Area: " + area.ToString());
        Rule root = Generator.GeneratePuzzleStartingFrom(area, _accessibleAreas);
        Debug.Log("Root rule: " + root.GetRuleAsString());
        _leaves.Add(area, new List<Rule>());
        _puzzleRules.Add(area, new List<Rule>());
        FindLeaves(root, area);
        _currentArea = area;    // 05/03 updating otherwise not used private variable _currentArea
        //Debug.Log("Rule generated: " + root.GetRuleAsString());
    }

    public List<Rule> RulesFor(GameItem gameItem, Area area) {      
        List<Rule> rules = new List<Rule>();
        List<Rule> test_rules = _leaves[area];
        foreach(Rule rule in _leaves[area]) {
            addApplicableRule(rule, gameItem, rules);
            /*if (rule.reversible) {
                foreach (Rule modRule in rule.InputPermutation())
                    addApplicableRule(modRule, gameItem, rules);
            }*/
            /*foreach(Rule gameOver in _gameOverRules) {
                if(!rules.Contains(rule))
                    rules.Add(gameOver);
            }*/
        }
        if (useAllRules) {
            List<Rule> dbRules = RuleDatabase.GetAllRulesWithInput(gameItem.dbItem);
            for (int i = dbRules.Count - 1; i >= 0; i--) {
                dbRules[i].inputs[0].gameItem = gameItem;               
                if (FindItemsForOutputs(dbRules[i]) && !rules.Contains(dbRules[i])) {
                    rules.Add(dbRules[i]);
                }
            }            
        }
        return rules;
    }

    private void addApplicableRule(Rule rule, GameItem gameItem, List<Rule> rules)
    {
        if (rule.inputs[0].dbItem != null)
        {
            if (rule.inputs[0].dbItem.name == gameItem.dbItem.name)
            {
                if (!rules.Contains(rule))
                {
                    Debug.Log("Found: " + gameItem.dbItem.name + " " + rule.action);
                    rule.inputs[0].gameItem = gameItem;
                    rules.Add(rule);
                }
            }
        }
    }

    public void ExecuteRule(Rule rule, Area area) {
        /*Debug.Log("Started to execute rule of " + rule.GetRuleAsString() + " in" + area.toString());
        foreach (Rule r in _leaves[area])
        {
            Debug.Log("Rule in " + area.name + " : " + r.GetRuleAsString());
        }
        Debug.Log("Done with rules in _leaves. ");*/
        if (_leaves[area].Contains(rule)) {
            Debug.Log("Execute: " + rule.parent.outputs[0].name);
            _leaves[area].Remove(rule);
            if (rule.parent.parent != null)
                _leaves[area].Add(rule.parent);
            else {
                Debug.Log("Finished this area!");
                Player.Instance.showFinishMessage(_currentArea.name);
                if (area.isFinal())
                    TriggerEnd();
                else
                {
                    foreach (Area connectedArea in area.connectedTo)
                    {
                        GenerateForArea(connectedArea);
                    }
                }
            }
        }
    }

    private void FindLeaves(Rule parent, Area area) {
        Debug.Log("Checking children for rule " + parent.GetRuleAsString());
        if (parent.children.Count == 0) {
            Debug.Log("Rule " + parent.toString() + " has no children. ");
            _leaves[area].Add(parent);
        } else {
            if (!_puzzleRules[area].Contains(parent)) {
                _puzzleRules[area].Add(parent);
            }
            foreach(Rule child in parent.children) {
                FindLeaves(child, area);
            }
        }
    }

    private bool FindItemsForOutputs(Rule rule) {
        foreach (Term output in rule.outputs) {
            bool found = false;
            foreach (Term input in rule.inputs) {
                if (output.name == input.name)
                    found = true;
            }
            if (!found) {
                List<Item> possibleItems = ItemDatabase.FindDBItemsFor(output, new List<Area>(), new List<Item>());
                if (possibleItems.Count > 0)
                    output.dbItem = possibleItems[Random.Range(0, possibleItems.Count)];
                else
                    return false;
            }
        }
        return true;
    }

    public string getHint()
    {
        return _currentArea.getHint();
    }

    public string getCurrentAreaName()
    {
        return _currentArea.name;
    }

    public void TriggerEnd()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
