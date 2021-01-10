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

    // Vrátí první otevřenou zeď, začínajíc od souseda na pozici 0
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

    // Vrátí počet otevřených zdí
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

    // Propojí navzájem podbuňku s druhou podbuňkou
    public void ConnectToSubcell(Subcell other, Side direction)
    {
        switch (direction)
        {
            case Side.Top:
                Neighbours[0] = other;
                other.Neighbours[4] = this;
                break;
            case Side.Right:
                Neighbours[2] = other;
                other.Neighbours[6] = this;
                break;
            case Side.Bottom:
                Neighbours[4] = other;
                other.Neighbours[0] = this;
                break;
            case Side.Left:
                Neighbours[6] = other;
                other.Neighbours[2] = this;
                break;
            case Side.TopRight:
                Neighbours[1] = other;
                other.Neighbours[5] = this;
                break;
            case Side.BottomRight:
                Neighbours[3] = other;
                other.Neighbours[7] = this;
                break;
            case Side.BottomLeft:
                Neighbours[5] = other;
                other.Neighbours[1] = this;
                break;
            case Side.TopLeft:
                Neighbours[7] = other;
                other.Neighbours[3] = this;
                break;
        }
    }
}
