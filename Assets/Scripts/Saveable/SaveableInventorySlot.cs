using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveableInventorySlot
{
    public int id;
    public int amount;

    public SaveableInventorySlot(int _id, int _amount)
    {
        id = _id;
        amount = _amount;
    }
}
