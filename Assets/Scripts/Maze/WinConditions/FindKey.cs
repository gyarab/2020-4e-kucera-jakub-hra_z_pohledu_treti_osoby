using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FindKey : MonoBehaviour, IWinCondition
{
    public Action OnCompleted { get; set; }

    private int _enemiesAlive, _totalEnemies;
    private bool _spawnedKey;
    private const string MESSAGE_COMPLETED = "Mission acomplished. This was the last mission. Thanks for playing the game.";
    private const string MESSAGE_BEGAN = "Find a key and use it to go through doors inside maze.";

    private const int KEY_ITEM_ID = 6;

    // Při aktivaci začne odebírat akci On Enemy Death
    private void OnEnable()
    {
        EnemyController.OnEnemyDeath += UpdateWinCondition;
    }

    // Při deaktivaci přestane odebírat akci On Enemy Death
    private void OnDisable()
    {
        EnemyController.OnEnemyDeath -= UpdateWinCondition;
    }

    // Metoda je vyvolána akcí On Enemy Death; rozhoduje o tom, kdy z protivníka padne klíč
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

    // Vytvoří instanci klíče
    private void SpawnKey(Vector3 position)
    {
        GetComponent<Spawner>().SpawnItem(position, KEY_ITEM_ID);
        _spawnedKey = true;
    }

    // Zapamatuje si počet nepřátel a vráti pole nepozměněné
    public List<Vector3> ConfirmSpawnLocations(List<Vector3> array)
    {
        _enemiesAlive = _totalEnemies = array.Count;
        return array;
    }

    // Vrací pole s podmínkou na vygenerování vedlejší místnosti
    public List<GenerationRule> SpecialGenerationRules()
    {
        return new List<GenerationRule> { GenerationRule.OuterRoom };
    }

    // Vrací zprávy informující hráče o jeho postupu
    public string[] GetMessages()
    {
        return new string[] { MESSAGE_BEGAN, MESSAGE_COMPLETED };
    }
}
