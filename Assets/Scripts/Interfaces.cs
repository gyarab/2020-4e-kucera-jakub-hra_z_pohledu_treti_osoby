using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO split into different files
public interface IDamageable
{
    void TakeDamage(float damage, float armourPenetration);
}

public interface IWinCondition // TODO change?
{
    bool IsCompleted();
    void SpawnObjects();
    void GiveInstructions();
    void OnCompleted();
}
