﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shopkeeper : FloatingButton
{
    private InventorySlotContainer _shopInventoryContainer;
    private InventoryMonoBehaviour _inventoryMonoBehaviour;

    // Start is called before the first frame update
    void Start()
    {
        FloatingButtonStart();
        _inventoryMonoBehaviour = GameManager.Instance.Player.GetComponent<PlayerController>().GetPlayerInventory();
    }

    public void LoadShopInventory(string path)
    {
        _shopInventoryContainer.Load(path);
    }

    public void Interact()
    {
        Debug.Log("Shop opened");

        _inventoryMonoBehaviour.SetShop(true);
        _inventoryMonoBehaviour.SwitchContainer(_shopInventoryContainer);
        _inventoryMonoBehaviour.ShowInventory();
    }
}
