using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField]
    private float _playerYOffset, _enemyYOffset;
    [SerializeField]
    private int _maxRandomStep;

    private Vector3 _startPoint;
    private CellData _cellData;
    private SubcellData _subcellData;
    private PathfindingNode[] _pathfindingNodes;
    private MazeSettingsSO _mazeSettings;
    private List<GenerationRule> _generationsRules; 
    private IWinCondition _winCondition;

    // Vygeneruje mapu, jako parametry přijíma Maze Settings a IWinCondition, jinými slovy pravidla, jak se má mapa generovat, a podmínku pro splnění úrovně
    public PathfindingNode[] GenerateMaze(MazeSettingsSO mazeSettings, IWinCondition winCondition, out int nodeCount)
    {
        _mazeSettings = mazeSettings;
        _winCondition = winCondition;
        _generationsRules = _winCondition.SpecialGenerationRules();

        // Vygeneruje buňky, pokud jich vygeneruje málo může generaci opakovat
        ICellGenerator cellGenerator = GetComponent<ICellGenerator>();
        _startPoint = new Vector3(_mazeSettings.centerPoint.x - ((float)_mazeSettings.width / 2f) * _mazeSettings.distanceBetweenCells, _mazeSettings.centerPoint.y, _mazeSettings.centerPoint.z - ((float)_mazeSettings.length / 2f) * _mazeSettings.distanceBetweenCells); // RLpos

        int generationCounter = 0;
        while (generationCounter < _mazeSettings.triesToGenerateMaze)
        {
            _cellData = cellGenerator.GenerateCells(_mazeSettings, _startPoint);
            if ((float)_cellData.CellCount / (float)(_mazeSettings.length * _mazeSettings.width) >= _mazeSettings.minCellPercentage)
            {
                break;
            }
            generationCounter++;
        }

        // Rozdělí buňky na podbuňky
        ISubcellGenerator subcellGenerator = GetComponent<ISubcellGenerator>();
        _subcellData = subcellGenerator.GenerateSubcells(_mazeSettings, _cellData, _startPoint, 1);

        // Pokud je u podmínky k zvítězení pravidlo na vedlejší místnost, tak ji vygeneruje
        if (_generationsRules.Contains(GenerationRule.OuterRoom))
        {
            CreateOuterRoom();
        }

        // Vytvoří na mapě části místností pro všechny podbuňky
        ITileGenerator tileGenerator = GetComponent<TileGenerator>();
        tileGenerator.GenerateTiles(_subcellData, _generationsRules.Contains(GenerationRule.OuterRoom) ? (_subcellData.EmptySpotInArray - 1) : _subcellData.EmptySpotInArray);

        // Vytvoří vrcholy pro hledání cesty
        PathfindingNodeGenerator pathfindingNodeGenerator = GetComponent<PathfindingNodeGenerator>();
        _pathfindingNodes = pathfindingNodeGenerator.GenerateNodes(_mazeSettings, _subcellData);

        GetComponent<Spawner>().SpawnReturnPortal(_subcellData.SpawnPoint);
        GameManager.Instance.Player.transform.position = new Vector3(_subcellData.SpawnPoint.x, _subcellData.SpawnPoint.y + _playerYOffset, _subcellData.SpawnPoint.z);

        SpawnEnemies();

        nodeCount = _pathfindingNodes.Length;
        return _pathfindingNodes;
    }

