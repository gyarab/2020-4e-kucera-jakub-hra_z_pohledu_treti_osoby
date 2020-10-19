using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Databse", menuName = "InventorySystem/Items/Database")]
public class ItemDatabaseObject : ScriptableObject, ISerializationCallbackReceiver
{
    public ItemObject[] items;
    public Dictionary<int, ItemObject> getItem = new Dictionary<int, ItemObject>();

    public void OnAfterDeserialize()
    {
        getItem = new Dictionary<int, ItemObject>();

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].id != 0) {
                getItem.Add(items[i].id, items[i]);
            }
        }
    }

    public void OnBeforeSerialize()
    {
        // TODO not working; wokring without it
        /*items.Clear();

        foreach(ItemObject item in getItem.Values)
        {
            items.Add(item);
        }*/
    }
}
