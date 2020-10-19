using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopInventoryItemObject : InventoryItem
{
    public int price;

    public ShopInventoryItemObject(ItemObject _itemObject, int _amount, int _price) : base(_itemObject, _amount)
    {
        price = _price;
    }
}
