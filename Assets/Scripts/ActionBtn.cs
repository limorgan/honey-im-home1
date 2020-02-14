using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ActionBtn : MonoBehaviour {

    private GameItem _gameItem;
    public Rule rule;

    void Start() {
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(delegate () { OnMouseDown(); });
    }

    public void OnMouseDown() {
        _gameItem.ExecuteRule(rule);
    }

    public static ActionBtn CreateComponent(GameObject where, GameItem gameItem, Rule rule) {
        ActionBtn actionBtn = where.AddComponent<ActionBtn>();
        string action = "";
        for(int i = 0; i < rule.action.Length; i++) {
            if (char.IsUpper(rule.action[i])) {
                action += " ";
            }
            action += rule.action[i];
        }
        where.GetComponentInChildren<Text>().text = action;
        actionBtn._gameItem = gameItem;
        actionBtn.rule = rule;
        return actionBtn;
    }
}
