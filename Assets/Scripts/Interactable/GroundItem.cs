using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GroundItem : FloatingButton
{
    public static Action<ItemObject, int> OnItemPickedUp;
    
    private ItemObject _item;

    // Inicializace 
    private void Start()
    {
        FloatingButtonStart();
    }

    // Nastaví předmět, který obsahuje
    public void SetVariables(ItemObject item)
    {
        _item = item;
    }
        
    // Metoda je zavolána pomocí UI; po zvednutí vyvolá akci On Item Picked Up a zničí tento Game Object
    public void PickUpGUI()
    {
        OnItemPickedUp?.Invoke(_item, 1);
        Destroy(gameObject);
    }
}
