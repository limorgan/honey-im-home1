using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
    [SerializeField]
    private List<ConditionalObject> _conditionalObjects = new List<ConditionalObject>();

    [SerializeField]
    public GameObject finalFade;
    [SerializeField]
    public AudioSource gameMusic;
    [SerializeField]
    public GameObject everything;
    [SerializeField]
    public GameObject generator;
    [SerializeField]
    public GameObject player;
    [SerializeField]
    public GameObject startingInventory;

    /*[SerializeField]
    public List<Item> _itemAssets = new List<Item>();
    [SerializeField]
    public List<Rule> _ruleAssets = new List<Rule>();
    [SerializeField]
    public List<Area> _areaAssets = new List<Area>();*/
    public Item[] _itemAssets;
    public Rule[] _ruleAssets;
    public Area[] _areaAssets;

    private static PuzzleManager _instance;
    public static PuzzleManager Instance { get { return _instance; } }

    void Awake() {
        if (_instance != null && _instance != this)
        {
            Debug.Log("Destroying PuzzleManager. ");
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            Debug.Log("PuzzleManager instance created. ");

            _areaAssets = Resources.LoadAll<Area>("Areas");
            Debug.Log("Areas loaded. ");
            _itemAssets = Resources.LoadAll<Item>("DBItems");
            Debug.Log("items loaded.");
            _ruleAssets = Resources.LoadAll<Rule>("Rules");

            everything.SetActive(true);
            player.SetActive(true);
            startingInventory.SetActive(true);
            generator.SetActive(true);
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

        foreach (ConditionalObject CO in _conditionalObjects)
        {
            if (CO.area.name == area.name)
            {
                foreach (Rule rule in _puzzleRules[area])
                {
                    if (PuzzleContains(CO.condition, rule))
                    {
                        CO.affectedItem.SetActive(true);
                    }
                }
            }
        }

        //Debug.Log("All Rules: \n" + GetFullPuzzle());
    }

    public List<Rule> RulesFor(GameItem gameItem, Area area) {      
        List<Rule> rules = new List<Rule>();
        //Debug.Log("rules for game item " + gameItem.name + " in " + area.name);
        foreach(Rule rule in _leaves[area]) {
            addApplicableRule(rule, gameItem, rules);
            /*foreach(Rule gameOver in _gameOverRules) {
                if(!rules.Contains(rule))
                    rules.Add(gameOver);
            }*/
        }
        if (useAllRules) {
            List<Rule> dbRules = GetAllRulesWithInput(gameItem.dbItem);
            for (int i = dbRules.Count - 1; i >= 0; i--) {
                dbRules[i].inputs[0].gameItem = gameItem;               
                if (FindItemsForOutputs(dbRules[i]) && !rules.Contains(dbRules[i])) {
                    rules.Add(dbRules[i]);
                }
            }            
        }
        foreach (Rule r in rules)
            Debug.Log("Rule: " + r.ToString() + " for " + gameItem.name);
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
            Debug.Log("Rule " + parent.ToString() + " has no children. ");
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
                List<Item> possibleItems = FindDBItemsFor(output, new List<Area>(), new List<Item>());
                if (possibleItems.Count > 0)
                    output.dbItem = possibleItems[Random.Range(0, possibleItems.Count)];
                else
                    return false;
            }
        }
        return true;
    }

    

    // == PREVIOUSLY ITEM DATABASE METHODS ==

    public Item GetObject(string itemName)
    {
        //ValidateDatabase();
        foreach (Item item in _itemAssets)
        {
            //Debug.Log("Checking for " + itemName + " DBItem name: " + item.name);
            if (item.name == itemName)
                return ScriptableObject.Instantiate(item) as Item;
        }
        return null;
    }

    public bool HasItemOfType(Term term, List<Area> accessibleAreas, List<Item> itemsInScene)
    {
        foreach (Item dbItem in _itemAssets)
        {
            if (dbItem.GetSuperTypes().Contains("Present"))
                Debug.Log(dbItem.name + " is a Bartender, in scene: " + dbItem.IsAccessible(accessibleAreas, itemsInScene));
            if ((dbItem.name == term.name || dbItem.GetSuperTypes().Contains(term.name)) && dbItem.IsAccessible(accessibleAreas, itemsInScene))
                return true;
        }
        return false;
    }

    public List<Item> GetItemsOfType(string itemName, List<Area> accessibleAreas, List<Item> itemsInScene)
    {
        List<Item> matchingItems = new List<Item>();
        foreach (Item dbItem in _itemAssets)
        {
            if (dbItem.name == itemName || dbItem.GetSuperTypes().Contains(itemName))
            {
                matchingItems.Add(dbItem);
            }
        }
        return matchingItems;
    }

    public List<Item> FindDBItemsFor(Term term, List<Area> accessibleAreas, List<Item> itemsInScene)
    {
        List<Item> matchingItems = new List<Item>();
        foreach (Item dbItem in _itemAssets)
        {
            if (dbItem.Matches(term) && dbItem.IsAccessible(accessibleAreas, itemsInScene))
            {
                matchingItems.Add(ScriptableObject.Instantiate(dbItem) as Item);
            }
        }
        return matchingItems;
    }

    // == PREVIOUSLY RULE DATABASE METHODS == 

    public List<Rule> GetAllRulesWithInput(Item dbItem)
    {
        List<Rule> rules = new List<Rule>();
        //Rule[] assets = Resources.LoadAll<Rule>("Assets/Resources/Rules");
        for (int i = 0; i < _ruleAssets.Length; i++)
        {
            if ((_ruleAssets[i].inputs[0].name == dbItem.name ||
                dbItem.GetSuperTypes().Contains(_ruleAssets[i].inputs[0].name)))
            {
                Rule ruleToAdd = ScriptableObject.Instantiate(_ruleAssets[i]) as Rule;
                rules.Add(ruleToAdd);
            }
        }
        return rules;
    }

    public List<Rule> GetRulesWithOutput(Term term)
    {
        List<Rule> rules = new List<Rule>();
        //Rule[] assets = Resources.LoadAll<Rule>("Assets/Resources/Rules");
        Debug.Log("GetRulesWithOutput - term: " + term.GetTermAsString());
        for (int i = 0; i < _ruleAssets.Length; i++)
        {
            if (_ruleAssets[i].outputs[0].name == term.name)
            {
                Rule ruleToAdd = ScriptableObject.Instantiate(_ruleAssets[i]) as Rule;
                rules.Add(ruleToAdd);
            }
        }
        /*foreach (Rule r in rules)
        {
            Debug.Log("term: " + term.GetTermAsString() + " rules: " + r.GetRuleAsString());
        }*/
        return rules;
    }

    //== PREVIOUSLY IN DATABASE ==

    public List<Item> GetAllItems()
    {
        //LoadDatabase();
        List<Item> objects = new List<Item>();
        foreach (Item asset in _itemAssets)
        {
            objects.Add(ScriptableObject.Instantiate(asset) as Item);
        }
        return objects;
    }

    public List<Rule> GetAllRules()
    {
        //LoadDatabase();
        List<Rule> objects = new List<Rule>();
        foreach (Rule asset in _ruleAssets)
        {
            objects.Add(ScriptableObject.Instantiate(asset) as Rule);
        }
        return objects;
    }

    public List<Area> GetAllAreas()
    {
        //LoadDatabase();
        List<Area> objects = new List<Area>();
        foreach (Area asset in _areaAssets)
        {
            objects.Add(ScriptableObject.Instantiate(asset) as Area);
        }
        return objects;
    }


    //== ADDITIONAL FUNCTIONALITIES ==

    public void UpdatePlayerProperties(Property property)
    {
        if (player.GetComponent<GameItem>().GetProperty(property.name) != null)
            player.GetComponent<GameItem>().GetProperty(property.name).value = property.value;
        else
            player.GetComponent<GameItem>().properties.Add(property);
    }

    public GameItem GetPlayer()
    {
        return player.GetComponent<GameItem>();
    }

    public bool PuzzleContains(Item item, Rule parent)
    {
        if (parent.ContainsItem(item))
            return true;
        if (parent.children.Count == 0)
        {
            Debug.Log("Rule " + parent.ToString() + " has no children. ");
            return false;
        }
        else
        {
            foreach (Rule child in parent.children)
            {
                if (PuzzleContains(item, child))
                    return true;
            }
        }
        return false;
    }

    public string getHint()
    {
        return _currentArea.getHint();
    }

    public string GetCurrentAreaName()
    {
        return _currentArea.name;
    }

    public Area GetCurrentArea()
    {
        return _currentArea;
    }

    public void TriggerEnd()
    {
        //Player.Instance.gameObject.SetActive(false);
        finalFade.SetActive(true);
    }
}
