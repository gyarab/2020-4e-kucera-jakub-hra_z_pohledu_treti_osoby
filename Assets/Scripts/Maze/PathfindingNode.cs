using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// TODO maybe change back to monobehaviuor
public class PathfindingNode : IComparable<PathfindingNode>
{
    public PathfindingNode[] neighbours;
    public Vector2 position;
    public int id, cameFromID, indexInHeap;
    public Status Status { get; set; }
    // G - distance from start; H - estimate
    public float gCost, hCost;

    public PathfindingNode(int id, float x, float y)
    {
        this.id = id;
        position = new Vector2(x, y);
        neighbours = new PathfindingNode[8];
    }

    public int CompareTo(PathfindingNode other)
    {
        if(GetFCost() == other.GetFCost())
        {
            return gCost.CompareTo(other.gCost);
        }

        return GetFCost().CompareTo(other.GetFCost());
    }

    public float GetFCost()
    {
        return gCost + hCost;
    }

    // TODO rename?
    public int GetFirstDoor()
    {
        bool foundWall = false;
        for (int i = 0; i < 8; i += 2)
        {
            if (foundWall)
            {
                if (neighbours[i] != null)
                {
                    return i / 2;
                }
            }
            else if (neighbours[i] == null)
            {
                foundWall = true;
            }
        }

        if (foundWall && neighbours[0] != null)
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
            if(neighbours[i] != null)
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
            if(neighbours[i] == null)
            {
                result += "-, ";
            } else
            {
                result += neighbours[i].id + ", ";
            }
        }

        return result;
    }
}
