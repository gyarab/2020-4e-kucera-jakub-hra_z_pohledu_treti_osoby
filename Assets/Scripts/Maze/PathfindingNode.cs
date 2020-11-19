using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// TODO maybe change back to monobehaviuor
public class PathfindingNode : IPathfindingNode<PathfindingNode>
{
    public PathfindingNode[] Neighbours { get; set; }
    public Vector3 Position { get; set; }
    public int ID { get; set; }
    public int CameFromID { get; set; }
    public int IndexInHeap { get; set; }
    public Status Status { get; set; }
    // G - distance from start; H - estimated distance from end
    public float GCost { get; set; }
    public float HCost { get; set; }
    public float FCost => GCost + HCost;

    public int TileType { get; private set; }

    public PathfindingNode(int id, float x, float y, float z)
    {
        ID = id;
        Position = new Vector3(x, y, z);
        Neighbours = new PathfindingNode[8];
    }

    public PathfindingNode(int id, float x, float y, float z, int tileType)
    {
        ID = id;
        Position = new Vector3(x, y, z);
        Neighbours = new PathfindingNode[8];
        TileType = tileType;
    }

    public int CompareTo(PathfindingNode other)
    {
        if(FCost == other.FCost)
        {
            return GCost.CompareTo(other.GCost);
        }

        return FCost.CompareTo(other.FCost);
    }

    // TODO rename?
    public int GetFirstDoor()
    {
        bool foundWall = false;
        for (int i = 0; i < 8; i += 2)
        {
            if (foundWall)
            {
                if (Neighbours[i] != null)
                {
                    return i / 2;
                }
            }
            else if (Neighbours[i] == null)
            {
                foundWall = true;
            }
        }

        if (foundWall && Neighbours[0] != null)
        {
            return 0;
        }

        return -1;
    }

    // TODO rename?
    public int GetDoorCount()
    {
        int count = 0;
        for (int i = 0; i < 8; i += 2)
        {
            if(Neighbours[i] != null)
            {
                count++;
            }
        }

        return count;
    }

    // TODO remove
    public string ToString2()
    {
        string result = "";

        for (int i = 0; i < 8; i++)
        {
            if(Neighbours[i] == null)
            {
                result += "-, ";
            } else
            {
                result += Neighbours[i].ID + ", ";
            }
        }

        return result;
    }
}
