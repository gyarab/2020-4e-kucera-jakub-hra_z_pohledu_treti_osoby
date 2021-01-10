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

    // Metoda je vyvolána akcí On Enemy Death; vyvolá akci On Completed, když je počet nepřátel naživu 0
    public void UpdateWinCondition(Vector3 position)
    {
        _enemiesAlive--;

        if(_enemiesAlive <= 0)
        {
            OnCompleted?.Invoke();
        }
    }

    // Zapamatuje si počet nepřátel a vráti vstupní seznam
    public List<Vector3> ConfirmSpawnLocations(List<Vector3> array)
    {
        _enemiesAlive = array.Count;
        return array;
    }

    // Nemá žádné speciální podmínky
    public List<GenerationRule> SpecialGenerationRules()
    {
        return new List<GenerationRule>();
    }

    // Vrací zprávy informující hráče o jeho postupu
    public string[] GetMessages()
    {
        return new string[] { MESSAGE_BEGAN, MESSAGE_COMPLETED };
    }
}
