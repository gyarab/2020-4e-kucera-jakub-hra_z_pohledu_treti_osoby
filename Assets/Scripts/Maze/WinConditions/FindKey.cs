using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FindKey : MonoBehaviour, IWinCondition // TODO remove the key part?
{
    private int _enemiesAlive, _totalEnemies;
    private bool _spawnedKey;
    public Action OnCompleted { get; set; }

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
        Debug.Log("Key Spawned");
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
}
