using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Set dílků
[CreateAssetMenu(fileName = "New Tileset", menuName = "Tileset")]
public class TilesSO : ScriptableObject
{
    public GameObject[] tiles;
}
