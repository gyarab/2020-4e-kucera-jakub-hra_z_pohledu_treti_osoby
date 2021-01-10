using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Konzumovatelný předmět
[CreateAssetMenu(fileName = "New Consumable Object", menuName = "InventorySystem/Items/Consumable")]
public class ConsumableObject : ItemObject
{
    public float healthRegen;

    public void Awake()
    {
        type = ItemType.Consumable;
    }
}
