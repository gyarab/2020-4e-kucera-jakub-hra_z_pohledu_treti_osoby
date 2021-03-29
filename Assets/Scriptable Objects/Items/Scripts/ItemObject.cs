using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Data o předmětu jako je jeho ID, obrázek, jméno atd.; založeno na https://www.youtube.com/watch?v=_IqTeruf3-s od Coding With Unity
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
