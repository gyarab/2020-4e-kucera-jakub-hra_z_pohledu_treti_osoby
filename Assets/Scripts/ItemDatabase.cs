using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    [SerializeField]
    private List<ItemObject> items;
    public Dictionary<int, ItemObject> GetItem { get; private set; }

    // Start is called before the first frame update
    void Awake()
    {
        GetItem = new Dictionary<int, ItemObject>();

        for (int i = 0; i < items.Count; i++)
        {
            GetItem.Add(items[i].itemID, items[i]);
        }
    }
}