using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using System.Text;

public class Player : MonoBehaviour {

    //NOTE: This class contains too many variables and functions which should generally be managed differently. 

    public Text UITextRef;
    public Text actionHeader;
    public GameObject actionMenu;
    public GameObject actionMenuContent;
    public GameObject message;              //Addition: used to display messages, e.g. when an item has no avaiable actions
    public float messageTime;               //        -  time that that action is displayed for 
    public GameObject buttonPrefab;
    public GameObject inventoryMenu;
    public GameObject inventoryButton;          
    public GameObject inventoryNotification;    
    public GameObject speechUI;                 
    public GameObject selectedItemField;        
    public Text speechBubble;
    public Text speechBubbleName;
    public Text gameOver;
    public Text areaText;                       
    public GameObject hintSystem;
    public AudioSource spawnNoise;          //Addition: specific sound feedback for certain actions (not general)
    public AudioSource dropNoise;
    public AudioSource completeNoise;

    private bool _actionMenuOpen = false;
    private bool _inventoryOpen = false;
    private bool _speechOpen = false;
    private bool _gameOver = false;
    private bool _pauseMenuOpen = false;
    public bool _noAction;
    private string _noActionMessage = "No Action Currently Available. ";
    [SerializeField]
    private List<GameItem> _inventory = new List<GameItem>();
    private Item _dbItem;
    private List<Property> _properties;
    private GameItem _selectedItem;
    private StringBuilder _transcript = new StringBuilder();

    private bool _isTyping = false;
    private string _currentSpeech = "";

    private static Player _instance;
    public static Player Instance { get { return _instance; } }

