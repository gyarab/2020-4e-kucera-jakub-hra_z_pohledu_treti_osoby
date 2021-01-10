using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Obsahuje informace o zbrani a odkaz na její model
[CreateAssetMenu(fileName = "New Weapon Object", menuName = "InventorySystem/Items/Weapon")]
public class WeaponObject : ItemObject
{
    public float damage;
    public float healthBonus;
    public float armourPenetration;
    public AnimationType animationType;

    public GameObject model;
    public Vector3 positionOffset;

    public void Awake()
    {
        type = ItemType.Weapon;
    }
}
