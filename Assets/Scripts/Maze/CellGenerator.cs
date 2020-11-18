using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellGenerator : MonoBehaviour, ICellGenerator
{
    private MazeSettingsSO _mazeSettings;
    private Cell[,] _cells; // Z, X
    private Stack<Vector2Int> _cellStack;
    private int[] _xDistance, _zDistance;

    public CellData GenerateCells(MazeSettingsSO mazeSettings, Vector3 startPoint) // TODO return class?
    {
        _mazeSettings = mazeSettings;

        int cellCounter = 1;
        // Z, X
        _cells = new Cell[_mazeSettings.length, _mazeSettings.width];
        _cellStack = new Stack<Vector2Int>();
        _xDistance = new int[_mazeSettings.width + 1];
        _zDistance = new int[_mazeSettings.length + 1];

        _zDistance[0] = _xDistance[0] = 0;
        for (int i = 1; i < _mazeSettings.length + 1; i++)
        {
            if (_mazeSettings.randomDistanceBetweenCells)
            {
                _zDistance[i] = _zDistance[i - 1] + Random.Range(_mazeSettings.minDistanceMultiplyer, _mazeSettings.maxDistanceMultiplyer);
            }
            else
            {
                _zDistance[i] = i;
            }
        }

        for (int i = 1; i < _mazeSettings.width + 1; i++)
        {
            if (_mazeSettings.randomDistanceBetweenCells)
            {
                _xDistance[i] = _xDistance[i - 1] + Random.Range(_mazeSettings.minDistanceMultiplyer, _mazeSettings.maxDistanceMultiplyer);
            }
            else
            {
                _xDistance[i] = i;
            }
        }

        // First Cell - add all possible neighbours, Z, X
        Vector2Int firstCell = new Vector2Int(Random.Range(1, _mazeSettings.length - 1), Random.Range(1, _mazeSettings.width - 1));
        Vector2Int currentCellPositionInArray = firstCell;
        _cells[currentCellPositionInArray.x, currentCellPositionInArray.y] = new Cell(true, true, true, true);
        PushNeighbouringCells(currentCellPositionInArray);
        int maxNodeCount = (_zDistance[currentCellPositionInArray.x + 1] - _zDistance[currentCellPositionInArray.x]) * (_xDistance[currentCellPositionInArray.y + 1] - _xDistance[currentCellPositionInArray.y]);

        // All other Cells
        while (_cellStack.Count > 0)
        {
            currentCellPositionInArray = _cellStack.Pop();
            if (_cells[currentCellPositionInArray.x, currentCellPositionInArray.y] != null)
            {
                continue;
            }
            CreateNewCell(currentCellPositionInArray);
            CreateNewDoors(currentCellPositionInArray);
            PushNeighbouringCells(currentCellPositionInArray);
            cellCounter++;
            maxNodeCount += (_zDistance[currentCellPositionInArray.x + 1] - _zDistance[currentCellPositionInArray.x]) * (_xDistance[currentCellPositionInArray.y + 1] - _xDistance[currentCellPositionInArray.y]);
        }

        return new CellData(_cells, firstCell, cellCounter, maxNodeCount, _xDistance, _zDistance);
    }

    // TODO move somwhere else, cuz used from other region
    // Pushes neighbouring Cells into Stack
    private void PushNeighbouringCells(Vector2Int position)
    {
        Cell cell = _cells[position.x, position.y];

        if (cell.IsDoor(Side.Top))
        {
            _cellStack.Push(new Vector2Int(position.x + 1, position.y));
        }
        if (cell.IsDoor(Side.Right))
        {
            _cellStack.Push(new Vector2Int(position.x, position.y + 1));
        }
        if (cell.IsDoor(Side.Bottom))
        {
            _cellStack.Push(new Vector2Int(position.x - 1, position.y));
        }
        if (cell.IsDoor(Side.Left))
        {
            _cellStack.Push(new Vector2Int(position.x, position.y - 1));
        }
    }

    // Creates new Cell and copies the door positions of surrounding Cells
    private void CreateNewCell(Vector2Int position)
    {
        Cell cell = new Cell();

        // Top - Z
        if (position.x < _mazeSettings.length - 1)
        {
            if (_cells[position.x + 1, position.y] != null)
            {
                if (_cells[position.x + 1, position.y].IsDoor(Side.Bottom))
                {
                    cell.OpenWall(Side.Top);
                }
            }
        }
        // Right - X
        if (position.y < _mazeSettings.width - 1)
        {
            if (_cells[position.x, position.y + 1] != null)
            {
                if (_cells[position.x, position.y + 1].IsDoor(Side.Left))
                {
                    cell.OpenWall(Side.Right);
                }
            }
        }
        // Bottom - Z
        if (position.x > 0)
        {
            if (_cells[position.x - 1, position.y] != null)
            {
                if (_cells[position.x - 1, position.y].IsDoor(Side.Top))
                {
                    cell.OpenWall(Side.Bottom);
                }
            }
        }
        // Left - X
        if (position.y > 0)
        {
            if (_cells[position.x, position.y - 1] != null)
            {
                if (_cells[position.x, position.y - 1].IsDoor(Side.Right))
                {
                    cell.OpenWall(Side.Left);
                }
            }
        }

        _cells[position.x, position.y] = cell;
    }

    // Try to create new doors
    private void CreateNewDoors(Vector2Int position)
    {
        int doorCount = _cells[position.x, position.y].GetDoorCount();
        //Debug.Log(doorCount);
        int offset = Random.Range(0, 4);

        for (int i = 0; i < 4; i++)
        {
            // Offset - random starting direction
            switch ((i + offset) % 4)
            {
                case 0:
                    // Top
                    if (position.x < _mazeSettings.length - 1)
                    {
                        if (_cells[position.x + 1, position.y] == null)
                        {
                            if (Random.Range(0f, 1) <= _mazeSettings.doorDirectionChance[0] * _mazeSettings.doorChanceFallOff[doorCount])
                            {
                                doorCount++;
                                _cells[position.x, position.y].OpenWall(Side.Top);
                            }
                        }
                    }
                    break;
                case 1:
                    // Rigth
                    if (position.y < _mazeSettings.width - 1)
                    {
                        if (_cells[position.x, position.y + 1] == null)
                        {
                            if (Random.Range(0f, 1) <= _mazeSettings.doorDirectionChance[1] * _mazeSettings.doorChanceFallOff[doorCount])
                            {
                                doorCount++;
                                _cells[position.x, position.y].OpenWall(Side.Right);
                            }
                        }
                    }
                    break;
                case 2:
                    // bottom
                    if (position.x > 0)
                    {
                        if (_cells[position.x - 1, position.y] == null)
                        {
                            if (Random.Range(0f, 1) <= _mazeSettings.doorDirectionChance[2] * _mazeSettings.doorChanceFallOff[doorCount])
                            {
                                doorCount++;
                                _cells[position.x, position.y].OpenWall(Side.Bottom);
                            }
                        }
                    }
                    break;
                case 3:
                    // Left
                    if (position.y > 0)
                    {
                        if (_cells[position.x, position.y - 1] == null)
                        {
                            if (Random.Range(0f, 1) <= _mazeSettings.doorDirectionChance[3] * _mazeSettings.doorChanceFallOff[doorCount])
                            {
                                doorCount++;
                                _cells[position.x, position.y].OpenWall(Side.Left);
                            }
                        }
                    }
                    break;
            }
        }
    }
}
