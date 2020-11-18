using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// TODO split into different files
public interface IDamageable
{
    void TakeDamage(float damage, float armourPenetration);
}

public interface IWinCondition
{
    Action OnCompleted { get; set; } // TODO add coin reward Action<int> || get artefact to sell for coins || reward from shopkeeper?
    List<Vector3> ConfirmSpawnLocations(List<Vector3> array);
    List<GenerationRule> SpecialGenerationRules();
}

public interface IDoor
{
    void Entered();
}

public interface ICellGenerator
{
    CellData GenerateCells(MazeSettingsSO mazeSettings, Vector3 position);
}