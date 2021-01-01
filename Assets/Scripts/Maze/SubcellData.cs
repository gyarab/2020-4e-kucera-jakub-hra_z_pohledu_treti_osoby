using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubcellData
{
    public Subcell[] Subcells { get; set; }
    public int EmptySpotInArray { get; set; }
    public Vector3 SpawnPoint { get; set; }

    public SubcellData(Subcell[] subcells, int firstEmptySpotInArray, Vector3 spawnPoint)
    {
        Subcells = subcells;
        EmptySpotInArray = firstEmptySpotInArray;
        SpawnPoint = spawnPoint;
    }
}
