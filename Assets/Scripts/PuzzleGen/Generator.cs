﻿using UnityEngine;
using System.Collections.Generic;

public class Generator : MonoBehaviour {
    public static bool debugMode = true;
    static private Dictionary<Area, Rule> _puzzlesPerArea = new Dictionary<Area, Rule>();

    private static Generator _instance;
    public static Generator Instance { get { return _instance; } }

    void Awake() {
        if (_instance != null & _instance != this)
            Destroy(this.gameObject);
        else { 
            _instance = this;
            Debug.Log("Generator instance created. ");
        }
    }

    static public void Spawn(Item item, Rule rule, Area area) {
        Debug.Log("Spawning: item - " + item.name + " in " + area.name + "x" + GameObject.Find(area.name));
        GameArea gameArea = GameObject.Find(area.name).GetComponent<GameArea>();
        bool found = false;
        for(int i = 0; i < gameArea.itemsInArea.Length; i++) {
            Debug.Log("item: " + gameArea.itemsInArea[i].name);
            if (gameArea.itemsInArea[i].name == item.name) {
                gameArea.itemsInArea[i].Setup(item.name, item);
                found = true;
                gameArea.itemsInArea[i].name = "";
            }
        }
        if (!found) {
            // 04/03 specific spawn points for NPCs?
            Vector3 nextSpawnPoint = new Vector3(0,0,0);
            if (item.specificSpawnPoints)
            {
                nextSpawnPoint = item.getNextSpawnPt();
            }
            else if (item.GetPropertyWithName("isa") != null && item.GetPropertyWithName("isa").value == "NPC")
            {
                nextSpawnPoint = gameArea.getNextSpawnPt(true);
            }
            else
                nextSpawnPoint = gameArea.getNextSpawnPt();
            GameObject itemGO = (GameObject)Instantiate(item.itemPrefab,
                nextSpawnPoint, Quaternion.identity);            //gameArea.getNextSpawnPt() replaced by nextSpawnPoint
            itemGO.transform.SetParent(gameArea.gameObject.transform);
            itemGO.GetComponent<GameItem>().Setup(item.name, item);
        }
    }

    static public Rule GeneratePuzzleStartingFrom(Area area, List<Area> accessibleAreas) {
        Rule root = new Rule();
        area.setFinal(false);
        //Find all existing items in the scene
        List<Item> itemsInTheScene = new List<Item>();
        GameItem[] existingGameItems = GameObject.Find(area.name).GetComponentsInChildren<GameItem>();
        for(int i = 0; i < existingGameItems.Length; i++) {
            itemsInTheScene.Add(existingGameItems[i].dbItem);
        }
        Debug.Log("number of items in inventory: " + Player.Instance.GetInventory().Count);
        foreach(GameItem item in Player.Instance.GetInventory()) {
            Debug.Log("item from inventory to existing game items: " + item.name);
            itemsInTheScene.Add(item.dbItem);
        }
        //Pick a possible goal for the area
        if (area.goals.Count > 0) {
            Term goal = area.goals[Random.Range(0, area.goals.Count)]; // Random.Range(0, area.goal.Count)];
            Debug.Log("area goal: " + goal.name);
            if (GenerateInputs(goal, root, 0, area, accessibleAreas, itemsInTheScene))
            {
                area.setCurrentGoal(goal);
                if (goal.GetPropertyWithName("gameover") != null && goal.GetPropertyWithName("gameover").value == "True")   //setting area to be final
                    area.setFinal(true);
                Debug.Log("SUCCESS");
            }
            else
                Debug.Log("FAILURE");
        } else {
            if(debugMode) Debug.Log("No goals associated with this area");
        }
        return root;
    }

	// Recursively generate inputs
	static bool GenerateInputs(Term startTerm, Rule parentRule, int depth, Area currentArea, List<Area> accessibleAreas, List<Item> itemsInTheScene) {

        List<Item> matchingItems = ItemDatabase.FindDBItemsFor(startTerm, accessibleAreas, itemsInTheScene);

        //Check if term of this type exists in DB before continuing
        if (matchingItems.Count == 0) {
            if (!ItemDatabase.HasItemOfType(startTerm, accessibleAreas, itemsInTheScene)) {
                Debug.Log("GRAMMAR ERROR: Couldn't find accessible item of type: " + startTerm.name);
                return false;
            }
        } 
        else if(startTerm.dbItem == null) {
            // Pick a random item to fit the term
            startTerm.dbItem = matchingItems[Random.Range(0, matchingItems.Count)];
            if (itemsInTheScene.Contains(startTerm.dbItem)) {
                return true;
            }
        }

        // Find rule with startTerm as output, output could be super-type of startTerm
        List<Rule> possibleRules = new List<Rule>();
        foreach (Rule rule in RuleDatabase.GetAllObjects()) {
            if (rule.MainOutputIs(startTerm)) {
                if(debugMode && startTerm.dbItem != null) Debug.Log("Found matching rule " + rule.outputs[0].name +
                    " with output dbItem: " + startTerm.dbItem.name + " at depth: " + depth);
                else if (debugMode) Debug.Log("Found matching rule with output: " + startTerm.name);
                possibleRules.Add(rule);
            } 
        }

        if(debugMode)
            Debug.Log("number of possible rules: " + possibleRules.Count);

        // Pick a rule
        if (possibleRules.Count > 0 && depth < currentArea.maxDepth) {
            Rule chosenRule = possibleRules[Random.Range(0, possibleRules.Count)];
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
        if (startTerm.dbItem == null) {
            Debug.Log("GRAMMAR ERROR: No terminal or non-terminal match for term: " + startTerm.name);
            return false;
        }
        Spawn(startTerm.dbItem, parentRule, currentArea);
        if (debugMode) Debug.Log("DB item added to spawn list: " + startTerm.dbItem.name);
        return true;
    }
}
