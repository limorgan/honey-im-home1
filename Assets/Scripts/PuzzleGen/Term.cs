using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Term {
    public string name;
    public List<Property> properties;
    public Item dbItem;
    public GameItem gameItem;
    public string description;

    public Term() {
        properties = new List<Property>();
    }

    public Term(string name) {
        properties = new List<Property>();
        this.name = name;
    }

    // make copy of term
    public Term(Term term, string name) {
        this.properties = term.properties;
        this.name = name;
    }

    public void AddPropertyOfType(PropertyType type) {
        properties.Add(new Property(type));
    }

    public void DeleteProperty(int index) {
        properties.RemoveAt(index);
    }

    public List<Property> GetPropertiesWithName(string propertyName) {
        List<Property> propertiesToReturn = new List<Property>();
        foreach (Property property in this.properties) {
            if (property.name == propertyName)
                propertiesToReturn.Add(property);
        }
        return propertiesToReturn;
    }

    public Property GetPropertyWithName(string propertyName) {
        foreach (Property property in this.properties) {
            if (property.name == propertyName)
                return property;
        }
        return null;
    }

    // If term name exists as DBItem name, get it's isa properties, otherwise its already a super type
    public List<string> GetSuperTypes() {
        Item dbItemMatch = ItemDatabase.GetObject(this.name);
        if (dbItemMatch != null)
            return dbItemMatch.GetSuperTypes();
        List<string> types = new List<string>();
        types.Add("Item");
        return types;
    }

    public string GetTermAsString() {
        string termAsString = "";
        termAsString += this.name;
        if (this.properties.Count > 0) {
            termAsString += "[";
            for (int i = 0; i < this.properties.Count; i++) {
                termAsString += this.properties[i].name + ":";
                termAsString += this.properties[i].value.ToString();
                if (i < this.properties.Count - 1)
                    termAsString += ", ";
            }
            termAsString += "] ";
        }
        if (dbItem != null)
            termAsString += dbItem.toString();
        else
            termAsString += " null db item ";
        return termAsString;
    }

    public string toString()
    {
        return this.GetTermAsString();
    }
}
