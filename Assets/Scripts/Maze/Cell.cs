using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Cell
{
    // top = 0; right = 1; bottom = 2; left = 3; true if open
    private bool[] doors;
    public bool generated;
    public int lowestSubcellIndex;

    // Konstruktor
    public Cell()
    {
        doors = new bool[4];
    }

    // Konstruktor s 4 parametry typu bool
    public Cell(bool top, bool right, bool bottom, bool left)
    {
        doors = new bool[] { top, right, bottom, left };
    }

    // Konstruktor, který vytvoří buňku s jednou otevřenou zdí
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

    // Otevře zeď
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

    // avře zeď
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

    // Vrátí true, když je buňka z té strany otevřená
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

    // Vrátí počet otevřených zdí
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
}
