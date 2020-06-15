using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class AreaDatabaseEditor : EditorWindow {

    private int _viewIndex = 0;
    private Vector2 _scrollPositionLeft = Vector2.zero;
    private Vector2 _scrollPositionRight = Vector2.zero;
    private PropertyType _newPropertyType = PropertyType.StringProperty;

    [MenuItem("Window/Area Editor %#e")]
    static void Init() {
        EditorWindow.GetWindow(typeof(AreaDatabaseEditor));
    }

    void OnEnable() {
        AreaDatabase.LoadDatabase();
    }

    void OnGUI() {
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Areas Editor", EditorStyles.boldLabel);
        GUIStyle wordWrapStyle = new GUIStyle(EditorStyles.textArea);
        wordWrapStyle.wordWrap = true;

        if (GUILayout.Button("Add Area")) {
            AreaDatabase.CreateAsset();
        }
        if (GUILayout.Button("Delete Selected Area")) {
            AreaDatabase.DeleteAsset(_viewIndex);
            _viewIndex = (_viewIndex == 0 ? _viewIndex++ : _viewIndex--);
        }
        if (GUILayout.Button("Reload")) {
            AreaDatabase.LoadDatabaseForce();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        GUILayout.BeginArea(new Rect(10, 70, 140, 350));
        _scrollPositionLeft = EditorGUILayout.BeginScrollView(_scrollPositionLeft, false, true);
        if (AreaDatabase.GetNumOfAssets() > 0) {
            for (int areaIndx = 0; areaIndx < AreaDatabase.GetNumOfAssets(); areaIndx++) {
                Area area = AreaDatabase.GetObject(areaIndx);
                if (GUILayout.Button(area.name, "label" /*+ (itemInd == _viewIndex ? "active" : "")*/)) {
                    _viewIndex = areaIndx;
                    GUIUtility.keyboardControl = 0;
                }
            }
        }
        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(170, 70, 500, 350), EditorStyles.helpBox);
        _scrollPositionRight = EditorGUILayout.BeginScrollView(_scrollPositionRight);
        if (AreaDatabase.GetNumOfAssets() > 0) {
            Area asset = AreaDatabase.GetAsset(_viewIndex);
            if (asset != null) {
                EditorUtility.SetDirty(asset);
                GUILayout.Space(5);
                asset.name = EditorGUILayout.TextField("Area Name", asset.name, GUILayout.MaxWidth(300));

                GUILayout.Space(10);
                EditorGUILayout.LabelField("Goals:", EditorStyles.boldLabel);
                for(int goalIndx = 0; goalIndx < asset.goals.Count; goalIndx++) {
                    Term goal = asset.goals[goalIndx];
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Goal" + goalIndx + ":" + goal.GetTermAsString());
                    if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) {
                        asset.DeleteGoal(goalIndx);
                    }
                    GUILayout.EndHorizontal();
                    EditorGUI.indentLevel++;
                    goal.name = EditorGUILayout.TextField("Goal Name", goal.name, GUILayout.MaxWidth(300));
                    
                    EditorGUILayout.LabelField("Objective:", EditorStyles.label);
                    EditorStyles.textArea.wordWrap = true;
                    goal.description = EditorGUILayout.TextArea(goal.description, wordWrapStyle);

                    EditorGUILayout.LabelField("Hint:", EditorStyles.label);
                    goal.hint = EditorGUILayout.TextArea(goal.hint, wordWrapStyle);


                    EditorGUILayout.LabelField("Properties:", EditorStyles.boldLabel);
                    for (int propIndx = 0; propIndx < goal.properties.Count; propIndx++) {
                        Property property = goal.properties[propIndx];
                        GUILayout.BeginHorizontal();
                        PropertyGUI(property);
                        GUILayout.Space(3);
                        if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) {
                            goal.DeleteProperty(propIndx);
                        }
                        GUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    if (GUILayout.Button("Add:", GUILayout.ExpandWidth(false))) {
                        goal.AddPropertyOfType(_newPropertyType);
                    }
                    _newPropertyType = (PropertyType)EditorGUILayout.EnumPopup(_newPropertyType, GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);
                }
                GUILayout.Space(3);
                if(GUILayout.Button("Add Goal", GUILayout.ExpandWidth(false))) {
                    asset.AddGoal();
                }
                GUILayout.Space(10);
                EditorGUILayout.LabelField("Connected Areas:", EditorStyles.boldLabel);
                for(int connectedId = 0; connectedId < asset.connectedTo.Count; connectedId++) {
                    GUILayout.BeginHorizontal();
                    asset.connectedTo[connectedId] = (Area)EditorGUILayout.ObjectField(asset.connectedTo[connectedId], typeof(Area), false);
                    if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) {
                        asset.DeleteConnectedArea(connectedId);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(5);
                if (GUILayout.Button("Add Connected Area", GUILayout.ExpandWidth(false))) {
                    asset.AddConnectedArea();
                }
                GUILayout.Space(5);
                //Addition 10/01/2020
                EditorGUILayout.LabelField("MaxDepth: ", EditorStyles.boldLabel);
                //end add
                asset.maxDepth = EditorGUILayout.IntField(asset.maxDepth, GUILayout.MinWidth(100), GUILayout.MaxWidth(300));
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
}
