using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tileset", menuName = "Tileset")]
public class TilesSO : ScriptableObject
{
    public int tileSetID; // TODO do something with it
    public GameObject[] tiles;
}
