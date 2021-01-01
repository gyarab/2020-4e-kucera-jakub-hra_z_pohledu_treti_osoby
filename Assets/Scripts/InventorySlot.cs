using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlot
{
    public ItemObject ItemObject { get; set; }
    public int Amount { get; set; }
    public int SlotHolderChildPosition { get; set; }

    public InventorySlot(ItemObject itemObject, int amount)
    {
        ItemObject = itemObject;
        Amount = amount;
    }

    public InventorySlot(ItemObject itemObject, int amount, int position)
    {
        ItemObject = itemObject;
        Amount = amount;
        SlotHolderChildPosition = position;
    }
}
