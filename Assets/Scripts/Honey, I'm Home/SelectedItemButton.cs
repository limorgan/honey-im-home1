using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SelectedItemButton : MonoBehaviour
{
    private GameItem _gameItem;

    // Start is called before the first frame update
    void Start()
    {
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(delegate () { OnMouseDown(); });
    }

    public void OnMouseDown()
    {
        Player.Instance.OpenActionMenu(_gameItem);
    }

    public static SelectedItemButton CreateComponent(GameObject where, GameItem gameItem)
    {
        SelectedItemButton actionBtn = where.AddComponent<SelectedItemButton>();
        Property containsProp = gameItem.GetProperty("contains");
        if (containsProp != null)
        {
            where.GetComponentInChildren<Text>().text = gameItem.dbItem.description + "[" + containsProp.value + "]";
        }
        else
        {
            where.GetComponentInChildren<Text>().text = gameItem.dbItem.description;
        }
        actionBtn._gameItem = gameItem;
        return actionBtn;
    }
}
