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

    // Vytvoří podbuňky podle pravidel v Maze Settings
    public SubcellData GenerateSubcells(MazeSettingsSO mazeSettings, CellData cellData, Vector3 startPoint, int additionalArraySize)
    {
        _mazeSettings = mazeSettings;
        _cellData = cellData;
        _startPoint = startPoint;

        _emptySpotInArray = 0;
        _subcells = new Subcell[_cellData.MaximumSubcellCount + additionalArraySize];
        Stack<Vector2Int> cellStack = new Stack<Vector2Int>();

        // Vezme první buňku, vytvoří v ní podbuňky a vloží sousední buňky do zásobníku
        Vector3 spawnPoint = new Vector3(startPoint.x + (float)_cellData.XDistance[_cellData.FirstCell.y] * _mazeSettings.distanceBetweenCells, startPoint.y, startPoint.z + (float)_cellData.ZDistance[_cellData.FirstCell.x] * _mazeSettings.distanceBetweenCells); // TODO only points to the right cell // RLpos
        CreateRoom(_cellData.FirstCell);
        PushNeighbouringCellsIntoStack(cellStack, _cellData.FirstCell);
        _cellData.Cells[_cellData.FirstCell.x, _cellData.FirstCell.y].generated = true;

        // Rozděluje buňky na podbuňky a vkládá do zásobníku sousední buňky, dokud zásobník není prázdný
        Vector2Int currentCell; // Z, X
        while (cellStack.Count > 0)
        {
            currentCell = cellStack.Pop();
            if (_cellData.Cells[currentCell.x, currentCell.y].generated == true)
            {
                continue;
            }

            // Náhodně vybere, jestli to bude místnost nebo chodba
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

    // Vytvoří místnost
    private void CreateRoom(Vector2Int position)
    {
        int tileType = _mazeSettings.roomTileTypes[Random.Range(0, _mazeSettings.roomTileTypes.Length)];

        Vector2Int dimensions = _cellData.GetDimensions(position);
        // X, Z
        Vector2 realPosition = new Vector2(_startPoint.x + _cellData.XDistance[position.y] * _mazeSettings.distanceBetweenCells, _startPoint.z + _cellData.ZDistance[position.x] * _mazeSettings.distanceBetweenCells); // RLpos
        _cellData.Cells[position.x, position.y].lowestSubcellIndex = _emptySpotInArray;

        // Začne v levém spodním rohu
        for (int i = 0; i < dimensions.x; i++) // Z
        {
            for (int j = 0; j < dimensions.y; j++) // X
            {
                _subcells[_emptySpotInArray] = new Subcell(_emptySpotInArray, realPosition.x + _mazeSettings.distanceBetweenCells * j, _startPoint.y, realPosition.y + _mazeSettings.distanceBetweenCells * i, tileType); // RLpos

                // Když je podbuňka aspoň ve druhém sloupci, je potřeba ji propojit s podbuňkou nalevo od ní; (Y)
                if (j > 0)
                {
                    _subcells[_emptySpotInArray].ConnectToSubcell(_subcells[_emptySpotInArray - 1], Side.Left);
                }

                // Když je podbuňka aspoň ve druhé řadě, je potřeba ji propojit s podbuňkou pod ní; (X)
                if (i > 0)
                {
                    _subcells[_emptySpotInArray].ConnectToSubcell(_subcells[_emptySpotInArray - dimensions.y], Side.Bottom);

                    // Když je podbuňka aspoň ve druhém sloupci, je potřeba ji propojit s podbuňkou vlevo dole od ní;
                    if (j > 0)
                    {
                        _subcells[_emptySpotInArray].ConnectToSubcell(_subcells[_emptySpotInArray - dimensions.y - 1], Side.BottomLeft);
                    }

                    // Když je podbuňka není v posledním sloupci, je potřeba ji propojit s podbuňkou vpravo dole od ní;
                    if (j < dimensions.y - 1)
                    {
                        _subcells[_emptySpotInArray].ConnectToSubcell(_subcells[_emptySpotInArray - dimensions.y + 1], Side.BottomRight);
                    }
                }

                _emptySpotInArray++;
            }
        }

        // Propojí podbuňky s podbuňkami z jiných buňek
        ConnectSubcellsFromDifferentCells(position, dimensions);
    }

    // Vytvoří chodbu
    private void CreateCorridor(Vector2Int position)
    {
        int tileType = _mazeSettings.corridorTileTypes[Random.Range(0, _mazeSettings.corridorTileTypes.Length)];

        // X, Z
        Vector2 realPosition = new Vector2(_startPoint.x + _cellData.XDistance[position.y] * _mazeSettings.distanceBetweenCells, _startPoint.z + _cellData.ZDistance[position.x] * _mazeSettings.distanceBetweenCells); // RLpos
        Vector2Int dimensions = _cellData.GetDimensions(position);
        Vector2Int pathCoords = _cellData.GetPath(position, dimensions);
        Vector2 centerPosition = new Vector2(realPosition.x + _mazeSettings.distanceBetweenCells * pathCoords.y, realPosition.y + _mazeSettings.distanceBetweenCells * pathCoords.x); // RLpos
        // Vytvoří podbuňku uprostřed buňky; Z, X
        int centerPositionInArray = _emptySpotInArray + (dimensions.y * pathCoords.x) + pathCoords.y;
        _cellData.Cells[position.x, position.y].lowestSubcellIndex = _emptySpotInArray;

        Subcell centerSubcell = new Subcell(centerPositionInArray, centerPosition.x, _startPoint.y, centerPosition.y, tileType);
        _subcells[centerPositionInArray] = centerSubcell;

        // Vytváří podbuňky nad sebou až k hranici buňky; Top
        if (_cellData.Cells[position.x, position.y].IsDoor(Side.Top))
        {
            if (pathCoords.x < (dimensions.x - 1))
            {
                int positionInArray = centerPositionInArray + dimensions.y;
                _subcells[positionInArray] = new Subcell(centerPositionInArray + dimensions.y, centerPosition.x, _startPoint.y, centerPosition.y + _mazeSettings.distanceBetweenCells, tileType); // RLpos
                _subcells[positionInArray].ConnectToSubcell(centerSubcell, Side.Bottom);
            }
        }
        // Vytváří podbuňky od prostřední podbuňky napravo až k hranici buňky; Right
        if (_cellData.Cells[position.x, position.y].IsDoor(Side.Right))
        {
            if (pathCoords.y < (dimensions.y - 1))
            {
                int positionInArray = centerPositionInArray + 1;
                _subcells[positionInArray] = new Subcell(centerPositionInArray + 1, centerPosition.x + _mazeSettings.distanceBetweenCells, _startPoint.y, centerPosition.y, tileType); // RLpos
                _subcells[positionInArray].ConnectToSubcell(centerSubcell, Side.Left);
            }
        }
        // Vytváří podbuňky pod sebou až k hranici buňky; Bottom
        if (_cellData.Cells[position.x, position.y].IsDoor(Side.Bottom))
        {
            if (pathCoords.x > 0)
            {
                int positionInArray = centerPositionInArray - dimensions.y;
                _subcells[positionInArray] = new Subcell(centerPositionInArray - dimensions.y, centerPosition.x, _startPoint.y, centerPosition.y - _mazeSettings.distanceBetweenCells, tileType); // RLpos
                _subcells[positionInArray].ConnectToSubcell(centerSubcell, Side.Top);
            }
        }
        // Vytváří podbuňky od prostřední podbuňky nalevo až k hranici buňky; Left
        if (_cellData.Cells[position.x, position.y].IsDoor(Side.Left))
        {
            if (pathCoords.y > 0)
            {
                int positionInArray = centerPositionInArray - 1;
                _subcells[positionInArray] = new Subcell(centerPositionInArray - 1, centerPosition.x - _mazeSettings.distanceBetweenCells, _startPoint.y, centerPosition.y, tileType); // RLpos
                _subcells[positionInArray].ConnectToSubcell(centerSubcell, Side.Right);
            }
        }

        _emptySpotInArray += dimensions.x * dimensions.y;

        // Propojí podbuňky s podbuňkami ze sousedících buňek
        ConnectSubcellsFromDifferentCells(position, dimensions);
    }

    // Propojí podbuňky s podbuňkami ze sousedících buňek
    private void ConnectSubcellsFromDifferentCells(Vector2Int position, Vector2Int firstDimensions)
    {
        Cell cell = _cellData.Cells[position.x, position.y];
        Vector2Int paths = _cellData.GetPath(position, firstDimensions);
        Vector2Int secondDimension;
        int firstBasePositionInArray = cell.lowestSubcellIndex;
        int secondBasePositionInArray;

        // Propojí horní podbuňku se spodní podbuňkou z jiné buňky; Top
        if (cell.IsDoor(Side.Top))
        {
            if (_cellData.Cells[position.x + 1, position.y].generated)
            {
                secondBasePositionInArray = _cellData.Cells[position.x + 1, position.y].lowestSubcellIndex;
                _subcells[firstBasePositionInArray + (firstDimensions.y * (firstDimensions.x - 1)) + paths.y].ConnectToSubcell(_subcells[secondBasePositionInArray + paths.y], Side.Top);
            }
        }
        // Propojí pravou podbuňku s levou podbuňkou z jiné buňky; Right
        if (cell.IsDoor(Side.Right))
        {
            if (_cellData.Cells[position.x, position.y + 1].generated)
            {
                secondBasePositionInArray = _cellData.Cells[position.x, position.y + 1].lowestSubcellIndex;
                secondDimension = _cellData.GetDimensions(position.x, position.y + 1);
                _subcells[firstBasePositionInArray + (firstDimensions.y * paths.x) + firstDimensions.y - 1].ConnectToSubcell(_subcells[secondBasePositionInArray + (secondDimension.y * paths.x)], Side.Right);
            }
        }
        // Propojí dolní podbuňku s horní podbuňkou z jiné buňky; Bottom
        if (cell.IsDoor(Side.Bottom))
        {
            if (_cellData.Cells[position.x - 1, position.y].generated)
            {
                secondBasePositionInArray = _cellData.Cells[position.x - 1, position.y].lowestSubcellIndex;
                secondDimension = _cellData.GetDimensions(position.x - 1, position.y);
                _subcells[firstBasePositionInArray + paths.y].ConnectToSubcell(_subcells[secondBasePositionInArray + (secondDimension.y * (secondDimension.x - 1)) + paths.y], Side.Bottom);
            }
        }
        // Propojí levou podbuňku s pravou podbuňkou z jiné buňky; Left
        if (cell.IsDoor(Side.Left))
        {
            if (_cellData.Cells[position.x, position.y - 1].generated)
            {
                secondBasePositionInArray = _cellData.Cells[position.x, position.y - 1].lowestSubcellIndex;
                secondDimension = _cellData.GetDimensions(position.x, position.y - 1);
                _subcells[firstBasePositionInArray + (firstDimensions.y * paths.x)].ConnectToSubcell(_subcells[secondBasePositionInArray + (secondDimension.y * paths.x) + secondDimension.y - 1], Side.Left);
            }
        }
    }

    // Vloží okolní buňky do zásobníku
    private void PushNeighbouringCellsIntoStack(Stack<Vector2Int> stack, Vector2Int currentCellposition)
    {
        foreach (Vector2Int position in _cellData.GetNeighbouringCellPositions(currentCellposition))
        {
            stack.Push(position);
        }
    }
}
