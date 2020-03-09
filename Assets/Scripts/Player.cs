using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class Player : MonoBehaviour {

    public Text UITextRef;
    public Text ActionHeader;
    //public Image CrossHair;
    public GameObject actionMenu;
    public GameObject actionMenuContent;
    public GameObject message;
    public float messageTime;
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
    public AudioSource spawnNoise;
    public AudioSource dropNoise;
    public AudioSource completeNoise;

    private bool _actionMenuOpen = false;
    private bool _inventoryOpen = false;
    private bool _speechOpen = false;
    private bool _gameOver = false;
    public bool noAction;
    [SerializeField]
    private List<GameItem> _inventory;
    private Item _dbItem;
    private List<Property> _properties;
    private string noActionMessage = "No action currently available";
    private GameItem selectedItem;

    private static Player _instance;
    public static Player Instance { get { return _instance; } }

    void Awake() {
        if (_instance != null & _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
    }

    void Start() {
        _inventory = new List<GameItem>();
        _dbItem = ItemDatabase.GetObject("Player");
        _properties = _dbItem.properties;
        closeAllMenus();
        inventoryNotification.SetActive(false);
    }

    void Update() {
        if (!_gameOver) {            
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
            if (Input.GetMouseButtonDown(0) && !_actionMenuOpen && !_inventoryOpen && !_speechOpen) {
                Vector2 v = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                if (hit) {
                    if (hit.collider.GetComponentInParent<GameItem>() != null) {
                        hit.collider.gameObject.GetComponentInParent<GameItem>().OnGameItemClicked(actionMenuContent, buttonPrefab, ActionHeader);
                        OpenActionMenu();
                        //UITextRef.text = "";
                        }
                }
            } else if (_actionMenuOpen && Input.GetKeyDown(KeyCode.Q)) {
                CloseActionMenu();
            } else if (_speechOpen && Input.GetKeyDown(KeyCode.Q)) {
                CloseSpeechBubble();
            } else if (Input.GetKeyDown(KeyCode.Tab) && !_actionMenuOpen) {
                if (_inventoryOpen)
                    CloseInventory();
                else
                    OpenInventory();
            } else if (hit){
                if (hit.collider.GetComponentInParent<GameItem>() != null)
                    if(!_actionMenuOpen && !_inventoryOpen)
                        hit.collider.gameObject.GetComponentInParent<GameItem>().OnGameItemMouseOver(UITextRef);
            } else if (Input.GetKeyDown(KeyCode.Return) && _speechOpen)
                CloseSpeechBubble();
            else {
                UITextRef.text = "";
                if (selectedItem != null)
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
        SelectItemFromInventory(item);                  //automatically select the last item picked up: default
    }

    public void RemoveItemFromInventory(GameItem item) {
        for(int i = _inventory.Count - 1; i >= 0 ; i--) {
            if(_inventory[i] == item) {
                _inventory.RemoveAt(i);
                item.dbItem.GetPropertyWithName("inInventory").RemoveProperty();             // property showing if in inventory or not.  
                item.transform.position = (this.transform.position) + new Vector3(3, 15, 0); //this.transform.forward * 2f; Appears to the side of character
                dropNoise.Play();   //plays drop noise
                item.gameObject.SetActive(true);
                if (item.selected)
                {
                    item.selected = false;
                    selectedItem = null;
                    selectedItemField.SetActive(false);
                }
            }
        }
    }

    public void RemoveSelectedFromInventory()
    {
        DeleteItemFromInventory(this.selectedItem);
    }

    public bool DeleteItemFromInventory(GameItem item) {
        for (int i = _inventory.Count - 1; i >= 0; i--) {
            if (_inventory[i] == item) {
                _inventory.RemoveAt(i);
                if (item.selected)
                {
                    item.selected = false;
                    selectedItem = null;
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
                    if(selectedItem != null)
                        DeselectItemFromInventory(selectedItem);  //deselected currently selected item
                    selectedItem = item;
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
                selectedItem = null;
                GameObject.Destroy(selectedItemField.GetComponentInChildren<SelectedItemButton>());
                selectedItemField.SetActive(false);
                //actionMenuContent = null;
            }
        }
    }

    public GameItem getSelectedItem()
    {
        return selectedItem;
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
        //EnableMouseLook();
        Time.timeScale = 1f;        //movement back on
    }

    public void OpenActionMenu() {
        if (noAction)
        {
            message.GetComponentInChildren<Text>().text = noActionMessage;
            Appear(message, messageTime);
            noAction = false;
        }
        else
        {
            _actionMenuOpen = true;
            actionMenu.SetActive(true);
            //DisableMouseLook();       //not needed in 2D
            Time.timeScale = 0f;        //instead - to avoid background movement while menu is open
        }
    }

    public void OpenActionMenu(GameItem item)
    {
        item.OnGameItemClicked(actionMenuContent, buttonPrefab, ActionHeader);
        OpenActionMenu();
    }

    public void OpenActionMenu(GameItem item, bool inventory)
    {
        item.OnGameItemClicked(actionMenuContent, buttonPrefab, ActionHeader, inventory);
        OpenActionMenu();
    }

    public void CloseActionMenu() {
        if (!_speechOpen) {            
            _actionMenuOpen = false;
            actionMenu.SetActive(false);
            foreach (Transform child in actionMenuContent.transform) {
                GameObject.Destroy(child.gameObject);
            }
            //EnableMouseLook();
            Time.timeScale = 1f;        //reenable background movement on closing menu
        }
    }

    public void ShowSpeechBubble(string speech, string name) {
        CloseActionMenu();
        //DisableMouseLook();
        _speechOpen = true;
        speechUI.gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(TypeSentenceSlowly(speech, speechBubble));
        //speechBubble.text = speech;
        speechBubbleName.text = name;
    }

    public void ShowSpeechBubble(string speech)     //speech bubble when the name field is not filled out
    {
        ShowSpeechBubble(speech, "");
    }

    public void CloseSpeechBubble() {
        _speechOpen = false;
        speechUI.gameObject.SetActive(false);
        //EnableMouseLook();
    }

    public void ShowHint()
    {
        hintSystem.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(TypeSentenceSlowly(PuzzleManager.Instance.getHint(), hintSystem.GetComponentInChildren<Text>()));
        Debug.Log("hint: " + PuzzleManager.Instance.getHint() + " current text: " + hintSystem.GetComponentInChildren<Text>().text);
        //hintSystem.GetComponentInChildren<Text>().text = PuzzleManager.Instance.getHint();
    }

    public void CloseHint()
    {
        hintSystem.SetActive(false);
        hintSystem.GetComponentInChildren<Text>().text = "";
    }

    public void updateAreaName(string nextArea)
    {
        areaText.text = nextArea;
    }

    public void showFinishMessage(string name)
    {
        /*message.GetComponentInChildren<Text>().text = "This area is completed [" + name + "]";
        Appear(message, messageTime);*/
        //other option: play finished sound: 
        completeNoise.Play();
    }

    private void DisableMouseLook() {
        //CrossHair.enabled = false;
        UITextRef.enabled = false;
        GetComponentInParent<FirstPersonController>().enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void EnableMouseLook() {
        //CrossHair.enabled = true;
        UITextRef.enabled = true;
        GetComponentInParent<FirstPersonController>().enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    IEnumerator TypeSentenceSlowly(string sentence, Text goal)
    {//Brackeys, How to make a Dialogue System in Unity https://www.youtube.com/watch?v=_nRzoTzeyxU
        goal.text = "";
        foreach (char c in sentence.ToCharArray())
        {
            goal.text += c;
            yield return null;
        }
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

    public void closeAllMenus()
    {
        CloseInventory();
        CloseActionMenu();
        CloseSpeechBubble();
        message.SetActive(false);
        hintSystem.SetActive(false);
    }
}
