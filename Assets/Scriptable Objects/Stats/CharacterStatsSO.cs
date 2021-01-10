using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Obsahuje data o bojových hodnotách postav
[CreateAssetMenu(fileName = "New Character Stats", menuName = "Stats/Character")]
public class CharacterStatsSO : ScriptableObject
{
    public float health;
    public float armour;
    public float damage;
    public float armourPenetration;
}