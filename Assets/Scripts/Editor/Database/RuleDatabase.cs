using System.Collections.Generic;
using UnityEngine;

public class RuleDatabase : Database<Rule> {

    static protected string filePath = "Assets/Resources/Rules/Rule.asset";

    static public void CreateAsset() {
        CreateAsset(filePath);
    }

    static public List<Rule> GetAllRulesWithInput(Item dbItem) {
        List<Rule> rules = new List<Rule>();
        for (int i = 0; i < _assets.Count; i++) {
            if ((_assets[i].inputs[0].name == dbItem.name ||
                dbItem.GetSuperTypes().Contains(_assets[i].inputs[0].name))) {
                Rule ruleToAdd = ScriptableObject.Instantiate(_assets[i]) as Rule;
                rules.Add(ruleToAdd);
            }
        }
        return rules;
    }

    static public List<Rule> GetRulesWithOutput(Term term) {
        List<Rule> rules = new List<Rule>();
        Debug.Log("GetRulesWithOutput - term: " + term.GetTermAsString());
        for (int i = 0; i < _assets.Count; i++) {
            if (_assets[i].outputs[0].name == term.name) {
                Rule ruleToAdd = ScriptableObject.Instantiate(_assets[i]) as Rule;
                rules.Add(ruleToAdd);
            }
        }
        foreach (Rule r in rules)
        {
            Debug.Log("term: " + term.GetTermAsString() + " rules: " + r.GetRuleAsString());
        }
        return rules;
    }
}
