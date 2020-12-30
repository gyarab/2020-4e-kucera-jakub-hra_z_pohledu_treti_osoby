using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Object", menuName = "InventorySystem/Items/Weapon")]
public class WeaponObject : ItemObject // TODO extend equipment?
{
    public float damage;
    public float healthBonus;
    public float armourPenetration;
    public AnimationType animationType;

    public GameObject model; // or more models idk how to do
    public Vector3 positionOffset;

    public void Awake()
    {
        type = ItemType.Weapon;
    }
}
