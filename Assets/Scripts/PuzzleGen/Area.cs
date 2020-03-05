using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Area : ScriptableObject {
    public new string name;
    public List<Term> goals;
    public List<Area> connectedTo;
    public Transform inGameArea;
    public int maxDepth;

    private Term currentGoal;        //05/03 added to make hint system 

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

    public void setCurrentGoal(Term goal)
    {
        currentGoal = goal;
    }

    public Term getCurrentGoal()
    {
        return currentGoal;
    }

    public string getHint()
    {
        return currentGoal.description;
    }

    public string toString()
    {
        string s = name + " Terms: ";
        foreach (Term g in goals)
            s += g.GetTermAsString();
        s += " Connected Areas : ";
        foreach (Area c in connectedTo)
            s += c.name;
        return s;
    }
}
