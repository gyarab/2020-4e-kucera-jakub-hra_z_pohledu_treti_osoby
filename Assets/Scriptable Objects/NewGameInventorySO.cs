using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Počáteční stav inventáře
[CreateAssetMenu(fileName = "New New Game Inventory", menuName = "NG Inventory")]
public class NewGameInventorySO : ScriptableObject
{
    public int[] itemIDs;
}
