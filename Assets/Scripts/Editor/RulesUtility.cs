using UnityEngine;
using System.Collections;
using UnityEditor;

public class RulesUtility {
	[MenuItem("Assets/Create/Database/Rule")]
	static public void CreateRule(){
		ScriptableObjectUtility.CreateAsset<Rule> ();
	}
}
