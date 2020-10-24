﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubManager : MonoBehaviour
{
    [SerializeField]
    private Portal[] _portals;
    [SerializeField]
    private Shopkeeper[] _shopkeepers;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.CurrentHubManager = this;
    }

    public void EnablePlayerDependantObjects(Transform target, string shopInventoryPath)
    {
        for (int i = 0; i < _portals.Length; i++)
        {
            _portals[i].enabled = true;
        }

        for (int i = 0; i < _shopkeepers.Length; i++)
        {
            _shopkeepers[i].enabled = true;
            _shopkeepers[i].LoadShopInventory(shopInventoryPath);
        }
    }

    public void ChangeLevel()
    {
        // TODO what?
    }
}
