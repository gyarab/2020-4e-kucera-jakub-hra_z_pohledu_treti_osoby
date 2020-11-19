using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Cell
{
    // top = 0; right = 1; bottom = 2; left = 3
    private bool[] doors;
    public bool generated;
    public int tileCount; // TODO prob useless
    public int lowestSubcellIndex;

    public Cell()
    {
        // true if open
        doors = new bool[4];
    }

    public Cell(bool top, bool right, bool bottom, bool left)
    {
        doors = new bool[] { top, right, bottom, left };
    }

    public Cell(Side invertedSide)
    {
        doors = new bool[4];

        switch (invertedSide)
        {
            case Side.Top:
                OpenWall(Side.Bottom);
                break;
            case Side.Right:
                OpenWall(Side.Left);
                break;
            case Side.Bottom:
                OpenWall(Side.Top);
                break;
            case Side.Left:
                OpenWall(Side.Right);
                break;
            default:
                throw new System.Exception("Invalid enum");
        }
    }

    public void OpenWall(Side position)
    {
        switch (position)
        {
            case Side.Top:
                doors[0] = true;
                break;
            case Side.Right:
                doors[1] = true;
                break;
            case Side.Bottom:
                doors[2] = true;
                break;
            case Side.Left:
                doors[3] = true;
                break;
            default:
                throw new System.Exception("Invalid enum");
        }
    }

    public void CloseDoor(Side position)
    {
        switch (position)
        {
            case Side.Top:
                doors[0] = false;
                break;
            case Side.Right:
                doors[1] = false;
                break;
            case Side.Bottom:
                doors[2] = false;
                break;
            case Side.Left:
                doors[3] = false;
                break;
            default:
                throw new System.Exception("Invalid enum");
        }
    }

    public bool IsDoor(Side position)
    {
        switch (position)
        {
            case Side.Top:
                return doors[0];
            case Side.Right:
                return doors[1];
            case Side.Bottom:
                return doors[2];
            case Side.Left:
                return doors[3];
            default:
                throw new System.Exception("Invalid enum");
        }
    }

    public int GetDoorCount()
    {
        int counter = 0;
        for (int i = 0; i < doors.Length; i++)
        {
            if(doors[i])
            {
                counter++;
            }
        }

        return counter;
    }

    // TODO remove
    public string ToString2()
    {
        return doors[0] + ", " + doors[1] + ", " + doors[2] + ", " + doors[3];
    }

    //TODO remove
    public bool CompareCells(Cell cell)
    {
        if(IsDoor(Side.Top) == cell.IsDoor(Side.Top))
        {
            if (IsDoor(Side.Right) == cell.IsDoor(Side.Right))
            {
                if (IsDoor(Side.Bottom) == cell.IsDoor(Side.Bottom))
                {
                    if (IsDoor(Side.Left) == cell.IsDoor(Side.Left))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
