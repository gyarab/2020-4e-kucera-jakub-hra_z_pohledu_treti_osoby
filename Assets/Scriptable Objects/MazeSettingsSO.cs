using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Maze Settings", menuName = "Maze Settings")]
public class MazeSettingsSO : ScriptableObject
{
    [Header("Dimensions and Position")]
    public Vector3 centerPoint = Vector3.zero;
    [Range(0, 30)]
    public float distanceBetweenCells = 1;
    public int length, width; // width - X; length - Z
    public int minDistanceMultiplyer, maxDistanceMultiplyer;
    public bool randomDistanceBetweenCells;
    public int pathfindingNodesInSubcell;
    [Range(0.1f, 1f)]
    public float nodeSpreadPercentage;

    [Header("General Probability")]
    public int triesToGenerateMaze = 10;
    [Range(0f,1f)]
    public float minCellPercentage;

    [Header("Specific Probability")]
    [Range(0f, 1f)]
    public float[] roomChance = new float[4];
    [Range(0f, 1f), Tooltip("From 1 to 4 Doors")]
    public float[] doorChanceFallOff = new float[4];
    [Range(0f, 1f), Tooltip("Top, Right, Bottom, Left")]
    public float[] doorDirectionChance = new float[4];

    [Header("Tile Type")]
    public int[] roomTileTypes;
    public int[] corridorTileTypes;

    [Header("Enemy")]
    [Range(0f, 1f)]
    public float spawnChance;
}
