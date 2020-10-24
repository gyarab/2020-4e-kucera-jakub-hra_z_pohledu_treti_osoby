using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveableObject
{
    public int itemID;
    public Vector3 pos;

    public SaveableObject(int _itemID, Vector3 _pos)
    {
        itemID = _itemID;
        pos = _pos;
    }
}