#if UNITY_EDITOR // Kód funguje pouze v editoru, není součástí buildu
    #region Visualization in the Editor

    // Vizualizace v editoru
    private void OnDrawGizmos()
    {
        //DrawCells();
        DrawNodes();
    }
    
    // Vykresluje pozice vrcholů k vyhledávání cesty
    private void DrawNodes()
    {
        if (_pathfindingNodes != null)
        {
            for (int i = 0; i < _pathfindingNodes.Length; i++)
            {
                if (_pathfindingNodes[i] == null)
                {
                    continue;
                }

                Vector3 start = new Vector3(_pathfindingNodes[i].Position.x, _startPoint.y - 0.1f, _pathfindingNodes[i].Position.z);
                Vector3 end;

                Gizmos.color = Color.green;

                Gizmos.DrawSphere(start, 0.3f);

                Gizmos.color = Color.yellow;

                for (int j = 0; j < 8; j++)
                {
                    if (_pathfindingNodes[i].Neighbours[j] != null)
                    {
                        end = new Vector3(_pathfindingNodes[i].Neighbours[j].Position.x, _startPoint.y - 0.1f, _pathfindingNodes[i].Neighbours[j].Position.z);
                        Gizmos.DrawLine(start, end);
                    }
                }
            }
        }
    }

    // Vykresluje pozice buňek
    private void DrawCells()
    {
        if (_cellData.Cells != null)
        {
            float xPos, zPos;

            for (int i = 0; i < _mazeSettings.length; i++)
            {
                for (int j = 0; j < _mazeSettings.width; j++)
                {
                    if (_cellData.Cells[i, j] != null)
                    {
                        Gizmos.color = Color.cyan;
                        xPos = _cellData.XDistance[j] * _mazeSettings.distanceBetweenCells;
                        zPos = _cellData.ZDistance[i] * _mazeSettings.distanceBetweenCells;

                        Gizmos.DrawSphere(new Vector3(_startPoint.x + xPos, _startPoint.y, _startPoint.z + zPos), 0.3f);

                        Gizmos.color = Color.red;
                        DrawPaths(i, j);
                    }
                }
            }
        }
    }

    // Vykresluje spojnice mezi buňkami
    private void DrawPaths(int x, int y)
    {
        float xPos = _cellData.XDistance[x] * _mazeSettings.distanceBetweenCells;
        float yPos = _cellData.ZDistance[y] * _mazeSettings.distanceBetweenCells;

        Vector3 start = new Vector3(_startPoint.x + xPos, _startPoint.y, _startPoint.z + yPos);
        Vector3 end;

        // Top
        if (_cellData.Cells[x, y].IsDoor(Side.Top))
        {
            xPos = _cellData.XDistance[x + 1] * _mazeSettings.distanceBetweenCells;
            yPos = _cellData.ZDistance[y] * _mazeSettings.distanceBetweenCells;
            end = new Vector3(_startPoint.x + xPos, _startPoint.y, _startPoint.z + yPos);
            Gizmos.DrawLine(start, end);
        }
        // Right
        if (_cellData.Cells[x, y].IsDoor(Side.Right))
        {
            xPos = _cellData.XDistance[x] * _mazeSettings.distanceBetweenCells;
            yPos = _cellData.ZDistance[y + 1] * _mazeSettings.distanceBetweenCells;
            end = new Vector3(_startPoint.x + xPos, _startPoint.y, _startPoint.z + yPos);
            Gizmos.DrawLine(start, end);
        }
        // Bottom
        if (_cellData.Cells[x, y].IsDoor(Side.Bottom))
        {
            xPos = _cellData.XDistance[x - 1] * _mazeSettings.distanceBetweenCells;
            yPos = _cellData.ZDistance[y] * _mazeSettings.distanceBetweenCells;
            end = new Vector3(_startPoint.x + xPos, _startPoint.y, _startPoint.z + yPos);
            Gizmos.DrawLine(start, end);
        }
        // Left
        if (_cellData.Cells[x, y].IsDoor(Side.Left))
        {
            xPos = _cellData.XDistance[x] * _mazeSettings.distanceBetweenCells;
            yPos = _cellData.ZDistance[y - 1] * _mazeSettings.distanceBetweenCells;
            end = new Vector3(_startPoint.x + xPos, _startPoint.y, _startPoint.z + yPos);
            Gizmos.DrawLine(start, end);
        }
    }

    #endregion
