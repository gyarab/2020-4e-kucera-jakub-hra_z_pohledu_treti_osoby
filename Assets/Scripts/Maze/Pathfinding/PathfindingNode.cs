using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathfindingNode : IPathfindingNode<PathfindingNode>
{
    public PathfindingNode[] Neighbours { get; set; }
    public Vector3 Position { get; set; }
    public int ID { get; set; }
    public int CameFromID { get; set; }
    public int IndexInHeap { get; set; }
    public Status Status { get; set; }
    // G - distance from the start; H - estimated distance from the end
    public float GCost { get; set; }
    public float HCost { get; set; }
    public float FCost => GCost + HCost;

    // Konstruktor
    public PathfindingNode(int id, float x, float y, float z)
    {
        ID = id;
        Position = new Vector3(x, y, z);
        Neighbours = new PathfindingNode[8];
    }

    // Porovná hodnoty uzlů, přednost má ten, u kterého je součet vzdálenosti od počátku a odhadovaná vzdálenost od konce menší
    public int CompareTo(PathfindingNode other)
    {
        if(FCost == other.FCost)
        {
            return GCost.CompareTo(other.GCost);
        }

        return FCost.CompareTo(other.FCost);
    }
}
