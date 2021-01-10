using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Kombinace útoků bosse
[CreateAssetMenu(fileName = "New Combo", menuName = "Combo")]
public class ComboSO : ScriptableObject
{
    public BossActionType[] actions;
    [Tooltip("It should contain as many entries as Wait enums in action list.")]
    public float[] waitTime;

    public float optimalAngle, optimalMinDistance, optimalMaxDistance;
}