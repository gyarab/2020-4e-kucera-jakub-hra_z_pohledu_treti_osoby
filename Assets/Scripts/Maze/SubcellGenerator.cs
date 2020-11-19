using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubcellGenerator : MonoBehaviour, ISubcellGenerator
{
    private Subcell[] _subcells;
    private int _emptySpotInArray;

    private MazeSettingsSO _mazeSettings;
    private CellData _cellData;
    private Vector3 _startPoint;

    public SubcellData GenerateSubcells(MazeSettingsSO mazeSettings, CellData cellData, Vector3 startPoint, int additionalArraySize)
    {
        _mazeSettings = mazeSettings;
        _cellData = cellData;
        _startPoint = startPoint;

        _emptySpotInArray = 0;
        _subcells = new Subcell[_cellData.MaximumSubcellCount + additionalArraySize];
        Stack<Vector2Int> cellStack = new Stack<Vector2Int>();

        // First Cell
        Vector3 spawnPoint = new Vector3(startPoint.x + (float)_cellData.XDistance[_cellData.FirstCell.y] * _mazeSettings.distanceBetweenCells, startPoint.y, startPoint.z + (float)_cellData.ZDistance[_cellData.FirstCell.x] * _mazeSettings.distanceBetweenCells); // TODO only points to the right cell // RLpos
        CreateRoom(_cellData.FirstCell);
        PushNeighbouringCellsIntoStack(cellStack, _cellData.FirstCell);
        _cellData.Cells[_cellData.FirstCell.x, _cellData.FirstCell.y].generated = true;

        // Other Cells, Z, X
        Vector2Int currentCell;
        while (cellStack.Count > 0)
        {
            currentCell = cellStack.Pop();
            if (_cellData.Cells[currentCell.x, currentCell.y].generated == true)
            {
                continue;
            }

            if (Random.Range(0f, 1f) <= _mazeSettings.roomChance[_cellData.Cells[currentCell.x, currentCell.y].GetDoorCount() - 1])
            {
                CreateRoom(currentCell);
            }
            else
            {
                CreateCorridor(currentCell);
            }

            PushNeighbouringCellsIntoStack(cellStack, currentCell);
            _cellData.Cells[currentCell.x, currentCell.y].generated = true;
        }

        return new SubcellData(_subcells, _emptySpotInArray, spawnPoint);
    }

    private void CreateRoom(Vector2Int position)
    {
        int tileType = _mazeSettings.roomTileTypes[Random.Range(0, _mazeSettings.roomTileTypes.Length)];

        Vector2Int dimensions = _cellData.GetDimensions(position);
        // X, Z
        Vector2 realPosition = new Vector2(_startPoint.x + _cellData.XDistance[position.y] * _mazeSettings.distanceBetweenCells, _startPoint.z + _cellData.ZDistance[position.x] * _mazeSettings.distanceBetweenCells); // RLpos
        _cellData.Cells[position.x, position.y].lowestSubcellIndex = _emptySpotInArray;

        // From bottom left corner 
        for (int i = 0; i < dimensions.x; i++) // Z
        {
            for (int j = 0; j < dimensions.y; j++) // X
            {
                _subcells[_emptySpotInArray] = new Subcell(_emptySpotInArray, realPosition.x + _mazeSettings.distanceBetweenCells * j, _startPoint.y, realPosition.y + _mazeSettings.distanceBetweenCells * i, tileType); // RLpos

                // 2nd column + (Y)
                if (j > 0)
                {
                    ConnectTwoSubcells(_subcells[_emptySpotInArray], _subcells[_emptySpotInArray - 1], Side.Left);
                }

                // 2nd row + (X)
                if (i > 0)
                {
                    ConnectTwoSubcells(_subcells[_emptySpotInArray], _subcells[_emptySpotInArray - dimensions.y], Side.Bottom);

                    // Diagonal ones
                    if (j > 0)
                    {
                        ConnectTwoSubcells(_subcells[_emptySpotInArray], _subcells[_emptySpotInArray - dimensions.y - 1], Side.BottomLeft);
                    }

                    if (j < dimensions.y - 1)
                    {
                        ConnectTwoSubcells(_subcells[_emptySpotInArray], _subcells[_emptySpotInArray - dimensions.y + 1], Side.BottomRight);
                    }
                }

                _emptySpotInArray++;
            }
        }

        // Connect nodes with surrounding Cells
        ConnectSubcellsFromDifferentCells(position, dimensions);
    }

    // Creates corridor
    private void CreateCorridor(Vector2Int position)
    {
        int tileType = _mazeSettings.corridorTileTypes[Random.Range(0, _mazeSettings.corridorTileTypes.Length)];

        // X, Z
        Vector2 realPosition = new Vector2(_startPoint.x + _cellData.XDistance[position.y] * _mazeSettings.distanceBetweenCells, _startPoint.z + _cellData.ZDistance[position.x] * _mazeSettings.distanceBetweenCells); // RLpos
        Vector2Int dimensions = _cellData.GetDimensions(position);
        Vector2Int pathCoords = _cellData.GetPath(position, dimensions);
        Vector2 centerPosition = new Vector2(realPosition.x + _mazeSettings.distanceBetweenCells * pathCoords.y, realPosition.y + _mazeSettings.distanceBetweenCells * pathCoords.x); // RLpos
        // Z, X
        int centerPositionInArray = _emptySpotInArray + (dimensions.y * pathCoords.x) + pathCoords.y;
        _cellData.Cells[position.x, position.y].lowestSubcellIndex = _emptySpotInArray;

        Subcell centerSubcell = new Subcell(centerPositionInArray, centerPosition.x, _startPoint.y, centerPosition.y, tileType);
        _subcells[centerPositionInArray] = centerSubcell;

        // Top
        if (_cellData.Cells[position.x, position.y].IsDoor(Side.Top))
        {
            if (pathCoords.x < (dimensions.x - 1))
            {
                int positionInArray = centerPositionInArray + dimensions.y;
                _subcells[positionInArray] = new Subcell(centerPositionInArray + dimensions.y, centerPosition.x, _startPoint.y, centerPosition.y + _mazeSettings.distanceBetweenCells, tileType); // RLpos
                ConnectTwoSubcells(_subcells[positionInArray], centerSubcell, Side.Bottom);
            }
        }
        // Right
        if (_cellData.Cells[position.x, position.y].IsDoor(Side.Right))
        {
            if (pathCoords.y < (dimensions.y - 1))
            {
                int positionInArray = centerPositionInArray + 1;
                _subcells[positionInArray] = new Subcell(centerPositionInArray + 1, centerPosition.x + _mazeSettings.distanceBetweenCells, _startPoint.y, centerPosition.y, tileType); // RLpos
                ConnectTwoSubcells(_subcells[positionInArray], centerSubcell, Side.Left);
            }
        }
        // Bottom
        if (_cellData.Cells[position.x, position.y].IsDoor(Side.Bottom))
        {
            if (pathCoords.x > 0)
            {
                int positionInArray = centerPositionInArray - dimensions.y;
                _subcells[positionInArray] = new Subcell(centerPositionInArray - dimensions.y, centerPosition.x, _startPoint.y, centerPosition.y - _mazeSettings.distanceBetweenCells, tileType); // RLpos
                ConnectTwoSubcells(_subcells[positionInArray], centerSubcell, Side.Top);
            }
        }
        // Left
        if (_cellData.Cells[position.x, position.y].IsDoor(Side.Left))
        {
            if (pathCoords.y > 0)
            {
                int positionInArray = centerPositionInArray - 1;
                _subcells[positionInArray] = new Subcell(centerPositionInArray - 1, centerPosition.x - _mazeSettings.distanceBetweenCells, _startPoint.y, centerPosition.y, tileType); // RLpos
                ConnectTwoSubcells(_subcells[positionInArray], centerSubcell, Side.Right);
            }
        }

        _emptySpotInArray += dimensions.x * dimensions.y;

        // Connect subcells with subcells from surrounding Cells
        ConnectSubcellsFromDifferentCells(position, dimensions);
    }

    private void ConnectTwoSubcells(Subcell first, Subcell second, Side direction)
    {
        switch (direction)
        {
            case Side.Top:
                first.Neighbours[0] = second;
                second.Neighbours[4] = first;
                break;
            case Side.Right:
                first.Neighbours[2] = second;
                second.Neighbours[6] = first;
                break;
            case Side.Bottom:
                first.Neighbours[4] = second;
                second.Neighbours[0] = first;
                break;
            case Side.Left:
                first.Neighbours[6] = second;
                second.Neighbours[2] = first;
                break;
            case Side.TopRight:
                first.Neighbours[1] = second;
                second.Neighbours[5] = first;
                break;
            case Side.BottomRight:
                first.Neighbours[3] = second;
                second.Neighbours[7] = first;
                break;
            case Side.BottomLeft:
                first.Neighbours[5] = second;
                second.Neighbours[1] = first;
                break;
            case Side.TopLeft:
                first.Neighbours[7] = second;
                second.Neighbours[3] = first;
                break;
        }
    }

    // Connects nodes between cells
    private void ConnectSubcellsFromDifferentCells(Vector2Int position, Vector2Int firstDimensions)
    {
        Cell cell = _cellData.Cells[position.x, position.y];
        Vector2Int paths = _cellData.GetPath(position, firstDimensions);
        Vector2Int secondDimension;
        int firstBasePositionInArray = cell.lowestSubcellIndex;
        int secondBasePositionInArray;

        // Top
        if (cell.IsDoor(Side.Top))
        {
            if (_cellData.Cells[position.x + 1, position.y].generated)
            {
                secondBasePositionInArray = _cellData.Cells[position.x + 1, position.y].lowestSubcellIndex;
                ConnectTwoSubcells(_subcells[firstBasePositionInArray + (firstDimensions.y * (firstDimensions.x - 1)) + paths.y], _subcells[secondBasePositionInArray + paths.y], Side.Top);
            }
        }
        // Right
        if (cell.IsDoor(Side.Right))
        {
            if (_cellData.Cells[position.x, position.y + 1].generated)
            {
                secondBasePositionInArray = _cellData.Cells[position.x, position.y + 1].lowestSubcellIndex;
                secondDimension = _cellData.GetDimensions(position.x, position.y + 1);
                ConnectTwoSubcells(_subcells[firstBasePositionInArray + (firstDimensions.y * paths.x) + firstDimensions.y - 1], _subcells[secondBasePositionInArray + (secondDimension.y * paths.x)], Side.Right);
            }
        }
        // Bottom
        if (cell.IsDoor(Side.Bottom))
        {
            if (_cellData.Cells[position.x - 1, position.y].generated)
            {
                secondBasePositionInArray = _cellData.Cells[position.x - 1, position.y].lowestSubcellIndex;
                secondDimension = _cellData.GetDimensions(position.x - 1, position.y);
                ConnectTwoSubcells(_subcells[firstBasePositionInArray + paths.y], _subcells[secondBasePositionInArray + (secondDimension.y * (secondDimension.x - 1)) + paths.y], Side.Bottom);
            }
        }
        // Left
        if (cell.IsDoor(Side.Left))
        {
            if (_cellData.Cells[position.x, position.y - 1].generated)
            {
                secondBasePositionInArray = _cellData.Cells[position.x, position.y - 1].lowestSubcellIndex;
                secondDimension = _cellData.GetDimensions(position.x, position.y - 1);
                ConnectTwoSubcells(_subcells[firstBasePositionInArray + (firstDimensions.y * paths.x)], _subcells[secondBasePositionInArray + (secondDimension.y * paths.x) + secondDimension.y - 1], Side.Left);
            }
        }
    }

    private void PushNeighbouringCellsIntoStack(Stack<Vector2Int> stack, Vector2Int currentCellposition)
    {
        foreach (Vector2Int position in _cellData.GetNeighbouringCellPositions(currentCellposition))
        {
            stack.Push(position);
        }
    }
}
