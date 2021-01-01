using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveableInventory
{
    public List<SaveableInventorySlot> savedItems;
    public List<int> equippedItemIds;
    public int coins;

    public SaveableInventory()
    {
        savedItems = new List<SaveableInventorySlot>();
        equippedItemIds = new List<int>();
        coins = 0;
    }

    public SaveableInventory(List<InventorySlot> items, List<InventorySlot> equippedItems, int coins)
    {
        savedItems = new List<SaveableInventorySlot>();

        foreach (InventorySlot slot in items) // TODO add to constructor?
        {
            savedItems.Add(new SaveableInventorySlot(slot.ItemObject.itemID, slot.Amount));
        }

        equippedItemIds = new List<int>();

        foreach (InventorySlot slot in equippedItems)
        {
            equippedItemIds.Add(slot.ItemObject.itemID);
        }

        this.coins = coins;
    }

    public SaveableInventory(int[] array)
    {
        savedItems = new List<SaveableInventorySlot>();

        for (int i = 0; i < array.Length; i++)
        {
            savedItems.Add(new SaveableInventorySlot(array[i], 1));
        }

        equippedItemIds = new List<int>();
    }
}
