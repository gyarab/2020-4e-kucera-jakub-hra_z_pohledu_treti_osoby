using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Default Character Stats", menuName = "Character/Stats/Default")]
public class DefaultCharacterStats : ScriptableObject
{
    public float health;
    public float damage;
    public float fireDamage;
    public float magicDamage;
    public float armor;
    public float fireResistance;
    public float magicResistance;
}
