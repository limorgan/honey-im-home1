using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[System.Serializable]
public class Rule : ScriptableObject {
    public List<Term> outputs;
    public List<Term> inputs;
    public string action;
    public string hint;
    public Rule parent;
    public List<Rule> children;
    public bool automatic; // Addition: automatically excuted rules
    public bool selectedInput;  // 10/03 second input term (i.e. usually the item in inventory, never the clicked item), doesn't need to be selected if true
    public bool inventory;  // main output item goes straight to inventory
    public string ruleNumber;

    public Rule(){
        outputs = new List<Term>();
        inputs = new List<Term>();
        children = new List<Rule>();

        inputs.Add(new Term("input"));
        outputs.Add(new Term("output"));
    }

    public Rule(string action) {
        this.action = action;
    }

    public override bool Equals(object o) {
        return ((Rule)o).name == this.name;
    }

    public void AddOutput() {
        outputs.Add(new Term("output"));
    }

    public void AddInput() {
        inputs.Add(new Term("input"));
    }

    public void DeleteOutputAtIndex(int index) {
        outputs.RemoveAt(index);
    }

    public void DeleteInputAtIndex(int index) {
        inputs.RemoveAt(index);
    }

    public void AddChildRule(Rule child) {
        if (children == null)
            children = new List<Rule>();
        children.Add(child);
    }

    public void RemoveLastAddedRule() {
        if (children.Count > 0)
            children.RemoveAt(children.Count - 1);
    }

    //Type or super-type and all properties should match
    public bool MainOutputIs(Term term) {
        if (term.dbItem != null) {
            if (term.dbItem.name != this.outputs[0].name) {
                if (!term.dbItem.GetSuperTypes().Contains(this.outputs[0].name)) {
                    return false;
                }
            }
        } else {
            if (term.name != this.outputs[0].name) {
                if (!term.GetSuperTypes().Contains(this.outputs[0].name)){// && !this.outputs[0].GetSuperTypes().Contains(term.name)) {
                    return false;
                }
            }
        }

        foreach (Property ruleOutputProp in this.outputs[0].properties)
        {       //Original
            bool found = false;
            foreach (Property termProperty in term.properties)
            {
                if (termProperty.Equals(ruleOutputProp))
                {
                    found = true;
                    break;
                }
                
            }
            if (!found)
            {
                return false;
            }
        }
        return true;
    }

    public bool HasPlayerInput()
    {
        foreach (Term t in inputs)
        {
            if (t.name == "Player")
                return true;
        }
        return false;
    }

    public string GetRuleAsString()
    {
        return GetRuleAsString(true);
    }

    public string GetRuleAsString(bool speech) {
        string ruleAsString = "";
        foreach (Term output in outputs) {

            if (output.name == "Speech" && !speech)
                continue;

            ruleAsString += output.name;
            if (output.properties.Count > 0) {
                ruleAsString += "[";
                for(int i = 0; i< output.properties.Count; i++) {
                    ruleAsString += output.properties[i].name + ":";
                    ruleAsString += output.properties[i].value.ToString();
                    if (i < output.properties.Count - 1)
                        ruleAsString += ", ";
                }
                ruleAsString += "] ";
            } else
                ruleAsString += " ";

        }
        ruleAsString += "--> " + action + " ";
        foreach (Term input in inputs) {
            ruleAsString += input.name;
            if (input.properties.Count > 0) {
                ruleAsString += "[";
                for (int i = 0; i < input.properties.Count; i++) {
                    ruleAsString += input.properties[i].name + ":";
                    ruleAsString += input.properties[i].value.ToString();
                    if (i < input.properties.Count - 1)
                        ruleAsString += ", ";
                }
                ruleAsString += "] ";
            } else
                ruleAsString += " ";
        }
        return ruleAsString;
    }

    public string ToString()
    {
        return GetRuleAsString();
    }

    public string ToShortString()
    {
        return GetRuleAsString(false);
    }

    public bool ContainsItem(Item item)
    {
        string itemName = item.name;
        foreach (Term t in inputs)
        {
            if (t.name == itemName)
                return true;
        }
        foreach (Term t in outputs)
        {
            if (t.name == itemName)
                return true;
        }
        return false;
    }
}
