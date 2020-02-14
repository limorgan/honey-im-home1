using UnityEngine;
using System.Collections;
using UnityEditor;

public class AreaUtility {
    [MenuItem("Assets/Create/Database/Area")]
    static public void CreateArea() {
        ScriptableObjectUtility.CreateAsset<Area>();
    }
}