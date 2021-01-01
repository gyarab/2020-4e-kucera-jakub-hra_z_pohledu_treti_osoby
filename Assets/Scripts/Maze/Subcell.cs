using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Subcell
{
    public int PositionInArray { get; set; }
    public Subcell[] Neighbours { get; set; }
    public Vector3 Position { get; set; }
    public int LowestPathfindingNodeID { get; set; }
    public int TileType { get; private set; }
    public bool NodesCreated { get; set; }

    public Subcell(int position, float x, float y, float z, int tileType)
    {
        PositionInArray = position;
        Position = new Vector3(x, y, z);
        Neighbours = new Subcell[8];
        TileType = tileType;
        NodesCreated = false;
    }

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

    public int GetDoorCount()
    {
        int count = 0;
        for (int i = 0; i < 8; i += 2)
        {
            if (Neighbours[i] != null)
            {
                count++;
            }
        }

        return count;
    }
}
