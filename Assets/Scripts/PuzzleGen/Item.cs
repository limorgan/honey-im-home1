using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Item : ScriptableObject{
	public new string name;
	public List<Property> properties;
    public GameObject itemPrefab;
    public string description;

    public Item() {
        if (properties == null)
            properties = new List<Property>();
        name = "NewItem";
    }

    public override bool Equals(object o) {
        return ((Item)o).name == this.name;
    }

    public bool PropertyExists(string propertyName) {
		foreach (Property prop in properties) {
			if (prop.name == propertyName)
				return true;
		}
		return false;
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

    public bool HasProperty(Property propertyToCheck) {
        foreach(Property property in this.properties) {
            if (property.Equals(propertyToCheck))
                return true;
        }
        return false;
    }

    public void AddPropertyOfType(PropertyType type) {
        properties.Add(new Property(type));
    }

	public void DeleteProperty(int index) {
		properties.RemoveAt (index);
	}

    public List<string> GetSuperTypes() {
        List<string> types = new List<string>();
        types.Add("Item");
        foreach (Property property in this.GetPropertiesWithName("isa")) {
            types.Add(property.value);
        }
        return types;
    }

    //DBItem must match all types, could have more properties than specified by Item
    public bool Matches(Term term) {
        if (term.name != this.name) {
            //Check type match with db item super types
            bool found = false;
            foreach(string type in this.GetSuperTypes()) {
                if (type == term.name)
                    found = true;
            }
            if (!found) return false;
        }
        foreach (Property property in term.properties) {
            if (!this.HasProperty(property)) {
                return false;
            }
        }
        return true;
    }

    public bool IsAccessible(List<Area> areas, List<Item> itemsInScene) {
        List<Property> areaProperties = this.GetPropertiesWithName("area");
        if (areaProperties.Count == 0) {
            if (IsSpawnable() || itemsInScene.Contains(this))
                return true;
            else
                return false;
        }
        foreach(Property areaProp in areaProperties) {
            foreach(Area area in areas) {
                if (areaProp.value == area.name && (IsSpawnable() || itemsInScene.Contains(this)))
                    return true;
            }
        }
        return false;
    }

    public bool IsCarryable() {
        Property carryable = GetPropertyWithName("carryable");
        if (carryable == null) return false;
        if (carryable.value == "False") return false;
        return true;
    }

    public bool IsCopyable()
    {
        Property copyable = GetPropertyWithName("copyable");
        if (copyable == null) return false;
        if (copyable.value == "False") return false;
        return true;
    }

    // Assume default: spawnable = true
    public bool IsSpawnable() {
        if (GetPropertyWithName("spawnable") == null)
            return true;
        return false;
    }

    public string toString()
    {
        return name;
    }
}

