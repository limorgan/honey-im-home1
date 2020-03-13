using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class ItemDatabaseEditor : EditorWindow {

	private int _viewIndex = 0;
	private Vector2 _scrollPositionLeft = Vector2.zero;
    private Vector2 _scrollPositionRight = Vector2.zero;
    private List<bool> foldedOut = new List<bool>();
    private PropertyType _newPropertyType = PropertyType.StringProperty;

	[MenuItem ("Window/Item Editor %#e")]
	static void Init() {
		EditorWindow.GetWindow (typeof(ItemDatabaseEditor));
	}

	void OnEnable() {
		ItemDatabase.LoadDatabase ();
        ItemDatabase.Sort();
	}

    void OnGUI() {
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Item Database Editor", EditorStyles.boldLabel);
        GUIStyle wordWrapStyle = new GUIStyle(EditorStyles.textArea);
        wordWrapStyle.wordWrap = true;

        if (GUILayout.Button("Add Item")) {
            ItemDatabase.CreateAsset();
        }
        if (GUILayout.Button("Delete Selected Item")) {
            ItemDatabase.DeleteAsset(_viewIndex);
            _viewIndex = (_viewIndex == 0 ? _viewIndex++ : _viewIndex--);
        }
        if (GUILayout.Button("Reload")) {
            ItemDatabase.LoadDatabaseForce();
        }
        if (GUILayout.Button("Sort")) {
            ItemDatabase.Sort();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        GUILayout.BeginArea(new Rect(10, 70, 140, 450));
        _scrollPositionLeft = EditorGUILayout.BeginScrollView(_scrollPositionLeft, false, true);
        if (ItemDatabase.GetNumOfAssets() > 0) {
            for (int itemInd = 0; itemInd < ItemDatabase.GetNumOfAssets(); itemInd++) {
                Item item = ItemDatabase.GetAsset(itemInd);
                if (GUILayout.Button(item.name, "label" /*+ (itemInd == _viewIndex ? "active" : "")*/)) {
                    _viewIndex = itemInd;
                    GUIUtility.keyboardControl = 0;
                }
            }
        }
        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(170, 70, 500, 450), EditorStyles.helpBox);
        _scrollPositionRight = EditorGUILayout.BeginScrollView(_scrollPositionRight);
        if (ItemDatabase.GetNumOfAssets() > 0) {
            Item dbAsset = ItemDatabase.GetAsset(_viewIndex);
            if (dbAsset != null) {
                EditorUtility.SetDirty(dbAsset);
                GUILayout.Space(5);
                dbAsset.name = EditorGUILayout.TextField("Item Name", dbAsset.name, GUILayout.MaxWidth(300));

                GUILayout.Space(10);
                EditorGUILayout.LabelField("Properties:", EditorStyles.boldLabel);
                for (int i = 0; i < dbAsset.properties.Count; i++) {
                    Property property = dbAsset.properties[i];
                    GUILayout.BeginHorizontal();

                    PropertyGUI(property);

                    GUILayout.Space(3);
                    if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) {
                        dbAsset.DeleteProperty(i);
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Add:", GUILayout.ExpandWidth(false))) {
                    dbAsset.AddPropertyOfType(_newPropertyType);
                }
                _newPropertyType = (PropertyType)EditorGUILayout.EnumPopup(_newPropertyType, GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
                EditorGUILayout.LabelField("Description:", EditorStyles.boldLabel);
                dbAsset.description = EditorGUILayout.TextArea(dbAsset.description, GUILayout.MaxWidth(400));
                GUILayout.Space(10);

                //"Inspect" option:                 
                if (dbAsset.GetPropertyWithName("inspectable") != null)
                    if (dbAsset.GetPropertyWithName("inspectable").value == "True")
                    {
                        EditorGUILayout.LabelField("Long description: ", EditorStyles.boldLabel);
                        //GUILayout.BeginArea(new Rect(50, 50, 200, 200));
                        EditorStyles.textArea.wordWrap = true;
                        dbAsset.longDescription = EditorGUILayout.TextArea(dbAsset.longDescription, wordWrapStyle);
                        //GUILayout.EndArea();
                    }

                GUILayout.Space(10);
                EditorGUILayout.LabelField("Prefab:", EditorStyles.boldLabel);
                dbAsset.itemPrefab = (GameObject)EditorGUILayout.ObjectField(dbAsset.itemPrefab, typeof(GameObject), false);

                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Specific Spawn Points: ", EditorStyles.boldLabel);
                dbAsset.specificSpawnPoints = EditorGUILayout.Toggle(dbAsset.specificSpawnPoints);
                GUILayout.EndHorizontal();

                /*if (dbAsset.specificSpawnPoints)
                {
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Specific Spawn Points: ", EditorStyles.boldLabel);
                    dbAsset.spawnLength = EditorGUILayout.IntField(dbAsset.spawnLength);
                    if(dbAsset.spawnLength > dbAsset.spawnPoints.Count)
                        for (int i = 0; i < dbAsset.spawnLength; i++)
                            dbAsset.spawnPoints.Add(new Vector3(0,0,0));
                    GUILayout.EndHorizontal();

                    for (int i = 0; i < dbAsset.spawnLength; i++)
                    {
                        GUILayout.BeginHorizontal();

                        dbAsset.spawnPoints[i] = EditorGUILayout.Vector3Field("Spawn Coordinates " + i, dbAsset.spawnPoints[i]);

                        GUILayout.Space(3);
                        if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
                        {
                            dbAsset.spawnPoints.RemoveAt(i);
                        }
                        GUILayout.EndHorizontal();
                    }
                }*/
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
