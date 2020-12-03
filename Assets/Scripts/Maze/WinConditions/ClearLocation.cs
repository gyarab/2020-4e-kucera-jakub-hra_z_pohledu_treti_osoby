using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearLocation : MonoBehaviour, IWinCondition
{
    public Action OnCompleted { get; set; }

    private int _enemiesAlive;
    private const string MESSAGE_COMPLETED = "Mission acomplished. Portal to next location is open.";
    private const string MESSAGE_BEGAN = "Clear maze from all enemy monsters.";

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

    public string[] GetMessages()
    {
        return new string[] { MESSAGE_BEGAN, MESSAGE_COMPLETED };
    }
}
