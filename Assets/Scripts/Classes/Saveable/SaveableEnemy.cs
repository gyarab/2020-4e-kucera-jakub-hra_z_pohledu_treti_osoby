using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveableEnemy
{
    public string prefabName;
    public int waypointID;
    public Vector3 pos;
    public float rotationY;

    public SaveableEnemy(string _prefabName, int _waypointID, Vector3 _pos, float _rotationY)
    {
        prefabName = _prefabName;
        waypointID = _waypointID;
        pos = _pos;
        rotationY = _rotationY;
    }
}
