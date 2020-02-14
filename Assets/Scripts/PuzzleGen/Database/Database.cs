using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class Database<T> where T : ScriptableObject {

    static protected List<T> _assets;
    static private bool _isDatabaseLoaded = false;

    static protected void ValidateDatabase() {
        if (_assets == null)
            _assets = new List<T>();
        if (!_isDatabaseLoaded)
            LoadDatabase();
    }

    static public void LoadDatabase() {
        if (_isDatabaseLoaded)
            return;
        _isDatabaseLoaded = true;
        LoadDatabaseForce();
    }

    static public void LoadDatabaseForce() {
        ValidateDatabase();
        T[] resources = Resources.LoadAll<T>("");
        foreach (T item in resources) {
            if (!_assets.Contains(item))
                _assets.Add(item);
        }
    }

    static public List<T> GetAllAssets() {
        return _assets;
    }

    static public void ClearDatabase() {
        _isDatabaseLoaded = false;
        _assets.Clear();
    }

    static public void CreateAsset(string filePath) {
        T asset = ScriptableObject.CreateInstance<T>();
        string path = AssetDatabase.GenerateUniqueAssetPath(filePath);
        AssetDatabase.CreateAsset(asset, path);
        _assets.Add(asset);
    }

    static public void DeleteAsset(int index) {
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(_assets[index]));
        _assets.RemoveAt(index);
    }

    static public int GetNumOfAssets() {
        return _assets.Count;
    }

    static public T GetAsset(int index) {
        ValidateDatabase();
        if (_assets.Count <= index)
            return null;
        return _assets[index];
    }

    static public T GetObject(int index) {
        if (_assets.Count <= index)
            return null;
        return ScriptableObject.Instantiate(_assets[index]) as T;
    }

    static public List<T> GetAllObjects() {
        LoadDatabase();
        List<T> objects = new List<T>();
        foreach (T asset in _assets) {
            objects.Add(ScriptableObject.Instantiate(asset) as T);
        }
        return objects;
    }
}
