using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlotContainer
{
    public List<InventorySlot> Slots { get; set; }
    public List<InventorySlot> EquippedItemSlots { get; set; }
    public int Coins { get; set; }
    public string SavePath { get; set; }

    public InventorySlotContainer(string path)
    {
        SavePath = path;
        Load(path);
    }

    public void AddItem(ItemObject itemObject, int amount)
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            if (Slots[i].ItemObject.itemID == itemObject.itemID)
            {
                Slots[i].Amount += amount;
                return;
            }
        }

        Slots.Add(new InventorySlot(itemObject, amount, Slots.Count));
    }

    public bool RemoveItem(int itemObjectID)
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            if (Slots[i].ItemObject.itemID == itemObjectID)
            {
                Slots[i].Amount--;

                if (Slots[i].Amount <= 0)
                {
                    Slots.Remove(Slots[i]);
                    return true;
                }
            }
        }

        return false;
    }

    public void RemoveItemAtIndex(int index)
    {
        Slots.RemoveAt(index);
    }

    public bool HasItem(int itemObjectID)
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            if (Slots[i].ItemObject.itemID == itemObjectID)
            {
                return true;
            }
        }

        return false;
    }

    public void Save()
    {
        LoadManager.SaveFile(SavePath, new SaveableInventory(Slots, EquippedItemSlots, Coins));
    }

    public void Load(string path)
    {
        Slots = new List<InventorySlot>();
        EquippedItemSlots = new List<InventorySlot>();

        SaveableInventory inventory = LoadManager.ReadFile<SaveableInventory>(SavePath);

        Coins = inventory.coins;

        foreach (SaveableInventorySlot slot in inventory.savedItems)
        {
            Slots.Add(new InventorySlot(GameManager.Instance.GetItemObjectByID(slot.id), slot.amount));
        }

        foreach(int id in inventory.equippedItemIds)
        {
            EquippedItemSlots.Add(GetSlotByItemID(id));
        }
    }

    public InventorySlot GetSlotByItemID(int id)
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            if(Slots[i].ItemObject.itemID == id)
            {
                return Slots[i];
            }
        }

        throw new System.Exception("Item is not in container");
    }
}