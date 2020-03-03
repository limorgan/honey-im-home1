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
    public GameObject buttonPrefab;
    public GameObject inventoryMenu;
    public GameObject speechUI;
    public Text speechBubble;
    public Text speechBubbleName;
    public Text gameOver;

    private bool _actionMenuOpen = false;
    private bool _inventoryOpen = false;
    private bool _speechOpen = false;
    private bool _gameOver = false;
    [SerializeField]
    private List<GameItem> _inventory;
    private Item _dbItem;
    private List<Property> _properties;

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
    }

    void Update() {
        if (!_gameOver) {            
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
            if (Input.GetMouseButtonDown(0) && !_actionMenuOpen && !_inventoryOpen && !_speechOpen) {
                Vector2 v = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                if (hit) {
                    if (hit.collider.GetComponentInParent<GameItem>() != null) {
                        OpenActionMenu();
                        UITextRef.text = "";
                        hit.collider.gameObject.GetComponentInParent<GameItem>().OnGameItemClicked(actionMenuContent, buttonPrefab, ActionHeader);
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
            } else if (hit){//Physics.Raycast(ray, out hit)) {
                if (hit.collider.GetComponentInParent<GameItem>() != null)
                    if(!_actionMenuOpen && !_inventoryOpen)
                        hit.collider.gameObject.GetComponentInParent<GameItem>().OnGameItemMouseOver(UITextRef);
            } else {
                UITextRef.text = "";
            }

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
        _inventory.Add(item);
    }

    public void RemoveItemFromInventory(GameItem item) {
        for(int i = _inventory.Count - 1; i >= 0 ; i--) {
            if(_inventory[i] == item) {
                _inventory.RemoveAt(i);
            }
        }
        item.transform.position = (this.transform.position) + this.transform.forward * 2f;
        item.gameObject.SetActive(true);
    }

    public bool DeleteItemFromInventory(GameItem item) {
        for (int i = _inventory.Count - 1; i >= 0; i--) {
            if (_inventory[i] == item) {
                _inventory.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public List<GameItem> GetInventory() {
        return _inventory;
    }

    public void OpenInventory() {
        foreach(GameItem item in _inventory) {
            GameObject inventoryItem = GameObject.Instantiate(buttonPrefab);
            InventoryBtn.CreateComponent(inventoryItem, item);
            inventoryItem.transform.SetParent(inventoryMenu.transform.Find("Viewport").Find("Content"));
        }
        _inventoryOpen = true;
        inventoryMenu.SetActive(true);
        //DisableMouseLook();
        Time.timeScale = 0f;        //no background movement while menu is open
    }

    public void CloseInventory() {
        _inventoryOpen = false;
        inventoryMenu.SetActive(false);
        foreach (Transform child in inventoryMenu.transform.Find("Viewport").Find("Content")) {
            GameObject.Destroy(child.gameObject);
        }
        //EnableMouseLook();
        Time.timeScale = 1f;        //movement back on
    }

    public void OpenActionMenu() {
        _actionMenuOpen = true;     
        actionMenu.SetActive(true);
        //DisableMouseLook();       //not needed in 2D
        Time.timeScale = 0f;        //instead - to avoid background movement while menu is open
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

    public void CloseSpeechBubble() {
        _speechOpen = false;
        speechUI.gameObject.SetActive(false);
        //EnableMouseLook();
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
}
