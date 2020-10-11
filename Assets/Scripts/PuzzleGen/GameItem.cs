using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameItem : MonoBehaviour {

    public new string name;
    [SerializeField]
    public Item dbItem;
    public List<Property> properties;
    public GameItem containedValue;
    public bool selected;

    private bool _pickedUp = false;

    void Awake() {
        if (dbItem == null) {
            dbItem = PuzzleManager.Instance.GetObject(name);
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
        if(name != "Player")
            UITextRef.text = dbItem.description;
    }

    // Addition 10/01/2020
    public GameItem copy(GameItem original)
    {
        GameItem c = new GameItem();
        c.Setup(original.name, original.dbItem);
        c.Awake();
        return c;
    }

    //Addition 10/01/2020
    private void Spawn(GameItem item)
    {
        Instantiate(item.dbItem.itemPrefab, transform.position, transform.rotation); // trans.pos + + new Vector3(0, 1, 0)...
        //Debug.Log("Spawning: " + item.dbItem);
    }

    public void OnGameItemClicked(GameObject actionMenu, GameObject buttonPrefab, Text ActionHeader, bool inventory)
    {
        if (!inventory)
            OnGameItemClicked(actionMenu, buttonPrefab, ActionHeader);
        else
        {
            if (selected)
                OnGameItemClicked(actionMenu, buttonPrefab, ActionHeader);
            else
            {
                GameObject action = GameObject.Instantiate(buttonPrefab);
                ActionBtn.CreateComponent(action, this, new Rule("Select"));
                action.transform.SetParent(actionMenu.transform);

                OnGameItemClicked(actionMenu, buttonPrefab, ActionHeader);
            }
        }
    }

    public void OnGameItemClicked(GameObject actionMenu, GameObject buttonPrefab, Text ActionHeader) {
        ActionHeader.text = this.dbItem.description;
        bool noAction = true;   //keep track of whether or not there are any actions
        if (GetProperty("inInventory") != null && GetProperty("inInventory").value == "True")
            noAction = false;
        foreach (Rule puzzleRule in PuzzleManager.Instance.RulesFor(this, PuzzleManager.Instance.GetCurrentArea()))
        {         //not necessarily accurate (nested areas) previous version: GetComponentInParent<GameArea>().area)
            Debug.Log("Checking rule " + puzzleRule.ToString() + " fulfilled by " + this.name + " ? " + RuleFulFilled(puzzleRule));
            if (RuleFulFilled(puzzleRule)) {
                noAction = false;
                GameObject action = GameObject.Instantiate(buttonPrefab);
                ActionBtn.CreateComponent(action, this, puzzleRule);
                action.transform.SetParent(actionMenu.transform);
            }
        }
        if (dbItem.IsCarryable() && !selected && (GetProperty("inInventory") == null || GetProperty("inInventory").value == "False")) {
            noAction = false;
            GameObject action = GameObject.Instantiate(buttonPrefab);
            ActionBtn.CreateComponent(action, this, new Rule("PickUp"));
            action.transform.SetParent(actionMenu.transform);
        }
        if (dbItem.IsInspectable())
        {
            noAction = false;
            GameObject action = GameObject.Instantiate(buttonPrefab);
            ActionBtn.CreateComponent(action, this, new Rule("Inspect"));
            action.transform.SetParent(actionMenu.transform);
        }
        if (containedValue && containedValue.GetProperty("carryable") != null) {
            if (containedValue.GetProperty("carryable").value == "True")
            {
                noAction = false;
                GameObject action = GameObject.Instantiate(buttonPrefab);
                ActionBtn.CreateComponent(action, this, new Rule("TakeOut"));
                action.transform.SetParent(actionMenu.transform);
            }
        }
        if(selected)
        {
            noAction = false;            
            GameObject action = GameObject.Instantiate(buttonPrefab);
            ActionBtn.CreateComponent(action, this, new Rule("Deselect"));
            action.transform.SetParent(actionMenu.transform);
        }

        if(GetProperty("inInventory") != null && GetProperty("inInventory").value == "True")
        {
            noAction = false;
            GameObject action = GameObject.Instantiate(buttonPrefab);
            ActionBtn.CreateComponent(action, this, new Rule("Drop"));
            action.transform.SetParent(actionMenu.transform);
        }
        Player.Instance._noAction = noAction;

        if(GetProperty("ison") != null && GetProperty("ison").value == "True" && this.gameObject.GetComponent<MusicControl>() != null)
        {
            noAction = false;
            GameObject action = GameObject.Instantiate(buttonPrefab);
            if (this.gameObject.GetComponent<MusicControl>().IsMuted())
                ActionBtn.CreateComponent(action, this, new Rule("Unmute"));
            else
                ActionBtn.CreateComponent(action, this, new Rule("Mute"));
            action.transform.SetParent(actionMenu.transform);
        }
    }

    public void ExecuteRule(Rule rule) {
        //NOTE: These are rules which are automatically available if certain properties are fulfilled
        
        if (rule.action == "Inspect")
        {
            Player.Instance.ShowSpeechBubble(dbItem.longDescription, dbItem.description);
            Statistics.Instance.UpdateInspects();
            return;
        }

        Statistics.Instance.UpdateActions();

        if (rule.action == "PickUp") {
            Player.Instance.AddItemToInventory(this);
            Player.Instance.CloseActionMenu();
            return;
        }

        if (rule.action == "Drop")
        {
            Player.Instance.DeselectItemFromInventory(this);
            Player.Instance.RemoveItemFromInventory(this);
            Player.Instance.CloseActionMenu();
            return;
        }

        if (rule.action == "Select")
        {
            Player.Instance.SelectItemFromInventory(this);
            Player.Instance.CloseActionMenu();
            return;
        }

        if (rule.action == "Deselect")
        {
            Player.Instance.DeselectItemFromInventory(this);
            Player.Instance.CloseActionMenu();
            return;
        }

        if (rule.action == "TakeOut")
        {
            this.containedValue.gameObject.SetActive(true);
            this.containedValue.gameObject.transform.position = (this.transform.position) + new Vector3(0, 2, 0); //this.transform.forward * 2f; Appears to the side of character
            this.containedValue = null;
            this.GetProperty("contains").RemoveProperty();
            Player.Instance.CloseActionMenu();
            return;
        }

        if (rule.action == "Mute")
        {
            MusicControl m = this.gameObject.GetComponent<MusicControl>();
            if (m != null)
            {
                m.TurnDownVolume();
                m.Mute();
                Player.Instance.GetComponent<AudioSource>().volume = 1f;
            }
            Player.Instance.CloseActionMenu();
            return;
        }

        if (rule.action == "Unmute")
        {
            MusicControl m = this.gameObject.GetComponent<MusicControl>();
            if (m != null)
            {
                m.Unmute();
            }
            Player.Instance.CloseActionMenu();
            return;
        }
        ExecuteRule(rule, true, this);
    }

    public static void ExecuteRule(Rule rule, bool full, GameItem gameI)
    {
        // Change:  This is normally part of the above ExecuteRule() function, but was separated to  
        //          be able to call if from within the puzzle manager script when executing "automatic"
        //          rules -- has not yet been fully implemented
        // Check for items to destroy
        List<GameObject> objectsToDestroy = new List<GameObject>();
        for (int i = 0; i < rule.inputs.Count; i++)
        {
            bool found = false;
            foreach (Term output in rule.outputs)
            {
                if (output.name == rule.inputs[i].name)
                {
                    found = true;
                    output.gameItem = rule.inputs[i].gameItem;
                    break;
                }
                else if (output.GetPropertyWithName("contains") != null)
                {
                    if (output.GetPropertyWithName("contains").value == rule.inputs[i].name)
                    {
                        //Debug.Log("To insert: " + i);
                        if (rule.inputs[i].name == Player.Instance.getSelectedItem().name)
                        {
                            output.gameItem.containedValue = Player.Instance.getSelectedItem();
                            Player.Instance.RemoveSelectedFromInventory();
                            output.gameItem.containedValue.gameObject.SetActive(false);
                        }
                        else
                        {
                            output.gameItem.containedValue = rule.inputs[i].gameItem;
                            output.gameItem.containedValue.gameObject.SetActive(false);
                        }
                        found = true;
                        break;
                    }
                }
            }
            Debug.Log("Intput Item at " + i + ": " + rule.inputs[i] + "of rule: " + rule.ToString());
            if (!found && rule.inputs[i].gameItem.IsDestructible())
            {
                Debug.Log("Destroying: " + rule.inputs[i].name);
                if (i == 0) objectsToDestroy.Add(gameI.gameObject);
                else
                {
                    if (!Player.Instance.DeleteItemFromInventory(rule.inputs[i].gameItem))
                        objectsToDestroy.Add(rule.inputs[i].gameItem.gameObject);
                }
            }
        }
        int spawnIndex = 0;
        // Check for property change and new items
        bool firstOutput = true;
        foreach (Term output in rule.outputs)
        {
            //Debug.Log("output terms: " + output.ToString() + "number of input rules: " + rule.inputs.Count);
            bool found = false;
            foreach (Term input in rule.inputs)
            {
                //Debug.Log("output term: " + output.name + " input term: " + input.name);
                if (output.name == "Player")
                {
                    foreach (Property outputProperty in output.properties)
                        PuzzleManager.Instance.UpdatePlayerProperties(outputProperty);
                }
                if (output.name == input.name)
                {
                    //Debug.Log("number of output properties " + output.properties.Count);
                    found = true;
                    foreach (Property outputProperty in output.properties)
                    {
                        Property property = input.gameItem.GetProperty(outputProperty.name);
                        if (property != null && property.name != "contains")
                        {
                            property.value = outputProperty.value;
                            Debug.Log("Property to change: " + property.name);
                            break;        //this means only the first property to be changed will be changed?! 
                                          //}

                        }
                        else
                        {
                            input.gameItem.properties.Add(outputProperty);
                        }
                        //Check if there is a better match, given the properties
                    }
                }
            }
            // Spawn new item
            if (!found)
            {
                if (output.dbItem != null)
                {
                    //Debug.Log("spawning item on exectution of rule: " + output.dbItem.name);
                    GameObject itemGO;
                    if (objectsToDestroy.Count > spawnIndex)
                    {
                        Transform transform = objectsToDestroy[spawnIndex].transform;
                        itemGO = (GameObject)Instantiate(output.dbItem.itemPrefab, transform.position + new Vector3(0, 1, 0), transform.rotation);
                        spawnIndex++;
                    }
                    else
                    {
                        Vector3 position = Player.Instance.transform.position + new Vector3(0, 20, 0); //2D: changed to x axis
                        position.z = 0;     // Addition: items kept spawning with bizarre z values - not necessary in 3D 
                        itemGO = (GameObject)Instantiate(output.dbItem.itemPrefab, position, Quaternion.identity);
                    }
                    itemGO.GetComponent<GameItem>().Setup(output.dbItem.name, output.dbItem);
                    itemGO.transform.SetParent(gameI.gameObject.transform.parent);
                    if (Player.Instance.spawnNoise != null)  // Addition: plays spawn noise
                        Player.Instance.spawnNoise.Play();
                    if (rule.inventory)
                    {
                        if (itemGO.GetComponent<GameItem>().name == rule.outputs[0].name || firstOutput)
                        {
                            Player.Instance.AddItemToInventory(itemGO.GetComponent<GameItem>());
                            Debug.Log("straight to inventory: " + itemGO.GetComponent<GameItem>().name);
                        }
                    }
                }
                else
                {
                    if (output.name == "Speech")
                    {
                        if (output.GetPropertyWithName("name") != null)
                            Player.Instance.ShowSpeechBubble(output.GetPropertyWithName("text").value, output.GetPropertyWithName("name").value);
                        else
                            Player.Instance.ShowSpeechBubble(output.GetPropertyWithName("text").value);
                    }
                    if (output.name == "Player")
                    {
                        Player.Instance.Execute(output);
                    }
                }
            }
            firstOutput = false;
        }
        PuzzleManager.Instance.ExecuteRule(rule, PuzzleManager.Instance.GetCurrentArea());     //Original: GetComponentInParent<GameArea>().area - issue with nested areas

        Player.Instance.CloseActionMenu();

        foreach (GameObject GO in objectsToDestroy)
        {
            Destroy(GO);
        }
    }

    private bool RuleFulFilled(Rule rule) {
        if (rule != null) {                                         //Addition: handling of "selected" item restriction
            rule.inputs[0].gameItem = this;     
            if (!this.FulFillsProperties(rule.inputs[0])) 
                return false;
            if (rule.inputs.Count > 1) {
                int i = 1;
                if (!rule.selectedInput)
                {
                    GameItem selectedItem = Player.Instance.getSelectedItem();
                    if (selectedItem != null)
                    {
                        if (selectedItem.name == rule.inputs[1].name || selectedItem.dbItem.GetSuperTypes().Contains(rule.inputs[1].name))
                            if (selectedItem.FulFillsProperties(rule.inputs[1]))
                            {
                                rule.inputs[1].gameItem = selectedItem;
                                i = 2;
                            }
                        else
                                return false;
                    }
                    else
                        return false;
                }
                List<GameItem> inventory = Player.Instance.GetInventory();
                if (inventory.Count == 0 && !rule.HasPlayerInput()) {
                    return false;
                }
                for (; i < rule.inputs.Count; i++) {
                    bool found = false;
                    if (rule.inputs[i].name == "Player") {
                        if (PuzzleManager.Instance.GetPlayer().FulFillsProperties(rule.inputs[i]))
                        {
                            found = true;
                            rule.inputs[i].gameItem = PuzzleManager.Instance.GetPlayer();
                        }
                    }
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

    public bool IsDestructible()
    {
        Property destructible = this.GetProperty("indestructible");
        if (destructible != null)
            if (destructible.value == "True")
                return false;
        return true;
    }

    public void Inspect()
    {
        if (dbItem.longDescription == "")
            Player.Instance.ShowSpeechBubble("I know nothing about this...", dbItem.description);
        else
            Player.Instance.ShowSpeechBubble(dbItem.longDescription, dbItem.description);
    }

    public string ToString()
    {
        return name;
    }
}

