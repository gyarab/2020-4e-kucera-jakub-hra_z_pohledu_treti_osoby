using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectArtefacts : MonoBehaviour, IWinCondition
{
    public Action OnCompleted { get; set; }

    private int _artefactsToCollect = 4;
    private const int ARTEFACT_ITEM_ID = 7;
    private const string MESSAGE_COMPLETED = "Mission acomplished. Portal to the next location is now open.";
    private const string MESSAGE_BEGAN = "Collect all 4 artefacts and get out of the maze alive.";

    // Při aktivaci začne odebírat akci On Item Picked Up
    private void OnEnable()
    {
        GroundItem.OnItemPickedUp += UpdateWinCondition;
    }

    // Při deaktivaci přestane odebírat akci On Item Picked Up
    private void OnDisable()
    {
        GroundItem.OnItemPickedUp -= UpdateWinCondition;
    }

    // Metoda je vyvolána akcí On Item Picked Up; Kontroluje, jestli všechny artefakty byly sebrány
    public void UpdateWinCondition(ItemObject item, int amount)
    {
        _artefactsToCollect -= amount;

        if (_artefactsToCollect <= 0)
        {
            OnCompleted?.Invoke();
        }
    }

    // Vyjme pár pozicí a na nich vytvoří instance artefaktů
    public List<Vector3> ConfirmSpawnLocations(List<Vector3> array)
    {
        Spawner spawner = GetComponent<Spawner>();
        for (int i = 0; i < _artefactsToCollect; i++)
        {
            int index = UnityEngine.Random.Range(0, array.Count);
            spawner.SpawnItem(array[index], ARTEFACT_ITEM_ID);
            array.RemoveAt(index);
        }

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
