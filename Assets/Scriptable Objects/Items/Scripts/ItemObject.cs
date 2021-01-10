using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Data o předmětu jako je jeho ID, obrázek, jméno atd.
public abstract class ItemObject : ScriptableObject
{
    public int itemID;
    public Sprite uiSprite;
    public ItemType type;
    public string itemName;
    [TextArea(15,20)]
    public string description;
    [Min(0)]
    public int price;
}
