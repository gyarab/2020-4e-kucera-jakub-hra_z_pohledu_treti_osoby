using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectArtefacts : MonoBehaviour, IWinCondition
{
    public Action OnCompleted { get; set; }

    private int _artefactsToCollect;
    private const int ARTEFACT_ITEM_ID = 7;
    private const string MESSAGE_COMPLETED = "Mission acomplished. Portal to next location is open.";
    private const string MESSAGE_BEGAN = "Collect all 4 artefacts and get our of the maze alive.";

    private void OnEnable()
    {
        GroundItem.OnItemPickedUp += UpdateWinCondition;
    }

    private void OnDisable()
    {
        GroundItem.OnItemPickedUp -= UpdateWinCondition;
    }

    public void UpdateWinCondition(ItemObject item, int amount)
    {
        _artefactsToCollect -= amount;

        if (_artefactsToCollect <= 0)
        {
            OnCompleted?.Invoke();
        }
    }

    public List<Vector3> ConfirmSpawnLocations(List<Vector3> array)
    {
        _artefactsToCollect = 4;

        Spawner spawner = GetComponent<Spawner>();
        for (int i = 0; i < _artefactsToCollect; i++)
        {
            int index = UnityEngine.Random.Range(0, array.Count);
            spawner.SpawnItem(array[index], ARTEFACT_ITEM_ID);
            array.RemoveAt(index);
        }

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
