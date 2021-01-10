using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shopkeeper : FloatingButton
{
    private InventorySlotContainer _shopInventoryContainer;
    private InventoryMonoBehaviour _inventoryMonoBehaviour;

    // Inicializace
    void Start()
    {
        FloatingButtonStart();
        _inventoryMonoBehaviour = GameManager.Instance.Player.GetComponent<PlayerController>().GetPlayerInventory();
    }

    // Načte stav obchodu ze souboru
    public void LoadShopInventory(string path)
    {
        _shopInventoryContainer = new InventorySlotContainer(path);
    }

    // Tato metoda je zavolána pomocí grafického rozhraní
    public void InteractGUI()
    {
        _inventoryMonoBehaviour.LoadAndOpenShop(_shopInventoryContainer);
    }
}