    void Awake() {
        if (_instance != null & _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
    }

    void Start() {
        _dbItem = PuzzleManager.Instance.GetObject("Player");
        _properties = _dbItem.properties;
        CloseAllMenus();
        inventoryNotification.SetActive(false);
        ShowObjective(true);
    }

    void Update() {
        if (!_gameOver) {            
            RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));         //Ray casts  were modified to suit 2D features
            if (Input.GetMouseButtonDown(0) && !_actionMenuOpen && !_inventoryOpen && !_speechOpen && !_pauseMenuOpen)
            {
                Vector2 v = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                if (hit)
                {
                    if (hit.collider.GetComponentInParent<GameItem>() != null)
                    {
                        hit.collider.gameObject.GetComponentInParent<GameItem>().OnGameItemClicked(actionMenuContent, buttonPrefab, actionHeader);
                        OpenActionMenu();
                    }
                }
            }
            else if (Input.GetMouseButtonDown(1) && !_actionMenuOpen && !_inventoryOpen && !_speechOpen && !_pauseMenuOpen)
            {       //right click to inspect quickly
                Vector2 v = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                if (hit)
                {
                    if (hit.collider.GetComponentInParent<GameItem>() != null)
                    {
                        hit.collider.gameObject.GetComponentInParent<GameItem>().Inspect();
                        Statistics.Instance.UpdateInspects(PuzzleManager.Instance.GetCurrentArea());    // registering the number of "inspect" actions
                    }
                }
            }
            else if (_actionMenuOpen && Input.GetKeyDown(KeyCode.Q))
            {
                CloseActionMenu();
            }
            else if (_speechOpen && Input.GetKeyDown(KeyCode.Q))
            {
                CloseSpeechBubble();
            }
            else if (Input.GetKeyDown(KeyCode.Tab) && !_actionMenuOpen)
            {
                if (_inventoryOpen)
                    CloseInventory();
                else
                    OpenInventory();
            }
            else if (hit && !_pauseMenuOpen)
            {
                if (hit.collider.GetComponentInParent<GameItem>() != null)
                    if (!_actionMenuOpen && !_inventoryOpen)
                        hit.collider.gameObject.GetComponentInParent<GameItem>().OnGameItemMouseOver(UITextRef);
            }
            else if (Input.GetKeyDown(KeyCode.Return) && _speechOpen)
                CloseSpeechBubble();
            else
            {
                UITextRef.text = "";
                if (_selectedItem != null)
                    selectedItemField.SetActive(true);
            }

            if (_inventory.Count == 0)
                inventoryNotification.SetActive(false);
        }
    }

    public void Execute(Term term) {
        Property status = term.GetPropertyWithName("alive");
        if (status != null) {
            if(status.value == "False") {
                gameOver.gameObject.SetActive(true);
                _gameOver = true;
            }
        }
    }

    public void AddItemToInventory(GameItem item) {
        item.gameObject.SetActive(false);
        if (item.dbItem.GetPropertyWithName("inInventory") == null)
            item.dbItem.properties.Add(new Property(PropertyType.BoolProperty, "inInventory", "True"));
        else
            item.dbItem.GetPropertyWithName("inInventory").value = "True";
        _inventory.Add(item);
        inventoryNotification.SetActive(true);
        SelectItemFromInventory(item);                  //Addition: automatically select the last item picked up: default
    }

    public void RemoveItemFromInventory(GameItem item) {
        for(int i = _inventory.Count - 1; i >= 0 ; i--) {
            if(_inventory[i] == item) {
                _inventory.RemoveAt(i);
                item.dbItem.GetPropertyWithName("inInventory").RemoveProperty();                // property showing if in inventory or not.  
                item.transform.position = (this.transform.position) + new Vector3(3, 15, 0);    // Item appears to the side of character -- awkward?
                dropNoise.Play();                                                               // plays drop noise
                item.gameObject.SetActive(true);
                if (item.selected)
                {
                    item.selected = false;
                    _selectedItem = null;
                    selectedItemField.SetActive(false);
                }
            }
        }
    }

    public void RemoveSelectedFromInventory()
    {
        DeleteItemFromInventory(this._selectedItem);
    }

    public bool DeleteItemFromInventory(GameItem item) {
        for (int i = _inventory.Count - 1; i >= 0; i--) {
            if (_inventory[i] == item) {
                _inventory.RemoveAt(i);
                if (item.selected)
                {
                    item.selected = false;
                    _selectedItem = null;
                    selectedItemField.SetActive(false);
                }
                return true;
            }
        }
        return false;
    }

    public void SelectItemFromInventory(GameItem item)
    {
        for (int i = _inventory.Count - 1; i >= 0; i--)
        {
            if (_inventory[i] == item)
            {
                if (!item.selected)
                {
                    if (_selectedItem != null)
                    {
                        DeselectItemFromInventory(_selectedItem);  //deselected currently selected item
                        foreach (Transform child in actionMenuContent.transform)
                        {
                            GameObject.Destroy(child.gameObject);
                        }
                    }
                    _selectedItem = item;
                    item.selected = true;
                    SelectedItemButton.CreateComponent(selectedItemField, item);
                    selectedItemField.SetActive(true);
                    CloseInventory();
                }
            }
        }
    }

    public void DeselectItemFromInventory(GameItem item)
    {
        for (int i = _inventory.Count - 1; i >= 0; i--)
        {
            if (_inventory[i] == item)
            {
                if (!item.selected)
                    return;
                item.selected = false;
                _selectedItem = null;
                GameObject.Destroy(selectedItemField.GetComponentInChildren<SelectedItemButton>());
                selectedItemField.SetActive(false);
                foreach (Transform child in actionMenuContent.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
                //actionMenuContent = null;
            }
        }
    }

    public GameItem getSelectedItem()
    {
        return _selectedItem;
    }

    public List<GameItem> GetInventory() {
        return _inventory;
    }

    public void OpenInventory() {
        if (_actionMenuOpen)
            CloseActionMenu();
        foreach(GameItem item in _inventory) {
            GameObject inventoryItem = GameObject.Instantiate(buttonPrefab);
            InventoryBtn.CreateComponent(inventoryItem, item);
            inventoryItem.transform.SetParent(inventoryMenu.transform.Find("Viewport").Find("Content"));
        }
        _inventoryOpen = true;
        actionMenu.SetActive(false);
        inventoryMenu.SetActive(true);
        inventoryButton.SetActive(false);
        inventoryNotification.SetActive(false);
        //DisableMouseLook();
        Time.timeScale = 0f;        //no background movement while menu is open
    }

    public void CloseInventory() {
        _inventoryOpen = false;
        inventoryMenu.SetActive(false);
        inventoryButton.SetActive(true);
        foreach (Transform child in inventoryMenu.transform.Find("Viewport").Find("Content")) {
            GameObject.Destroy(child.gameObject);
        }
        Time.timeScale = 1f;        
    }

    public void OpenActionMenu() {
        if (_noAction)
        {
            message.GetComponentInChildren<Text>().text = _noActionMessage;
            Appear(message, messageTime);
            _noAction = false;
        }
        else
        {
            _actionMenuOpen = true;
            actionMenu.SetActive(true);
            Time.timeScale = 0f;            
        }
    }

    public void OpenActionMenu(GameItem item)
    {
        if (_inventoryOpen)
            CloseInventory();
        if(_actionMenuOpen)
            CloseActionMenu();
        item.OnGameItemClicked(actionMenuContent, buttonPrefab, actionHeader);
        OpenActionMenu();
    }

    public void OpenActionMenu(GameItem item, bool inventory)
    {
        if (_actionMenuOpen)
            CloseActionMenu();
        item.OnGameItemClicked(actionMenuContent, buttonPrefab, actionHeader, inventory);
        OpenActionMenu();
    }

    public void CloseActionMenu() {
        if (!_speechOpen) {            
            _actionMenuOpen = false;
            actionMenu.SetActive(false);
            foreach (Transform child in actionMenuContent.transform) {
                GameObject.Destroy(child.gameObject);
            }
            Time.timeScale = 1f;       
        }
    }

    public void ShowSpeechBubble(string speech, string name) {
        _currentSpeech = speech;
        CloseActionMenu();
        AddToTranscript(speech, name);
        _speechOpen = true;
        speechUI.gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(TypeSentenceSlowly(speech, speechBubble));
        speechBubbleName.text = name;
    }

    public void ShowSpeechBubble(string speech)     //speech bubble when the name field is not filled out
    {
        ShowSpeechBubble(speech, "");
    }

    public void CloseSpeechBubble() {
        if (_isTyping)
        {
            StopAllCoroutines();
            speechBubble.text = _currentSpeech;
            _isTyping = false;
        }
        else
        {
            _speechOpen = false;
            speechUI.gameObject.SetActive(false);
            //EnableMouseLook();
        }
    }

    public void ShowHint()
    {
        hintSystem.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(TypeSentenceSlowly(PuzzleManager.Instance.GetHint(), hintSystem.GetComponentInChildren<Text>()));
    }

    public void CloseHint()
    {
        if(_isTyping)
        {
            StopAllCoroutines();
            hintSystem.GetComponentInChildren<Text>().text = PuzzleManager.Instance.GetHint();
            _isTyping = false;
        }
        else
        {
            hintSystem.SetActive(false);
            hintSystem.GetComponentInChildren<Text>().text = "";
        }
    }

    public void UpdateAreaName(string nextArea)
    {
        areaText.text = nextArea;
    }

    public void ShowFinishMessage(string name)
    {
        /*message.GetComponentInChildren<Text>().text = "This area is completed [" + name + "]";
        Appear(message, messageTime);*/
        //other option: play finished sound: 
        completeNoise.Play();
        if (!_gameOver)
            ShowObjective(false);
    }   

    public void ShowObjective(bool first)
    {
        string objective = "";
        if (!first)
        {
            objective += "That's done. ";
            List<Area> connectedAreas = PuzzleManager.Instance.GetCurrentArea().connectedTo;
            if (connectedAreas.Count > 0)
            {
                for (int i = 0; i < connectedAreas.Count; i++)
                {
                    if (i > 0)
                        objective += " And...";
                    objective += connectedAreas[i].GetObjective();
                }
            }
        }
        else
            objective += PuzzleManager.Instance.GetCurrentArea().GetObjective();
        ShowSpeechBubble(objective, "Objective");
    }

    IEnumerator TypeSentenceSlowly(string sentence, Text goal)
    {//Brackeys, How to make a Dialogue System in Unity https://www.youtube.com/watch?v=_nRzoTzeyxU
        goal.text = "";
        _isTyping = true;
        foreach (char c in sentence.ToCharArray())
        {
            goal.text += c;
            yield return null;
        }
        _isTyping = false;
    }

    IEnumerator CloseSlowly(GameObject obj, float timeInS)
    {
        yield return new WaitForSeconds(timeInS);
        obj.SetActive(false);
    }

    public void Appear(GameObject obj, float time)
    {
        obj.SetActive(true);
        StartCoroutine(CloseSlowly(obj, time));
    }

    public void CloseAllMenus()
    {
        CloseInventory();
        CloseActionMenu();
        CloseSpeechBubble();
        message.SetActive(false);
        hintSystem.SetActive(false);
    }

    public void PauseMenuStatus(bool open)
    {
        _pauseMenuOpen = open;
    }

    public void AddToTranscript(string line)
    {
        AddToTranscript(line, "");
    }

    public void AddToTranscript(string line, string name)
    {
        StringBuilder total = new StringBuilder();
        if (name.Length > 0)
            total.AppendLine(name + ":");
        total.AppendLine(line);
        if (_transcript.Length > 0)
            total.AppendLine("".PadLeft(60, '*'));
        _transcript.Insert(0, total);

    }

    public string GetTranscript()
    {
        return _transcript.ToString();
    }
}
