using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellData
{
    public Cell[,] Cells { get; private set; } // Z, X
    public Vector2Int FirstCell { get; private set; }
    public int CellCount { get; private set; }
    public int MaximumNodeCount { get; private set; }
    public int[] XDistance { get; private set; }
    public int[] ZDistance { get; private set; }

    public CellData(Cell[,] cells, Vector2Int firstCell, int cellCount, int maximumNodeCount, int[] xDistance, int[] zDistance)
    {
        Cells = cells;
        FirstCell = firstCell;
        CellCount = cellCount;
        MaximumNodeCount = maximumNodeCount;
        XDistance = xDistance;
        ZDistance = zDistance;
    }

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
}
