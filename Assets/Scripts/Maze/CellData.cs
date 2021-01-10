using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellData
{
    public Cell[,] Cells { get; private set; } // Z, X
    public Vector2Int FirstCell { get; private set; }
    public int CellCount { get; private set; }
    public int MaximumSubcellCount { get; private set; }
    public int[] XDistance { get; private set; }
    public int[] ZDistance { get; private set; }

    // Konstruktor se všemi parametry
    public CellData(Cell[,] cells, Vector2Int firstCell, int cellCount, int maximumSubcellCount, int[] xDistance, int[] zDistance)
    {
        Cells = cells;
        FirstCell = firstCell;
        CellCount = cellCount;
        MaximumSubcellCount = maximumSubcellCount;
        XDistance = xDistance;
        ZDistance = zDistance;
    }

    // Vrátí buňky, které jsou k buňce na dané pozici napojené
    public List<Vector2Int> GetNeighbouringCellPositions(Vector2Int position)
    {
        Cell cell = Cells[position.x, position.y];
        List<Vector2Int> result = new List<Vector2Int>();

        if (cell.IsDoor(Side.Top))
        {
            result.Add(new Vector2Int(position.x + 1, position.y));
        }
        if (cell.IsDoor(Side.Right))
        {
            result.Add(new Vector2Int(position.x, position.y + 1));
        }
        if (cell.IsDoor(Side.Bottom))
        {
            result.Add(new Vector2Int(position.x - 1, position.y));
        }
        if (cell.IsDoor(Side.Left))
        {
            result.Add(new Vector2Int(position.x, position.y - 1));
        }

        return result;
    }

    // Vrátí délku a výšku buňky
    public Vector2Int GetDimensions(Vector2Int position)
    {
        return new Vector2Int(ZDistance[position.x + 1] - ZDistance[position.x], XDistance[position.y + 1] - XDistance[position.y]);
    }

    // Vrátí délku a šířku buňky
    public Vector2Int GetDimensions(int positionZ, int positionX)
    {
        return new Vector2Int(ZDistance[positionZ + 1] - ZDistance[positionZ], XDistance[positionX + 1] - XDistance[positionX]);
    }

    // Vrátí souřadnice, na kterých povede cesta mezi buňkami
    public Vector2Int GetPath(Vector2Int position, Vector2Int dimension)
    {
        Vector2Int result = new Vector2Int();

        switch (dimension.x)
        {
            case 1:
                result.x = 0;
                break;
            case 2:
                result.x = position.x % 2;
                break;
            case 3:
                result.x = 1;
                break;
            default:
                throw new System.NotImplementedException("Rooms of size " + dimension.x + "are not supported");
        }

        switch (dimension.y)
        {
            case 1:
                result.y = 0;
                break;
            case 2:
                result.y = position.y % 2;
                break;
            case 3:
                result.y = 1;
                break;
            default:
                throw new System.NotImplementedException("Rooms of size " + dimension.y + "are not supported");
        }

        return result;
    }
}
