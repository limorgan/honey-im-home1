using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameItem : MonoBehaviour {

    public new string name;
    [SerializeField]
    public Item dbItem;
    public List<Property> properties;
    public GameItem containedValue;

    private bool _pickedUp = false;

    void Awake() {
        if (dbItem == null) {
            dbItem = ItemDatabase.GetObject(name);
            Debug.Log("awake: " + name + " result dbtem: " + dbItem);
        }
        if(dbItem != null)
            properties = dbItem.properties;
    }

    public void Setup(string name, Item dbItem) {
        this.name = name;
        this.dbItem = dbItem;
        properties = this.dbItem.properties;
    }



    public void OnGameItemMouseOver(Text UITextRef)
    {
        //Modification 10/01/2020
        
        //Debug.Log(dbItem.name + " located at x = " + location.x + " y = " + location.y);
        
        //End mod
        UITextRef.text = dbItem.description;
    }

    // Addition 10/01/2020
    public GameItem copy(GameItem original)
    {
        GameItem c = new GameItem();
        c.Setup(original.name, original.dbItem);
        c.Awake();
        //Debug.Log("copy name: " + c.name + " dbItem: " + c.dbItem.name);
        return c;
    }

    //Addition 10/01/2020
    private void Spawn(GameItem item)
    {
        Instantiate(item.dbItem.itemPrefab, transform.position, transform.rotation); // trans.pos + + new Vector3(0, 1, 0)...
        Debug.Log("Spawning: " + item.dbItem);
    }

    public void OnGameItemClicked(GameObject actionMenu, GameObject buttonPrefab, Text ActionHeader) {
        //Debug.Log("on game item clicked ");
        ActionHeader.text = name;
        foreach (Rule puzzleRule in PuzzleManager.Instance.RulesFor(this, GetComponentInParent<GameArea>().area)) {
            //Debug.Log("rules...");
            if (RuleFulFilled(puzzleRule)) {
                GameObject action = GameObject.Instantiate(buttonPrefab);
                ActionBtn.CreateComponent(action, this, puzzleRule);
                action.transform.SetParent(actionMenu.transform);
            }
        }
        //Debug.Log("past foreach in gameitem");
        if (dbItem.IsCarryable()) {
            Debug.Log("Creating Pick up button. ");
            GameObject action = GameObject.Instantiate(buttonPrefab);
            ActionBtn.CreateComponent(action, this, new Rule("PickUp"));
            action.transform.SetParent(actionMenu.transform);
        }
        //Addition: 10/01/2020
        /*if(dbItem.IsCopyable())
        {
            Debug.Log("Creating copy (i.e. make note...) button. ");
            GameObject action = GameObject.Instantiate(buttonPrefab);
            ActionBtn.CreateComponent(action, this, new Rule("MakeNote"));
            action.transform.SetParent(actionMenu.transform);
        }*/
        //end of addition
        if (containedValue) {
            GameObject action = GameObject.Instantiate(buttonPrefab);
            ActionBtn.CreateComponent(action, this, new Rule("TakeOut"));
            action.transform.SetParent(actionMenu.transform);
        }
    }

    public void ExecuteRule(Rule rule) {
        if (rule.action == "PickUp") {
            Player.Instance.AddItemToInventory(this);
            Player.Instance.CloseActionMenu();
            return;
        }

        if (rule.action == "MakeNote")
        {
            GameItem c = copy(this);
            Debug.Log("copy name: " + c.name + " dbItem: " + c.dbItem.name);
            Spawn(c);
            Player.Instance.AddItemToInventory(this);
            Player.Instance.CloseActionMenu();
            return;
        }

        PuzzleManager.Instance.ExecuteRule(rule, GetComponentInParent<GameArea>().area);

        // Check for items to destroy
        List<GameObject> objectsToDestroy = new List<GameObject>();
        for (int i = 0; i < rule.inputs.Count; i++) {
            bool found = false;
            foreach (Term output in rule.outputs) {
                if (output.name == rule.inputs[i].name) {
                    found = true;
                    output.gameItem = rule.inputs[i].gameItem;
                    break;
                } else if (output.GetPropertyWithName("contains") != null) {
                    if (output.GetPropertyWithName("contains").value == rule.inputs[i].name) {
                        Debug.Log("To insert: " + i);
                        output.gameItem.containedValue = rule.inputs[i].gameItem;
                        output.gameItem.containedValue.gameObject.SetActive(false);
                        found = true;
                        break;
                    }
                }
            }
            if (!found) {
                Debug.Log("Destroying: " + rule.inputs[i].name);
                if (i == 0) objectsToDestroy.Add(this.gameObject);
                else {
                    if(!Player.Instance.DeleteItemFromInventory(rule.inputs[i].gameItem))
                        objectsToDestroy.Add(rule.inputs[i].gameItem.gameObject);
                }
            }
        }
        int spawnIndex = 0;
        // Check for property change and new items
        foreach (Term output in rule.outputs) {
            //Debug.Log("output terms: " + output.ToString() + "number of input rules: " + rule.inputs.Count);
            bool found = false;
            foreach (Term input in rule.inputs) {
                //Debug.Log("output term: " + output.name + " input term: " + input.name);
                if (output.name == input.name) {
                    //Debug.Log("number of output properties " + output.properties.Count);
                    found = true;
                    foreach (Property outputProperty in output.properties) {
                        Property property = input.gameItem.GetProperty(outputProperty.name);
                        //Debug.Log("output property name: " + outputProperty.name);
                        if (property != null && property.name != "contains")
                        {                                
                            /*if (outputProperty.name == "inInventory")
                            {
                                Debug.Log("inInventory property exists");
                                if (outputProperty.value == "True")
                                {
                                    GameObject itemGO = (GameObject)Instantiate(output.dbItem.itemPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                                    Player.Instance.AddItemToInventory(itemGO.GetComponent<GameItem>());
                                }
                            }
                            else
                            {*/
                            property.value = outputProperty.value;
                            Debug.Log("Property to change: " + property.name);
                            break;        //this means only the first property to be changed will be changed?! 
                            //}
                            
                        }
                        else {
                            input.gameItem.properties.Add(outputProperty);
                        }
                        //Check if there is a better match, given the properties
                    }
                }
            }
            // Spawn new item
            if (!found) {
                if(output.dbItem != null) {
                    Debug.Log("spawning item on exectution of rule: " + output.dbItem.name);
                    GameObject itemGO;
                    if(objectsToDestroy.Count > spawnIndex) {
                        Transform transform = objectsToDestroy[spawnIndex].transform;
                        itemGO = (GameObject)Instantiate(output.dbItem.itemPrefab, transform.position + new Vector3(0, 1, 0), transform.rotation);
                        spawnIndex++;
                    } else {
                        Vector3 position = Player.Instance.transform.position + new Vector3(5, 15, 0); //Player.Instance.transform.forward; changed to x axis
                        itemGO = (GameObject)Instantiate(output.dbItem.itemPrefab, position, Quaternion.identity);
                    }                    
                    itemGO.GetComponent<GameItem>().Setup(output.dbItem.name, output.dbItem);
                    itemGO.transform.SetParent(this.gameObject.transform.parent);
                    Debug.Log("Spawning: " + output.dbItem);
                    if (output.dbItem.GetPropertyWithName("inInventory") != null)
                    {
                        //Debug.Log("trying inventory");
                        if(output.dbItem.GetPropertyWithName("inInventory").value == "True")
                            Player.Instance.AddItemToInventory(itemGO.GetComponent<GameItem>());
                        Debug.Log("straight to inventory...");
                    }
                } else {
                    //Debug.Log(output.name);
                    if(output.name == "Speech") {
                        if (output.GetPropertyWithName("name") != null)
                            Player.Instance.ShowSpeechBubble(output.GetPropertyWithName("text").value, output.GetPropertyWithName("name").value);
                        else
                            Player.Instance.ShowSpeechBubble(output.GetPropertyWithName("text").value);
                    }
                    if(output.name == "Player") {
                        Player.Instance.Execute(output);
                    }
                }
            }
        }

        Player.Instance.CloseActionMenu();

        foreach (GameObject GO in objectsToDestroy) {
            Destroy(GO);
        }
    }

    private bool RuleFulFilled(Rule rule) {
        if (rule != null) {
            rule.inputs[0].gameItem = this;
            if (!this.FulFillsProperties(rule.inputs[0])) {
                return false;
            }
            if (rule.inputs.Count > 1) {
                List<GameItem> inventory = Player.Instance.GetInventory();
                if (inventory.Count == 0) {
                    return false;
                }
                for (int i = 1; i < rule.inputs.Count; i++) {
                    bool found = false;
                    foreach (GameItem inventoryItem in inventory) {
                        if (inventoryItem.name == rule.inputs[i].name || inventoryItem.dbItem.GetSuperTypes().Contains(rule.inputs[i].name)) {
                            if (inventoryItem.FulFillsProperties(rule.inputs[i])) {
                                found = true;
                                rule.inputs[i].gameItem = inventoryItem;
                            }
                        }
                    }
                    if (!found) return false;
                }
            }
            return true;
        }
        return false;
    }

    private bool FulFillsProperties(Term input) {
        foreach (Property property in input.properties) {
            if (!HasProperty(property)) {
                return false;
            }
        }
        return true;
    }

    public bool HasProperty(Property propertyToCheck) {
        foreach (Property property in properties) {
            if (property.Equals(propertyToCheck))
                return true;
        }
        return false;
    }

    public Property GetProperty(string propertyName) {
        foreach (Property property in properties) {
            if (property.name == propertyName)
                return property;
        }
        return null;
    }

    public string toString()
    {
        return name;
    }
    
}

