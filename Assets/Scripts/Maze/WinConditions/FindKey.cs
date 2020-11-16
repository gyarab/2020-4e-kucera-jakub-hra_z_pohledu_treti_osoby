using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FindKey : MonoBehaviour, IWinCondition
{
    private int _enemiesAlive, _totalEnemies;
    public Action OnCompleted { get; set; }

    private const int KEY_ITEM_ID = 1; // TODO rework?

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
        Debug.Log(_enemiesAlive + " alive out of " + _totalEnemies);

        if(_enemiesAlive == 0)
        {
            SpawnKey(position);
        } else if(((float)_enemiesAlive / (float)_totalEnemies) < 0.5f)
        {
            if(UnityEngine.Random.Range(0,3) == 0) // TODO move?
            {
                SpawnKey(position);
            }
        }
    }

    private void SpawnKey(Vector3 position)
    {
        GetComponent<Spawner>().SpawnItem(position, KEY_ITEM_ID);
        Debug.Log("Key Spawned");
    }

    public List<Vector3> ConfirmSpawnLocations(List<Vector3> array)
    {
        _enemiesAlive = _totalEnemies = array.Count;
        return array;
    }

    public List<GenerationRule> SpecialGenerationRules()
    {
        // TODO spawn boss doors + room
        return new List<GenerationRule> { GenerationRule.OuterRoom };
    }
}
