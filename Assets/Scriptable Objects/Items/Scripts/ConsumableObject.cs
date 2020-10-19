using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable Object", menuName = "InventorySystem/Items/Consumable")]
public class ConsumableObject : ItemObject
{
    [Range(1, 2)]
    public float healthBuff = 1;
    [Range(1, 2)]
    public float damageBuff = 1;
    [Range(1, 2)]
    public float staminaBuff = 1;

    public void Awake()
    {
        type = ItemType.Consumable;
    }
}
