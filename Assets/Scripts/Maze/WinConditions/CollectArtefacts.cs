using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectArtefacts : MonoBehaviour, IWinCondition
{
    public Action OnCompleted { get; set; }

    private const int ARTEFACT_ITEM_ID = 1; // TODO rework?; set id

    public List<Vector3> ConfirmSpawnLocations(List<Vector3> array)
    {
        Spawner spawner = GetComponent<Spawner>();
        for (int i = 0; i < 4; i++)
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
}
