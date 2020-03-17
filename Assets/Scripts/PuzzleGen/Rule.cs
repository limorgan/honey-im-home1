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
    public bool reversible; // 06/03 order of input items does not matter
    public bool selectedInput;  // 10/03 second input term (i.e. usually the item in inventory, never the clicked item), doesn't need to be selected if true
    public bool inventory;  // main output item goes straight to inventory

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

        /*if(this.outputs[0].properties.Count != term.properties.Count) {       //this prevents things such as car and car[ready:false] from working.
            return false;                                                       //the default for a property that isn't there is false. 
        }*/                                                                     //

        /*foreach (Property termProperty in term.properties)                      //the 
        {
            bool found = false;
            foreach (Property ruleOutputProp in this.outputs[0].properties)
            {
                if (termProperty.Equals(ruleOutputProp))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                if (this.outputs[0].GetPropertyWithName(termProperty.name) == null && termProperty.value == "False")
                    found = true;
                else
                    return false;
            }
        }*/

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
                /*else if (ruleOutputProp.name != "contains")
                {
                    if (term.dbItem != null)
                    {
                        if (term.dbItem.GetPropertyWithName(ruleOutputProp.name) != null)
                        {
                            if (term.dbItem.GetPropertyWithName(ruleOutputProp.name).value != "True")
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                    if (term.GetPropertyWithName(ruleOutputProp.name) == null && ruleOutputProp.value == "False")
                    {
                        found = true;
                        break;
                    }
                }*/
            }
            if (!found)
            {
                Debug.Log("Output property " + ruleOutputProp.name + " was not matched.");
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

    public string GetRuleAsString() {
        string ruleAsString = "";
        foreach (Term output in outputs) {
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

    public List<Rule> InputPermutation()        // 06/03 gives all permutations of the input terms
    {
        List<Rule> permutations = new List<Rule>();
        if (inputs.Count <= 1)
            permutations.Add(this);
        else
        {
            List<List<Term>> inputPermutations = InputPermutation(this.inputs, 0, this.inputs.Count - 1);
            Rule modRule = this;
            foreach (List<Term> modInput in inputPermutations)
            {
                modRule.inputs = modInput;
                permutations.Add(modRule);
            }
        }
        return permutations;
    }

    public List<List<Term>> InputPermutation(List<Term> input, int i, int j)
    {
        List<List<Term>> permutations = new List<List<Term>>();
        if (i == j)
        {
            permutations.Add(input);
            return permutations;
        }
        else
        {
            for(int k = i; k <= j; k++)
            {
                input = SwapInputTerms(input, i, k);
                InputPermutation(input, i + 1, j);
                input = SwapInputTerms(input, i, k);
            }
        }
        return null;
    }

    public List<Term> SwapInputTerms(List<Term> input, int i, int j)
    {
        Term temp = input[i];
        input.Insert(i, input[j]);
        input.Insert(j, temp);
        return input;
    }
    

    public string ToString()
    {
        return GetRuleAsString();
    }
}
