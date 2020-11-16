using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearLocation : MonoBehaviour, IWinCondition
{
    private int _enemiesAlive;
    public Action OnCompleted { get; set; }

    private void OnEnable()
    {
        EnemyController.OnEnemyDeath += UpdateWinCondition;
    }

    private void OnDisable()
    {
        EnemyController.OnEnemyDeath -= UpdateWinCondition;
    }

    public void UpdateWinCondition(Vector3 position)
    {
        _enemiesAlive--;

        if(_enemiesAlive <= 0)
        {
            OnCompleted?.Invoke();
        }
    }

    public List<Vector3> ConfirmSpawnLocations(List<Vector3> array)
    {
        _enemiesAlive = array.Count;
        return array;
    }

    public List<GenerationRule> SpecialGenerationRules()
    {
        // Nothing to change
        return new List<GenerationRule>();
    }
}
