using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveableInventory
{
    public List<SaveableInventorySlot> savedItems;

    public SaveableInventory()
    {
        savedItems = new List<SaveableInventorySlot>();
    }

    public SaveableInventory(List<InventoryItem> items)
    {
        savedItems = new List<SaveableInventorySlot>();

        foreach (InventoryItem item in items) // TODO add to constructor?
        {
            savedItems.Add(new SaveableInventorySlot(item.itemObject.id, item.amount));
        }
    }
}
