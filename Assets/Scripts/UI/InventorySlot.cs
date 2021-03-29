using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Jednotlivý předmět v inventáři; založeno na https://www.youtube.com/watch?v=_IqTeruf3-s od Coding with Unity
public class InventorySlot
{
    public ItemObject ItemObject { get; set; }
    public int Amount { get; set; }
    public int SlotHolderChildPosition { get; set; }

    // Konstruktor
    public InventorySlot(ItemObject itemObject, int amount)
    {
        ItemObject = itemObject;
        Amount = amount;
    }

    // Konstruktor, ale i s pozicí v poli v inventáři
    public InventorySlot(ItemObject itemObject, int amount, int position)
    {
        ItemObject = itemObject;
        Amount = amount;
        SlotHolderChildPosition = position;
    }
}
