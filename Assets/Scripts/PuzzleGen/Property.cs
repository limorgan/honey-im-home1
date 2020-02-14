using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PropertyType { StringProperty = 0, IntProperty = 1, BoolProperty = 2 };

[System.Serializable]
public class Property {
    public PropertyType type;
    public string name = "";
    public string value = "";

    public Property(PropertyType type) {
        this.type = type;
        if (type == PropertyType.IntProperty)
            value = "0";
    }

    public bool Equals(Property otherProperty) {
        return otherProperty.name == this.name && otherProperty.value == this.value;
    }
}


