using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
// Uložitelný slot v inventáři; založeno na https://www.youtube.com/watch?v=_IqTeruf3-s od Coding With Unity
public class SaveableInventorySlot
{
    public int id;
    public int amount;

    // Konstruktor s ID předmětu a množstvím
    public SaveableInventorySlot(int _id, int _amount)
    {
        id = _id;
        amount = _amount;
    }
}
