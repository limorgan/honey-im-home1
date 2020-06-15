using UnityEngine;
using System.Collections.Generic;

public class Generator : MonoBehaviour {
    public static bool debugMode = true;
    static private Dictionary<Area, Rule> _puzzlesPerArea = new Dictionary<Area, Rule>();

    private static Generator _instance;
    public static Generator Instance { get { return _instance; } }

    [SerializeField]
    private List<GameItem> _startingInventory = new List<GameItem>(); 

    void Awake() {
        if (_instance != null ) //& _instance != this)
            Destroy(this.gameObject);
        else { 
            _instance = this;
            Debug.Log("Generator instance created. ");
        }
    }

    static public void Spawn(Item item, Rule rule, Area area) {
        GameArea gameArea = GameObject.Find(area.name).GetComponent<GameArea>();
        bool found = false;
        for(int i = 0; i < gameArea.itemsInArea.Length; i++) {
            if (gameArea.itemsInArea[i].name == item.name) {
                gameArea.itemsInArea[i].Setup(item.name, item);
                found = true;
                gameArea.itemsInArea[i].name = "";
            }
        }
        if (!found) {
            // Addition: specific spawn points for NPCs + specific spawnpoints for an ground-level item 
            Vector3 nextSpawnPoint = new Vector3(0,0,0);
            if (item.GetPropertyWithName("floor") != null && item.GetPropertyWithName("floor").value == "True")
                nextSpawnPoint = gameArea.GetNextSpawnPt(false, true);
            else if (item.GetPropertyWithName("isa") != null && item.GetPropertyWithName("isa").value == "NPC")
            {
                nextSpawnPoint = gameArea.GetNextSpawnPt(true, false);
            }
            else
                nextSpawnPoint = gameArea.GetNextSpawnPt();
            Debug.Log("Spawn pt: " + nextSpawnPoint.ToString() + " ( " + item.name + " ) ");
            GameObject itemGO = (GameObject)Instantiate(item.itemPrefab,
                nextSpawnPoint, Quaternion.identity);            
            Debug.Log(itemGO.transform.position);
            itemGO.transform.SetParent(gameArea.gameObject.transform);
            itemGO.GetComponent<GameItem>().Setup(item.name, item);
            Debug.Log("Post set-up: " + itemGO.transform.position);
        }
    }

    static public Rule GeneratePuzzleStartingFrom(Area area, List<Area> accessibleAreas) {
        Rule root = new Rule();
        area.setFinal(false);
        //Find all existing items in the scene
        List<Item> itemsInTheScene = new List<Item>();
        GameItem[] existingGameItems = GameObject.Find(area.name).GetComponentsInChildren<GameItem>();
        for(int i = 0; i < existingGameItems.Length; i++) {
            Debug.Log("Existing Game Items: " + existingGameItems[i].name);
            itemsInTheScene.Add(existingGameItems[i].dbItem);
        }
        foreach(GameItem item in Player.Instance.GetInventory()) {
            itemsInTheScene.Add(item.dbItem);
        }
        //Pick a possible goal for the area
        if (area.goals.Count > 0) {
            Term goal = area.goals[Random.Range(0, area.goals.Count)]; 
            Debug.Log("Area goal: " + goal.name);
            //PuzzleTree.Instance.WriteAreaTree(area, false);
            //PuzzleTree.Instance.WriteTree(goal.ToString(), 0, false);
            bool successfulInputs = GenerateInputs(goal, root, 0, area, accessibleAreas, itemsInTheScene);
            if (successfulInputs)
            {
                area.setCurrentGoal(goal);
                if (goal.GetPropertyWithName("gameover") != null && goal.GetPropertyWithName("gameover").value == "True")   //setting area to be final
                    area.setFinal(true);
                Debug.Log("SUCCESS");
            }
            else {
                Debug.Log("FAILURE");
                root = GeneratePuzzleStartingFrom(area, accessibleAreas);       //restart if failure
            }
              
        } else {
            if(debugMode) Debug.Log("No goals associated with this area");
        }
        return root;
    }

