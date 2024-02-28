using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public List<CubeData> cubes = new List<CubeData>();
}

// Because Unity's Vector3, Quaternion and Color structs are not marked as [Serializable] they can't be serialized into JSON.
// For that we create serializable versions of each one. There can be other approaches depending on how you serialize/deserialize the data.
[Serializable]
public class CubeData
{
    public SerializableVector3 position;
    public SerializableQuaternion rotation;
    public SerializableColor color;

    public CubeData() {}

    public CubeData(Vector3 position, Quaternion rotation, Color color)
    {
        this.position = new SerializableVector3(position);
        this.rotation = new SerializableQuaternion(rotation);
        this.color = new SerializableColor(color);
    }
}

[Serializable]
public class SerializableVector3
{
    public float x, y, z;
    
    public SerializableVector3() {}

    public SerializableVector3(Vector3 sourceVector)
    {
        x = sourceVector.x;
        y = sourceVector.y;
        z = sourceVector.z;
    }

    public Vector3 ToVector3() => new Vector3(x, y, z);
}

[Serializable]
public class SerializableQuaternion
{
    public float x, y, z, w;
    
    public SerializableQuaternion() {}

    public SerializableQuaternion(Quaternion sourceQuaternion)
    {
        x = sourceQuaternion.x;
        y = sourceQuaternion.y;
        z = sourceQuaternion.z;
        w = sourceQuaternion.w;
    }
    
    public Quaternion ToQuaternion() => new Quaternion(x, y, z, w);
}

[Serializable]
public class SerializableColor
{
    public float r, g, b, a;
    
    public SerializableColor() {}

    public SerializableColor(Color sourceColor)
    {
        r = sourceColor.r;
        g = sourceColor.g;
        b = sourceColor.b;
        a = sourceColor.a;
    }
    
    public Color ToColor() => new Color(r, g, b, a);
}