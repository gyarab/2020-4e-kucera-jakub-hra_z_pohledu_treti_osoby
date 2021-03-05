using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IWinCondition
{
    Action OnCompleted { get; set; }
    List<Vector3> ConfirmSpawnLocations(List<Vector3> array);
    List<GenerationRule> SpecialGenerationRules();
    string[] GetMessages();
}
