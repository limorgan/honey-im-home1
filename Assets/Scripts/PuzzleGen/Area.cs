using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Area : ScriptableObject {
    public new string name;
    public List<Term> goals;
    public List<Area> connectedTo;
    public Transform inGameArea;
    public int maxDepth;

    public Area() {
        goals = new List<Term>();
        connectedTo = new List<Area>();
        name = "NewArea";
    }

    public void AddGoal() {
        goals.Add(new Term());
    }

    public void DeleteGoal(int index) {
        goals.RemoveAt(index);
    }

    public void AddConnectedArea() {
        connectedTo.Add(null);
    }

    public void DeleteConnectedArea(int index) {
        connectedTo.RemoveAt(index);
    }

    public string toString()
    {
        string s = name + " Terms: ";
        foreach (Term g in goals)
            s += g.GetTermAsString();
        s += " Connected Areas : ";
        foreach (Area c in connectedTo)
            s += c.toString();
        return s;
    }
}
