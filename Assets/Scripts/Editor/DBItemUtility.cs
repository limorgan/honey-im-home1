using UnityEngine;
using System.Collections;
using UnityEditor;

public class DBItemUtility {
	[MenuItem("Assets/Create/Database/Item")]
	static public void CreateItem(){
		ScriptableObjectUtility.CreateAsset<Item> ();
	}
}
