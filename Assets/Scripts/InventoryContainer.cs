using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlotContainer
{
    public List<InventoryItem> Slots { get; set; }
    public string SavePath { get; set; }

    public void Save()
    {
        LoadManager.SaveFile(SavePath, new SaveableInventory(Slots));
    }

    public void Load(string path)
    {
        SavePath = path;
        Slots = new List<InventoryItem>();

        SaveableInventory inv = LoadManager.ReadFile<SaveableInventory>(SavePath);

        foreach (SaveableInventorySlot slot in inv.savedItems)
        {
            Slots.Add(new InventoryItem(GameManager.Instance.GetItemObjectByID(slot.id), slot.amount));
        }
    }
}
