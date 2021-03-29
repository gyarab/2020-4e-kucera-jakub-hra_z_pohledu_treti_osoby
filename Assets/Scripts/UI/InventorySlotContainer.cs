using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Třída s předměty v inventáři; založeno na https://www.youtube.com/watch?v=_IqTeruf3-s, https://www.youtube.com/watch?v=232EqU1k9yQ od Coding With Unity
public class InventorySlotContainer
{
    public List<InventorySlot> Slots { get; set; }
    public List<InventorySlot> EquippedItemSlots { get; set; }
    public int Coins { get; set; }
    public string SavePath { get; set; }

    // Konstruktor, který obsahuje cestu k uloženeému souboru
    public InventorySlotContainer(string path)
    {
        SavePath = path;
        Load(path);
    }

    // Přidá předmět podle ID do inventáře
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

    // Odstraní předmět podle ID z inventáře; vrací true, když to byl poslední předmět toho druhu
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

    // Odstraní předmět na určité pozici v poli
    public void RemoveItemAtIndex(int index)
    {
        Slots.RemoveAt(index);
    }

    // Vrátí true, když je předmět s určitým ID v inventáři
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

    // Uloží inventář na zařízení
    public void Save()
    {
        LoadManager.SaveFile(SavePath, new SaveableInventory(Slots, EquippedItemSlots, Coins));
    }

    // Načte inventář ze zařízení
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

    // Vrátí Inventory SLot obsahující předmět s daným ID
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