#endif

    #region Special Generation Rules

    // Vytvoří místnost na kraji mapy a prpojí ji ke zbytku 
    private void CreateOuterRoom()
    {
        int side = Random.Range(0, 4);

        Vector2Int cellPosition = GetOuterCell(side);
        Vector2Int dimensions = _cellData.GetDimensions(cellPosition);
        Vector2Int path = _cellData.GetPath(cellPosition, dimensions);
        int currenSubcellIndex = _cellData.Cells[cellPosition.x, cellPosition.y].lowestSubcellIndex + (dimensions.y * path.x) + path.y;

        Subcell outerRoomSubcell = CreateOuterRoomSubcell(currenSubcellIndex, side);

        GetComponent<TileGenerator>().SpawnSingleTile(outerRoomSubcell.Position, outerRoomSubcell.GetFirstDoor() * 90, 2, outerRoomSubcell.TileType);
        GameObject boss = GetComponent<Spawner>().SpawnBossRoom(outerRoomSubcell.Position, side, _mazeSettings.distanceBetweenCells);
        boss.GetComponent<Boss>().OnBossDeath = _winCondition.OnCompleted;
    }

    // Náhodně najde buňku, ke které může připojit vedlejší místnost
    private Vector2Int GetOuterCell(int side)
    {
        Vector2Int startPos;
        Vector2Int increment;
        Vector2Int nextLine;

        switch (side)
        {
            case 0: // TOP
                startPos = new Vector2Int(_mazeSettings.length - 1, 0);
                increment = new Vector2Int(0, 1);
                nextLine = new Vector2Int(-1, 0);
                break;
            case 1: // RIGHT
                startPos = new Vector2Int(0, _mazeSettings.width - 1);
                increment = new Vector2Int(1, 0);
                nextLine = new Vector2Int(0, -1);
                break;
            case 2: // BOTTOM
                startPos = new Vector2Int(0, 0);
                increment = new Vector2Int(0, 1);
                nextLine = new Vector2Int(1, 0);
                break;
            case 3: // LEFT
                startPos = new Vector2Int(0, 0);
                increment = new Vector2Int(1, 0);
                nextLine = new Vector2Int(0, 1);
                break;
            default:
                throw new System.Exception("Index out of bounds");
        }

        bool zAxisIsMain;
        int maxCellsInMainDirection;
        int maxCellsInSecondaryDirection;
        if (side % 2 == 0)
        {
            maxCellsInMainDirection = _mazeSettings.width;
            maxCellsInSecondaryDirection = _mazeSettings.length;
            zAxisIsMain = true;
        } else
        {
            maxCellsInMainDirection = _mazeSettings.length;
            maxCellsInSecondaryDirection = _mazeSettings.width;
            zAxisIsMain = false;
        }

        int randomOffset = Random.Range(0, maxCellsInMainDirection);
        Vector2Int currentPos = startPos;

        for (int j = 0; j < maxCellsInSecondaryDirection; j++)
        {
            for (int i = 0; i < maxCellsInMainDirection; i++)
            {
                if (zAxisIsMain)
                {
                    if (_cellData.Cells[currentPos.x, (currentPos.y + randomOffset) % maxCellsInMainDirection] != null)
                    {
                        return new Vector2Int(currentPos.x, (currentPos.y + randomOffset) % maxCellsInMainDirection);
                    }
                } else
                {
                    if (_cellData.Cells[(currentPos.x + randomOffset) % maxCellsInMainDirection, currentPos.y] != null)
                    {
                        return new Vector2Int((currentPos.x + randomOffset) % maxCellsInMainDirection, currentPos.y);
                    }
                }

                currentPos += increment;
            }
            startPos += nextLine;
        }

        throw new System.Exception("Could not find cell");
    }

    // Připojí vedlejší místnost
    private Subcell CreateOuterRoomSubcell(int subcellPositionInArray, int side)
    {
        Subcell currentSubcell = _subcellData.Subcells[subcellPositionInArray];
        Subcell newSubcell;

        switch (side)
        {
            case 0: // TOP
                while(currentSubcell.Neighbours[0] != null)
                {
                    currentSubcell = currentSubcell.Neighbours[0];
                }

                newSubcell = new Subcell(_subcellData.EmptySpotInArray, currentSubcell.Position.x, currentSubcell.Position.y, currentSubcell.Position.z + _mazeSettings.distanceBetweenCells, currentSubcell.TileType);
                currentSubcell.ConnectToSubcell(newSubcell, Side.Top);
                break;
            case 1: // RIGHT
                while (currentSubcell.Neighbours[2] != null)
                {
                    currentSubcell = currentSubcell.Neighbours[2];
                }

                newSubcell = new Subcell(_subcellData.EmptySpotInArray, currentSubcell.Position.x + _mazeSettings.distanceBetweenCells, currentSubcell.Position.y, currentSubcell.Position.z, currentSubcell.TileType);
                currentSubcell.ConnectToSubcell(newSubcell, Side.Right);
                break;
            case 2: // BOTTOM
                while (currentSubcell.Neighbours[4] != null)
                {
                    currentSubcell = currentSubcell.Neighbours[4];
                }

                newSubcell = new Subcell(_subcellData.EmptySpotInArray, currentSubcell.Position.x, currentSubcell.Position.y, currentSubcell.Position.z - _mazeSettings.distanceBetweenCells, currentSubcell.TileType);
                currentSubcell.ConnectToSubcell(newSubcell, Side.Bottom);
                break;
            case 3: // LEFT
                while (currentSubcell.Neighbours[6] != null)
                {
                    currentSubcell = currentSubcell.Neighbours[6];
                }

                newSubcell = new Subcell(_subcellData.EmptySpotInArray, currentSubcell.Position.x - _mazeSettings.distanceBetweenCells, currentSubcell.Position.y, currentSubcell.Position.z, currentSubcell.TileType);
                currentSubcell.ConnectToSubcell(newSubcell, Side.Left);
                
                break;
            default:
                throw new System.Exception("Index out of bounds");
        }

        _subcellData.Subcells[_subcellData.EmptySpotInArray] = newSubcell;
        _subcellData.EmptySpotInArray++;

        return newSubcell;
    }

    #endregion

    // Přídá na mapu nepřátele
    private void SpawnEnemies()
    {
        List<Vector3> positionsToSpawn = new List<Vector3>();

        float minEnemyPercentage = (float)(_mazeSettings.minEnemyCount + 1) / (float)_pathfindingNodes.Length;

        for (int i = 0; i < _pathfindingNodes.Length; i++)
        {
            if (_pathfindingNodes[i] != null)
            {
                if (Random.Range(0f, 1f) <= _mazeSettings.spawnChance)
                {
                    positionsToSpawn.Add(new Vector3(_pathfindingNodes[i].Position.x, _pathfindingNodes[i].Position.y + _enemyYOffset, _pathfindingNodes[i].Position.z));
                    if(positionsToSpawn.Count == _mazeSettings.maxEnemyCount)
                    {
                        break;
                    }
                } else if (((float)positionsToSpawn.Count + 1f) / ((float)i + 1f) < minEnemyPercentage)
                {
                    positionsToSpawn.Add(new Vector3(_pathfindingNodes[i].Position.x, _pathfindingNodes[i].Position.y + _enemyYOffset, _pathfindingNodes[i].Position.z));
                }
            }
        }

        if (positionsToSpawn.Count < _mazeSettings.minEnemyCount)
        {
            for (int i = _pathfindingNodes.Length - 1; i > 0; i--)
            {
                if (_pathfindingNodes[i] != null)
                {
                    positionsToSpawn.Add(new Vector3(_pathfindingNodes[i].Position.x, _pathfindingNodes[i].Position.y + _enemyYOffset, _pathfindingNodes[i].Position.z));
                    i -= Random.Range(0, _maxRandomStep);

                    if (positionsToSpawn.Count == _mazeSettings.minEnemyCount)
                    {
                        break;
                    }
                }
            }
        }

        positionsToSpawn = _winCondition.ConfirmSpawnLocations(positionsToSpawn);

        Spawner spawner = GetComponent<Spawner>();

        foreach (Vector3 position in positionsToSpawn)
        {
            spawner.SpawnEnemy(position);
        }
    }
}