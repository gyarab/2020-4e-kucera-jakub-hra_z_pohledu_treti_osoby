using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment Object", menuName = "InventorySystem/Items/Equipment")]
public class EquipmentObject : ItemObject // TODO remove or implement
{
    public float defenceBonus = 0;
    [Range(0,2)]
    public float speedModifier = 1;

    public void Awake()
    {
        type = ItemType.Equipment;
    }
}
