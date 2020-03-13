using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class AreaDatabase : Database<Area> {

    static protected string filePath = "Assets/Resources/Areas/Area.asset";

    static public void CreateAsset() {
        CreateAsset(filePath);
    }


}
