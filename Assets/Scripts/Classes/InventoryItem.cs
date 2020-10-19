using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public ItemObject itemObject;
    public int amount;

    public InventoryItem(ItemObject _itemObject, int _amount)
    {
        itemObject = _itemObject;
        amount = _amount;
    }
}
