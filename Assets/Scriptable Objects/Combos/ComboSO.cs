using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Combo", menuName = "Combo")]
public class ComboSO : ScriptableObject
{
    public BossActionType[] actions;
    [Tooltip("Should be as long as are there Wait enums in action list.")] // TODO fix english
    public float[] waitTime;

    public float optimalAngle, optimalMinDistance, optimalMaxDistance;
}