using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class ItemDatabase : Database<Item> {

    static protected string filePath = "Assets/Resources/DBItems/DBItem.asset";

    static public void CreateAsset() {
        CreateAsset(filePath);
    }

    static public Item GetObject(string itemName) {
        ValidateDatabase();
        foreach (Item item in _assets) {
            if (item.name == itemName)
                return ScriptableObject.Instantiate(item) as Item;
        }
        return null;
    }

    static public void Sort() {
        _assets.Sort((x, y) => x.name.CompareTo(y.name));
    }

    static public bool HasItemOfType(Term term, List<Area> accessibleAreas, List<Item> itemsInScene) {
        foreach (Item dbItem in _assets) {
            if ((dbItem.name == term.name || dbItem.GetSuperTypes().Contains(term.name)) && dbItem.IsAccessible(accessibleAreas, itemsInScene))
                return true;
        }
        return false;
    }

    static public List<Item> GetItemsOfType(string itemName, List<Area> accessibleAreas, List<Item> itemsInScene) {
        List<Item> matchingItems = new List<Item>();
        foreach (Item dbItem in _assets) {
            if(dbItem.name == itemName || dbItem.GetSuperTypes().Contains(itemName)){
                matchingItems.Add(dbItem);
            }
        }
        return matchingItems;
    }

    // Returning all matches
    static public List<Item> FindDBItemsFor(Term term, List<Area> accessibleAreas, List<Item> itemsInScene) {
        List<Item> matchingItems = new List<Item>();
        foreach(Item dbItem in _assets) {
            if (dbItem.name == "TaxiOrder")
                Debug.Log("checking: " + dbItem.name + " matches?" + dbItem.Matches(term) + " accessible: " + dbItem.IsAccessible(accessibleAreas, itemsInScene));
            if (dbItem.Matches(term) && dbItem.IsAccessible(accessibleAreas, itemsInScene)) {
                matchingItems.Add(ScriptableObject.Instantiate(dbItem) as Item);
            }
        }
        return matchingItems;
    }

}