	// Recursively generate inputs
	static bool GenerateInputs(Term startTerm, Rule parentRule, int depth, Area currentArea, List<Area> accessibleAreas, List<Item> itemsInTheScene) {
        List<Item> matchingItems = PuzzleManager.Instance.FindDBItemsFor(startTerm, accessibleAreas, itemsInTheScene);
        //Check if term of this type exists in DB before continuing
        if (matchingItems.Count == 0)
        {
            if (!PuzzleManager.Instance.HasItemOfType(startTerm, accessibleAreas, itemsInTheScene))  // Previously: (!ItemDatabase.HasItemOfType(startTerm, accessibleAreas, itemsInTheScene))
            {
                Debug.Log("GRAMMAR ERROR: Couldn't find accessible item of type: " + startTerm.name);
                return false;
            }
        }
        else if (startTerm.dbItem == null)
        {
            // Pick a random item to fit the term
            startTerm.dbItem = matchingItems[Random.Range(0, matchingItems.Count)];
            if (itemsInTheScene.Contains(startTerm.dbItem))
            {
                return true;
            }
        }   
        
        // Find rule with startTerm as output, output could be super-type of startTerm
        List<Rule> possibleRules = new List<Rule>();
        foreach (Rule rule in PuzzleManager.Instance.GetAllRules())
        {       //Previously: foreach (Rule rule in RuleDatabase.GetAllObjects()) {
            //Debug.Log("Main Output: " + rule.outputs[0].name + " vs Start Term: " + startTerm.name + " Main Output? " + rule.MainOutputIs(startTerm));
            if (rule.MainOutputIs(startTerm)) {
                if (debugMode && startTerm.dbItem != null)
                {
                    Debug.Log("Found matching rule " + rule.outputs[0].name +
                    " with output dbItem: " + startTerm.dbItem.name + " at depth: " + depth);
                    //PuzzleTree.Instance.WriteTree(rule.ToString(), depth, false);
                }
                else if (debugMode)
                {
                    Debug.Log("Found matching rule with output: " + startTerm.name);
                    //PuzzleTree.Instance.WriteTree(rule.ToString(), depth, false);
                }
                possibleRules.Add(rule);
            } 
        }

        if(debugMode)
            Debug.Log("number of possible rules: " + possibleRules.Count);

        // Pick a rule
        if (possibleRules.Count > 0 && depth < currentArea.maxDepth) {
            Rule chosenRule = possibleRules[Random.Range(0, possibleRules.Count)];
            //PuzzleTree.Instance.WriteTree(chosenRule.ToString(), depth, false);
            chosenRule.outputs[0].dbItem = startTerm.dbItem;
            chosenRule.parent = parentRule;
            parentRule.AddChildRule(chosenRule);
            for (int i = 1; i < chosenRule.outputs.Count; i++) {
                bool found = false;
                for (int j = 0; j < chosenRule.inputs.Count; j++) {
                    if (chosenRule.inputs[j].name == chosenRule.outputs[i].name) {
                        found = true;
                    }
                }
                if (!found) {
                    //List<DBItem> sideEffectItem = ItemDatabase.FindDBItemsFor(chosenRule.outputs[i], accessibleAreas, itemsInTheScene);
                    //chosenRule.outputs[i].dbItem = sideEffectItem[Random.Range(0, sideEffectItem.Count)];
                }
            }
            bool result = true;
            for (int i = 0; i < chosenRule.inputs.Count; i++) {
                //Check if input is same type as the start term, otherwise make the input as specific as start term
                if (chosenRule.outputs[0].name == chosenRule.inputs[i].name) {
                    if (startTerm.dbItem != null) {
                        chosenRule.inputs[i].dbItem = startTerm.dbItem;
                    } else if(startTerm.name != chosenRule.inputs[i].name) {
                        if(startTerm.GetSuperTypes().Contains(chosenRule.inputs[i].name))
                            chosenRule.inputs[i].name = startTerm.name;
                    }
                }
                result = GenerateInputs(chosenRule.inputs[i], chosenRule, depth + 1, currentArea, accessibleAreas, itemsInTheScene);
                if (chosenRule.outputs[0].name == chosenRule.inputs[i].name) {
                    startTerm.dbItem = chosenRule.inputs[i].dbItem;
                }
            }
            return result;
        }

        if(debugMode) Debug.Log("No suitable rule found for: " + startTerm.name + " at depth " + depth);
        //Find DB item for input & add to spawn list
        if (startTerm.dbItem == null && startTerm.name != "Player") {
            Debug.Log("GRAMMAR ERROR: No terminal or non-terminal match for term: " + startTerm.name);
            return false;
        }
        Spawn(startTerm.dbItem, parentRule, currentArea);
        if (debugMode) Debug.Log("DB item added to spawn list: " + startTerm.dbItem.name);
        return true;
    }
}
