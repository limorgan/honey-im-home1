using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Item : ScriptableObject{
	public new string name;
	public List<Property> properties;
    public GameObject itemPrefab;
    public string description = "";
    public string longDescription;
    //public List<Vector3> spawnPoints = new List<Vector3>();
    public bool specificSpawnPoints;
    //public int spawnLength;
    //private int _index;

    public void Start()
    {
        //_index = Random.Range(0, spawnPoints.Count);
    }

    public Item() {
        if (properties == null)
            properties = new List<Property>();

        Property inspectable = new Property(PropertyType.BoolProperty);         // 07/03 added inspectable as a default property...
        properties.Add(inspectable);

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

    public bool IsTypeOf(Term term)
    {
        if (term.name != this.name)
        {
            //Check type match with db item super types
            bool found = false;
            foreach (string type in this.GetSuperTypes())
            {
                if (this.name == "Flower")
                    Debug.Log("Flower: supertype: " + type + " checking against " + term.name);
                if (type == term.name)
                    found = true;
            }
            if (!found) return false;
        }
        return true;    //case where names are equal
    }

    public bool IsAccessible(List<Area> areas, List<Item> itemsInScene) {
        if (name == "Player")       //Player is always accessible
            return true;
        /*if (this.name == "Car")
            Debug.Log("Car: spawnable " + IsSpawnable() + " contained in scene " + itemsInScene.Contains(this)); */
        List<Property> areaProperties = this.GetPropertiesWithName("area");
        if (areaProperties.Count == 0) {
            if (IsSpawnable() || itemsInScene.Contains(this))
                return true;
            else if (GetPropertyWithName("inInventory") != null && GetPropertyWithName("inInventory").value == "True")
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
        if (GetPropertyWithName("spawnable").value == "True")
            return true;
        return false;
    }

    public bool IsInspectable()
    {
        if (GetPropertyWithName("inspectable") != null && GetPropertyWithName("inspectable").value == "True")
            return true;
        else
            return false;
    }

    public override string ToString()
    {
        return name;
    }

    /*public Vector3 GetNextSpawnPt()
    {
        Debug.Log("item: " + name + "spawnlength: " + spawnLength + " length of list: " + spawnPoints.Count);
        _index = Random.Range(0, spawnLength);
        if (spawnLength == 0)
            Debug.Log("no spawn points");
        _index++;
        if (_index > spawnLength - 1)
            _index = 0;
        Debug.Log("Spawn point " + _index + ": " + spawnPoints[_index]);
        return spawnPoints[_index];
    }*/
}

