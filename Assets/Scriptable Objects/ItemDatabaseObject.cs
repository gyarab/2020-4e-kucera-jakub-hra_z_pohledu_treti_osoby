using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Databse", menuName = "InventorySystem/Items/Database")]
public class ItemDatabaseObject : ScriptableObject, ISerializationCallbackReceiver
{
    public ItemObject[] items;
    public Dictionary<int, ItemObject> getItem;

    // Vytvoří z pole slovník
    public void OnAfterDeserialize()
    {
        getItem = new Dictionary<int, ItemObject>();

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].itemID != 0) {
                getItem.Add(items[i].itemID, items[i]);
            }
        }
    }

    // Metoda je tu pouze kvůli interfacu ISerializationCallbackReceiver
    public void OnBeforeSerialize()
    {
        // probably useless
        /*items.Clear();

        foreach(KeyValuePair<int, ItemObject> kvp in getItem)
        {
            items.Add(kvp.Value);
        }*/
    }
}
