﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[System.Serializable]
public class InventoryBtn : MonoBehaviour {

    private GameItem _gameItem;
    //public Image imageSpot;

    void Start() {
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(delegate () { OnMouseDown(); });
    }

    public void OnMouseDown() {
        if (!_gameItem.selected)
        {
            if(Player.Instance.getSelectedItem() == null)
                Player.Instance.SelectItemFromInventory(_gameItem);
            else
            {
                Player.Instance.OpenActionMenu(_gameItem, true);
                Player.Instance.CloseInventory();
            }
        }
        else
        {
            Player.Instance.DeselectItemFromInventory(_gameItem);
        }
        /*Player.Instance.RemoveItemFromInventory(_gameItem);
        Destroy(this.gameObject);*/
    }

    public static InventoryBtn CreateComponent(GameObject where, GameItem gameItem) {
        InventoryBtn actionBtn = where.AddComponent<InventoryBtn>();
        Property containsProp = gameItem.GetProperty("contains");
        if (containsProp != null) {
            where.GetComponentInChildren<Text>().text = gameItem.dbItem.description + "[" + containsProp.value + "]";
        } else {
            where.GetComponentInChildren<Text>().text = gameItem.dbItem.description;
        }
        actionBtn._gameItem = gameItem;
        return actionBtn;
    }
}