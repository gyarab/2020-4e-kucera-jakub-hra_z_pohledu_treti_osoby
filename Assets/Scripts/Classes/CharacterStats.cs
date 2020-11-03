using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterStats
{
    public float Health { get; set; }
    public float Armour { get; set; }
    public float Damage { get; set; }
    public float ArmourPenetration { get; set; }

    public CharacterStats()
    {
        Health = Armour = Damage = ArmourPenetration = 0;
    }

    public CharacterStats(float health, float armour, float damage, float armourPenetration)
    {
        Health = health;
        Armour = armour;
        Damage = damage;
        ArmourPenetration = armourPenetration;

    }

    public void AddStats(CharacterStats stats)
    {
        Health += stats.Health;
        Armour += stats.Armour;
        Damage += stats.Damage;
        ArmourPenetration += ArmourPenetration;
    }

    public void SubtractStats(CharacterStats stats)
    {
        Health -= stats.Health;
        Armour -= stats.Armour;
        Damage -= stats.Damage;
        ArmourPenetration -= ArmourPenetration;
    }

    public string StatsToStringColumn(bool signs, bool ignoreNull)
    {
        string result = StatToString(Health, signs, ignoreNull);
        result += StatToString(Armour, signs, ignoreNull);
        result += StatToString(Damage, signs, ignoreNull);
        result += StatToString(ArmourPenetration, signs, ignoreNull);

        return result;
    }

    public string StatToString(float number, bool signs, bool ignoreNull)
    {
        if(number == 0 && ignoreNull)
        {
            return "\n";
        }

        string result = ((int)number).ToString();

        if (signs)
        {
            if(number > 0)
            {
                result = string.Concat("+", result);
            }
        }

        return result += "\n";
    }
}
