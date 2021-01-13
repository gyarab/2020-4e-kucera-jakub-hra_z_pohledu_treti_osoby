using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DamageCalculator
{
    // Spočítá uděleé poškození
    public static float CalculateDamage(float damage, float armourPenetration, float armour)
    {
        float armourLeft = Mathf.Max(armour - armourPenetration, 0);

        return damage - armourLeft;
    }
}
