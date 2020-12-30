using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FindKey : MonoBehaviour, IWinCondition // TODO remove the key part?
{
    public Action OnCompleted { get; set; }

    private int _enemiesAlive, _totalEnemies;
    private bool _spawnedKey;
    private const string MESSAGE_COMPLETED = "Mission acomplished. This was the last mission. Thanks for playing the game.";
    private const string MESSAGE_BEGAN = "Find a key and use it to go through doors inside maze.";

    private const int KEY_ITEM_ID = 6;

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

        if (_spawnedKey)
        {
            return;
        }

        if(_enemiesAlive == 0)
        {
            SpawnKey(position);
        } else if(((float)_enemiesAlive / (float)_totalEnemies) < 0.5f)
        {
            if(UnityEngine.Random.Range(0,3) == 0)
            {
                SpawnKey(position);
            }
        }
    }

    private void SpawnKey(Vector3 position)
    {
        GetComponent<Spawner>().SpawnItem(position, KEY_ITEM_ID);
        _spawnedKey = true;
    }

    public List<Vector3> ConfirmSpawnLocations(List<Vector3> array)
    {
        _enemiesAlive = _totalEnemies = array.Count;
        return array;
    }

    public List<GenerationRule> SpecialGenerationRules()
    {
        // Spawn outer room with boss
        return new List<GenerationRule> { GenerationRule.OuterRoom };
    }

    public string[] GetMessages()
    {
        return new string[] { MESSAGE_BEGAN, MESSAGE_COMPLETED };
    }
}
