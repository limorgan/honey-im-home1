﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Text.RegularExpressions;

public class RuleDatabaseEditor : EditorWindow {

    private int _viewIndex = 0;
    private Vector2 _scrollPositionBottom = Vector2.zero;
    private List<bool> foldedOut = new List<bool>();
    private PropertyType _newPropertyType = PropertyType.StringProperty;    
    

    [MenuItem("Window/Rules Editor %#e")]
    static void Init() {
        EditorWindow.GetWindow(typeof(RuleDatabaseEditor));
    }

    void OnEnable() {
        RuleDatabase.LoadDatabase();
        for (int i = 0; i < RuleDatabase.GetNumOfAssets(); i++)
            foldedOut.Add(false);
    }

    void OnGUI() {
        

        GUILayout.BeginArea(new Rect(10, 10, 1200, 500), EditorStyles.helpBox);
        _scrollPositionBottom = EditorGUILayout.BeginScrollView(_scrollPositionBottom, true, true);

        EditorGUILayout.LabelField("Rules:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(" -S: second input item doesn't need to be selected. -I: main output goes straight to inventory. #Rules: " + RuleDatabase.GetNumOfAssets()); // +
            //"-A: rule is executed automatically. ");
        if (GUILayout.Button("Add New Rule", GUILayout.ExpandWidth(false))) {
            foldedOut.Add(false);
            RuleDatabase.CreateAsset();
        }



        for (int ruleIndx = 0; ruleIndx < RuleDatabase.GetNumOfAssets(); ruleIndx++) {
            Rule rule = RuleDatabase.GetAsset(ruleIndx);
            EditorUtility.SetDirty(rule);
            string additionalProperties = "";       //Addition - indicating Rule properties: 
            if (rule.selectedInput)
                additionalProperties += " -S ";     //Second input item does not need to be selected
            if (rule.inventory)
                additionalProperties += " -I ";     //First output item is spawned directly in the inventory
            /*if (rule.automatic)
                additionalProperties += " -A ";  */   //Such rules are excuted automatically
            if (rule.ruleNumber != GetFileNameShort(rule))
                rule.ruleNumber = GetFileNameShort(rule);
            string ruleNumber = "#" + rule.ruleNumber + ": ";
            foldedOut[ruleIndx] = EditorGUILayout.Foldout(foldedOut[ruleIndx], ruleNumber + rule.GetRuleAsString() + additionalProperties);

            if (foldedOut[ruleIndx]) {
                // === OUTPUT ===
                EditorGUILayout.LabelField("Output:", EditorStyles.boldLabel);

                for (int outputIndx = 0; outputIndx < rule.outputs.Count; outputIndx++) {
                    GUILayout.BeginHorizontal();
                    Term output = rule.outputs[outputIndx];
                    output.name = EditorGUILayout.TextArea(output.name, GUILayout.Width(100));
                    if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) {
                        rule.DeleteOutputAtIndex(outputIndx);
                    }
                    GUILayout.BeginVertical();
                    for (int propIndx = 0; propIndx < output.properties.Count; propIndx++) {
                        GUILayout.BeginHorizontal();
                        PropertyGUI(output.properties[propIndx]);
                        if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) {
                            output.DeleteProperty(propIndx);
                        }
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Add:", GUILayout.ExpandWidth(false))) {
                        output.AddPropertyOfType(_newPropertyType);
                    }
                    _newPropertyType = (PropertyType)EditorGUILayout.EnumPopup(_newPropertyType, GUILayout.ExpandWidth(false));
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                    GUILayout.EndHorizontal();
                }

                if (GUILayout.Button("+", GUILayout.ExpandWidth(false))) {
                    rule.AddOutput();
                }

                // === ACTION ===
                EditorGUILayout.LabelField("Action:", EditorStyles.boldLabel);
                rule.action = EditorGUILayout.TextArea(rule.action);

                // === INPUT ===
                EditorGUILayout.LabelField("Input:", EditorStyles.boldLabel);

                for (int inputIndx = 0; inputIndx < rule.inputs.Count; inputIndx++) {
                    GUILayout.BeginHorizontal();
                    Term input = rule.inputs[inputIndx];
                    input.name = EditorGUILayout.TextArea(input.name, GUILayout.Width(100));
                    if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) {
                        rule.DeleteInputAtIndex(inputIndx);
                    }

                    GUILayout.BeginVertical();
                    for (int propIndx = 0; propIndx < input.properties.Count; propIndx++) {
                        GUILayout.BeginHorizontal();
                        PropertyGUI(input.properties[propIndx]);
                        if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) {
                            input.DeleteProperty(propIndx);
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Add:", GUILayout.ExpandWidth(false))) {
                        input.AddPropertyOfType(_newPropertyType);
                    }
                    _newPropertyType = (PropertyType)EditorGUILayout.EnumPopup(_newPropertyType, GUILayout.ExpandWidth(false));
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                    GUILayout.EndHorizontal();
                }


                if (GUILayout.Button("+", GUILayout.ExpandWidth(false))) {
                    rule.AddInput();
                }


                // === InIventory ===                   (addition)
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Input doesn't need to be selected:", EditorStyles.boldLabel);
                rule.selectedInput = EditorGUILayout.Toggle(rule.selectedInput);
                EditorGUILayout.EndHorizontal();

                // === Straight to Inventory ===        (addition)
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("First output goes straight to inventory:", EditorStyles.boldLabel);
                rule.inventory = EditorGUILayout.Toggle(rule.inventory);
                EditorGUILayout.EndHorizontal();

                /*// === Automatic Rule ===        (addition)       This is a starting point for including rules which can be executed without player interaction
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Automatically executed rule:", EditorStyles.boldLabel);
                rule.automatic = EditorGUILayout.Toggle(rule.automatic);
                EditorGUILayout.EndHorizontal();
                */
                if (GUILayout.Button("Delete Rule", GUILayout.ExpandWidth(false))) {
                    RuleDatabase.DeleteAsset(ruleIndx);
                }
            }
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void PropertyGUI(Property property) {
        property.name = EditorGUILayout.TextField(property.name, GUILayout.MaxWidth(100));
        if (property.name != null) property.name.ToLower();

        if (property.type == PropertyType.StringProperty) {
            property.value = EditorGUILayout.TextField(property.value, GUILayout.MinWidth(100), GUILayout.MaxWidth(300));
        }
        else if (property.type == PropertyType.IntProperty) {
            property.value = EditorGUILayout.IntField(int.Parse(property.value), GUILayout.MinWidth(100), GUILayout.MaxWidth(300)).ToString();
        }
        else if (property.type == PropertyType.BoolProperty) {
            property.value = EditorGUILayout.Toggle(property.value == "True", GUILayout.MinWidth(100), GUILayout.MaxWidth(300)).ToString();
        }
    }

    public string GetFileNameShort(Rule rule)
    {
        string fullPath = GetFileName(rule);
        Regex reg = new Regex(@"\d+");
        Match match = reg.Match(fullPath);
        if (match.Success)
            return match.Value;
        else
            return "0";
    }

    public string GetFileName(Rule rule)
    {
        return RuleDatabase.GetPath(rule);
    }

    public void TestRegex()
    {
        
    }
}
 