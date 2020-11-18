using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    private TilesSO[] _tiles;
    private GameObject _enemyPrefab;

    private Vector3 _spawnPoint;
    private Vector3 _startPoint;
    private CellData _cellData;
    private int _currentEmptyNode;
    private PathfindingNode[] _pathfindingNodes;
    private MazeSettingsSO _mazeSettings;
    private List<GenerationRule> _generationsRules; 
    private int _nodeIDconnectedToOuterRoom;
    private IWinCondition _winCondition;

    private void Awake()
    {
        _enemyPrefab = Resources.Load<GameObject>("Maze/TempEnemy"); // TODO change?
        _tiles = Resources.LoadAll<TilesSO>("Maze");
        // TODO order by id?
    }

    // Start is called before the first frame update
    public PathfindingNode[] GenerateMaze(MazeSettingsSO mazeSettings, IWinCondition winCondition, out int nodeCount) // TODO IWIN, IWIN as getcomponent?;
    {
        _mazeSettings = mazeSettings;
        _winCondition = winCondition;
        _generationsRules = _winCondition.SpecialGenerationRules();

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
        //Debug.Log("Tries: " + generationCounter);
        Debug.Log("Max Nodes: " + _cellData.MaximumNodeCount);

        CreateNodes();

        if (_generationsRules.Contains(GenerationRule.OuterRoom))
        {
            CreateOuterRoom();
        }

        SpawnTiles();

        Debug.Log("Generation Done");

        // TODO remove or move?
        GetComponent<Spawner>().SpawnReturnPortal(_spawnPoint);
        GameManager.Instance.Player.transform.position = new Vector3(_spawnPoint.x, _spawnPoint.y + 2, _spawnPoint.z); // TODO doesnt work?

        // TODO make better & move?
        SpawnEnemies();

        Destroy(this); // TODO uncomment?
        nodeCount = _currentEmptyNode;
        return _pathfindingNodes;
    }

    // TODO remove
    #region Visualization in Editor

    private void OnDrawGizmos()
    {
        /*
        DrawCells();
        */

        DrawNodes();
    }

    private void DrawNodes()
    {
        if (_pathfindingNodes != null)
        {
            for (int i = 0; i < _currentEmptyNode; i++)
            {
                if (_pathfindingNodes[i] == null)
                {
                    continue;
                }

                Vector3 start = new Vector3(_pathfindingNodes[i].position.x, _startPoint.y - 0.1f, _pathfindingNodes[i].position.y);
                Vector3 end;

                Gizmos.color = Color.green;

                Gizmos.DrawSphere(start, 0.3f);

                Gizmos.color = Color.yellow;

                for (int j = 0; j < 8; j++)
                {
                    if (_pathfindingNodes[i].neighbours[j] != null)
                    {
                        end = new Vector3(_pathfindingNodes[i].neighbours[j].position.x, _startPoint.y - 0.1f, _pathfindingNodes[i].neighbours[j].position.y);
                        Gizmos.DrawLine(start, end);
                    }
                }
            }
        }
    }
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

    #region Node Generation

    private void CreateNodes()
    {
        _currentEmptyNode = 0;
        _pathfindingNodes = new PathfindingNode[_cellData.MaximumNodeCount + 1];
        Stack<Vector2Int> cellStack = new Stack<Vector2Int>();

        // First Cell
        _spawnPoint = new Vector3(_startPoint.x + (_cellData.XDistance[_cellData.FirstCell.y]) * _mazeSettings.distanceBetweenCells, _startPoint.y, _startPoint.z + (_cellData.ZDistance[_cellData.FirstCell.x]) * _mazeSettings.distanceBetweenCells); // TODO only points to the right cell; rework hardcoded?; TODO spawns somwhere else // RLpos
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
                //Debug.Log("Room");
            }
            else
            {
                CreateCorridor(currentCell);
                //Debug.Log("Corridor");
            }

            PushNeighbouringCellsIntoStack(cellStack, currentCell);
            _cellData.Cells[currentCell.x, currentCell.y].generated = true;
        }
    }

    private void CreateRoom(Vector2Int position)
    {
        int tileType = _mazeSettings.roomTileTypes[Random.Range(0, _mazeSettings.roomTileTypes.Length)];

        Vector2Int dimensions = GetDimensions(position);
        // X, Z
        Vector2 realPosition = new Vector2(_startPoint.x + _cellData.XDistance[position.y] * _mazeSettings.distanceBetweenCells, _startPoint.z + _cellData.ZDistance[position.x] * _mazeSettings.distanceBetweenCells); // RLpos
        _cellData.Cells[position.x, position.y].lowestPathfindingNodeID = _currentEmptyNode;

        // From bottom left corner 
        for (int i = 0; i < dimensions.x; i++) // Z
        {
            for (int j = 0; j < dimensions.y; j++) // X
            {
                _pathfindingNodes[_currentEmptyNode] = new PathfindingNode(_currentEmptyNode, realPosition.x + _mazeSettings.distanceBetweenCells * j, _startPoint.y, realPosition.y + _mazeSettings.distanceBetweenCells * i, tileType); // RLpos

                // 2nd column + (Y)
                if (j > 0)
                {
                    ConnectTwoNodes(_pathfindingNodes[_currentEmptyNode], _pathfindingNodes[_currentEmptyNode - 1], Side.Left);
                }

                // 2nd row + (X)
                if (i > 0)
                {
                    ConnectTwoNodes(_pathfindingNodes[_currentEmptyNode], _pathfindingNodes[_currentEmptyNode - dimensions.y], Side.Bottom);

                    // Diagonal ones
                    if (j > 0)
                    {
                        ConnectTwoNodes(_pathfindingNodes[_currentEmptyNode], _pathfindingNodes[_currentEmptyNode - dimensions.y - 1], Side.BottomLeft);
                    }

                    if (j < dimensions.y - 1)
                    {
                        ConnectTwoNodes(_pathfindingNodes[_currentEmptyNode], _pathfindingNodes[_currentEmptyNode - dimensions.y + 1], Side.BottomRight);
                    }
                }

                _currentEmptyNode++;

                // TODO prob move somwhere else
                //Instantiate(floor, new Vector3(startPoint.x + realPosition.x + i * distanceBetweenCells, startPoint.y, startPoint.z + realPosition.y + j * distanceBetweenCells), Quaternion.identity);
            }
        }

        // Connect nodes with surrounding Cells
        ConnectNodesFromDifferentCells(position, dimensions);

        // TODO walls (spawn different tiles), spawn items, spawn enemies; prob do later
    }

    // Creates corridor
    private void CreateCorridor(Vector2Int position)
    {
        int tileType = _mazeSettings.corridorTileTypes[Random.Range(0, _mazeSettings.corridorTileTypes.Length)];

        // X, Z
        Vector2 realPosition = new Vector2(_startPoint.x + _cellData.XDistance[position.y] * _mazeSettings.distanceBetweenCells, _startPoint.z + _cellData.ZDistance[position.x] * _mazeSettings.distanceBetweenCells); // RLpos
        Vector2Int dimensions = GetDimensions(position);
        Vector2Int pathCoords = GetPath(position, dimensions);
        Vector2 centerPosition = new Vector2(realPosition.x + _mazeSettings.distanceBetweenCells * pathCoords.y, realPosition.y + _mazeSettings.distanceBetweenCells * pathCoords.x); // RLpos
        // Z, X
        int centerID = _currentEmptyNode + (dimensions.y * pathCoords.x) + pathCoords.y;
        _cellData.Cells[position.x, position.y].lowestPathfindingNodeID = _currentEmptyNode;

        PathfindingNode centerNode = new PathfindingNode(centerID, centerPosition.x, _startPoint.y ,centerPosition.y, tileType);
        _pathfindingNodes[centerID] = centerNode;

        // Top
        if (_cellData.Cells[position.x, position.y].IsDoor(Side.Top))
        {
            if (pathCoords.x < (dimensions.x - 1))
            {
                int id = centerID + dimensions.y;
                _pathfindingNodes[id] = new PathfindingNode(centerID + dimensions.y, centerPosition.x, _startPoint.y, centerPosition.y + _mazeSettings.distanceBetweenCells, tileType); // RLpos
                ConnectTwoNodes(_pathfindingNodes[id], centerNode, Side.Bottom);
            }
        }
        // Right
        if (_cellData.Cells[position.x, position.y].IsDoor(Side.Right))
        {
            if (pathCoords.y < (dimensions.y - 1))
            {
                int id = centerID + 1;
                _pathfindingNodes[id] = new PathfindingNode(centerID + 1, centerPosition.x + _mazeSettings.distanceBetweenCells, _startPoint.y, centerPosition.y, tileType); // RLpos
                ConnectTwoNodes(_pathfindingNodes[id], centerNode, Side.Left);
            }
        }
        // Bottom
        if (_cellData.Cells[position.x, position.y].IsDoor(Side.Bottom))
        {
            if (pathCoords.x > 0)
            {
                int id = centerID - dimensions.y;
                _pathfindingNodes[id] = new PathfindingNode(centerID - dimensions.y, centerPosition.x, _startPoint.y, centerPosition.y - _mazeSettings.distanceBetweenCells, tileType); // RLpos
                ConnectTwoNodes(_pathfindingNodes[id], centerNode, Side.Top);
            }
        }
        // Left
        if (_cellData.Cells[position.x, position.y].IsDoor(Side.Left))
        {
            if (pathCoords.y > 0)
            {
                int id = centerID - 1;
                _pathfindingNodes[id] = new PathfindingNode(centerID - 1, centerPosition.x - _mazeSettings.distanceBetweenCells, _startPoint.y, centerPosition.y, tileType); // RLpos
                ConnectTwoNodes(_pathfindingNodes[id], centerNode, Side.Right);
            }
        }

        _currentEmptyNode += dimensions.x * dimensions.y;

        // Connect nodes with surrounding Cells
        ConnectNodesFromDifferentCells(position, dimensions);

        // TODO walls (spawn different tiles), spawn items, spawn enemies; prob do later
    }

    private Vector2Int GetDimensions(Vector2Int position)
    {
        return new Vector2Int(_cellData.ZDistance[position.x + 1] - _cellData.ZDistance[position.x], _cellData.XDistance[position.y + 1] - _cellData.XDistance[position.y]);
    }

    private Vector2Int GetDimensions(int positionZ, int positionX)
    {
        return new Vector2Int(_cellData.ZDistance[positionZ + 1] - _cellData.ZDistance[positionZ], _cellData.XDistance[positionX + 1] - _cellData.XDistance[positionX]);
    }

    private Vector2Int GetPath(Vector2Int position, Vector2Int dimension)
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
                throw new System.Exception("Rework needed to support larger rooms");
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
                throw new System.Exception("Rework needed to support larger rooms");
        }

        return result;
    }

    private void ConnectTwoNodes(PathfindingNode first, PathfindingNode second, Side direction)
    {
        switch (direction)
        {
            case Side.Top:
                first.neighbours[0] = second;
                second.neighbours[4] = first;
                break;
            case Side.Right:
                first.neighbours[2] = second;
                second.neighbours[6] = first;
                break;
            case Side.Bottom:
                first.neighbours[4] = second;
                second.neighbours[0] = first;
                break;
            case Side.Left:
                first.neighbours[6] = second;
                second.neighbours[2] = first;
                break;
            case Side.TopRight:
                first.neighbours[1] = second;
                second.neighbours[5] = first;
                break;
            case Side.BottomRight:
                first.neighbours[3] = second;
                second.neighbours[7] = first;
                break;
            case Side.BottomLeft:
                first.neighbours[5] = second;
                second.neighbours[1] = first;
                break;
            case Side.TopLeft:
                first.neighbours[7] = second;
                second.neighbours[3] = first;
                break;
        }
    }

    // Connects nodes between cells
    private void ConnectNodesFromDifferentCells(Vector2Int position, Vector2Int firstDimensions)
    {
        Cell cell = _cellData.Cells[position.x, position.y];
        Vector2Int paths = GetPath(position, firstDimensions);
        Vector2Int secondDimension;
        int firstBaseID = cell.lowestPathfindingNodeID;
        int secondBaseID;

        //Debug.Log("pos " + position.x + ", " + position.y);
        //Debug.Log(cell.ToString2());
        // Top
        if (cell.IsDoor(Side.Top))
        {
            if (_cellData.Cells[position.x + 1, position.y].generated)
            {
                secondBaseID = _cellData.Cells[position.x + 1, position.y].lowestPathfindingNodeID;
                ConnectTwoNodes(_pathfindingNodes[firstBaseID + (firstDimensions.y * (firstDimensions.x - 1)) + paths.y], _pathfindingNodes[secondBaseID + paths.y], Side.Top);
            }
        }
        // Right
        if (cell.IsDoor(Side.Right))
        {
            if (_cellData.Cells[position.x, position.y + 1].generated)
            {
                secondBaseID = _cellData.Cells[position.x, position.y + 1].lowestPathfindingNodeID;
                secondDimension = GetDimensions(position.x, position.y + 1);
                ConnectTwoNodes(_pathfindingNodes[firstBaseID + (firstDimensions.y * paths.x) + firstDimensions.y - 1], _pathfindingNodes[secondBaseID + (secondDimension.y * paths.x)], Side.Right);
            }
        }
        // Bottom
        if (cell.IsDoor(Side.Bottom))
        {
            if (_cellData.Cells[position.x - 1, position.y].generated)
            {
                secondBaseID = _cellData.Cells[position.x - 1, position.y].lowestPathfindingNodeID;
                secondDimension = GetDimensions(position.x - 1, position.y);
                ConnectTwoNodes(_pathfindingNodes[firstBaseID + paths.y], _pathfindingNodes[secondBaseID + (secondDimension.y * (secondDimension.x - 1)) + paths.y], Side.Bottom);
            }
        }
        // Left
        if (cell.IsDoor(Side.Left))
        {
            if (_cellData.Cells[position.x, position.y - 1].generated)
            {
                secondBaseID = _cellData.Cells[position.x, position.y - 1].lowestPathfindingNodeID;
                secondDimension = GetDimensions(position.x, position.y - 1);
                ConnectTwoNodes(_pathfindingNodes[firstBaseID + (firstDimensions.y * paths.x)], _pathfindingNodes[secondBaseID + (secondDimension.y * paths.x) + secondDimension.y - 1], Side.Left);
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

    #endregion

    #region Tile Spawning

    private void SpawnTiles()
    {
        Vector2 dimensions;

        for (int i = 0; i < _mazeSettings.length; i++)
        {
            for (int j = 0; j < _mazeSettings.width; j++)
            {
                if (_cellData.Cells[i, j] != null)
                {

                    dimensions = GetDimensions(i, j);
                    for (int k = 0; k < dimensions.x * dimensions.y; k++)
                    {
                        SpawnTilePrefab(_pathfindingNodes[_cellData.Cells[i, j].lowestPathfindingNodeID + k]);
                    }
                }
            }
        }
    }

    // TODO add int with tileset type
    private void SpawnTilePrefab(PathfindingNode node)
    {
        if (node != null)
        {
            int firstDoor = node.GetFirstDoor();

            switch (node.GetDoorCount())
            {
                case 1:
                    Instantiate(_tiles[node.TileType].tiles[0], node.position, Quaternion.Euler(new Vector3(0, firstDoor * 90, 0)), transform);
                    break;
                case 2:
                    if (node.neighbours[((firstDoor + 2) * 2) % 8] == null)
                    {
                        Instantiate(_tiles[node.TileType].tiles[1], node.position, Quaternion.Euler(new Vector3(0, firstDoor * 90, 0)), transform);
                    }
                    else
                    {
                        Instantiate(_tiles[node.TileType].tiles[2], node.position, Quaternion.Euler(new Vector3(0, firstDoor * 90, 0)), transform);
                    }
                    break;
                case 3:
                    Instantiate(_tiles[node.TileType].tiles[3], node.position, Quaternion.Euler(new Vector3(0, (firstDoor - 1) * 90, 0)), transform);
                    break;
                case 4:
                    Instantiate(_tiles[node.TileType].tiles[4], node.position, Quaternion.identity, transform);
                    break;
                default:
                    throw new System.Exception("Cell can't have more than four doors");
            }
        }
    }

    // TODO remove
    /*private void SpawnTilePrefab(PathfindingNode node, float yOffset)
    {
        if (node != null)
        {
            int firstDoor = node.GetFirstDoor();

            switch (node.GetDoorCount())
            {
                case 1:
                    Instantiate(_tiles[node.TileType].tiles[0], new Vector3(node.position.x, node.position.y + yOffset, node.position.z), Quaternion.Euler(new Vector3(0, firstDoor * 90, 0)), transform);
                    break;
                case 2:
                    if (node.neighbours[((firstDoor + 2) * 2) % 8] == null)
                    {
                        Instantiate(_tiles[node.TileType].tiles[1], new Vector3(node.position.x, node.position.y + yOffset, node.position.z), Quaternion.Euler(new Vector3(0, firstDoor * 90, 0)), transform);
                    }
                    else
                    {
                        Instantiate(_tiles[node.TileType].tiles[2], new Vector3(node.position.x, node.position.y + yOffset, node.position.z), Quaternion.Euler(new Vector3(0, firstDoor * 90, 0)), transform);
                    }
                    break;
                case 3:
                    Instantiate(_tiles[node.TileType].tiles[3], new Vector3(node.position.x, node.position.y + yOffset, node.position.z), Quaternion.Euler(new Vector3(0, (firstDoor - 1) * 90, 0)), transform);
                    break;
                case 4:
                    Instantiate(_tiles[node.TileType].tiles[4], new Vector3(node.position.x, node.position.y + yOffset, node.position.z), Quaternion.identity, transform);
                    break;
                default:
                    throw new System.Exception("Cell can't have more than four doors");
            }
        }
    }*/

    #endregion

    #region Special Generation Rules

    private void CreateOuterRoom()
    {
        int side = Random.Range(0, 4);

        Vector2Int cellPosition = GetOuterCell(side);
        Vector2Int dimensions = GetDimensions(cellPosition);
        Vector2Int path = GetPath(cellPosition, dimensions);
        int currentNodeID = _cellData.Cells[cellPosition.x, cellPosition.y].lowestPathfindingNodeID + (dimensions.y * path.x) + path.y;

        Vector3 outerRoomPosition = CreateOuterRoom(currentNodeID, side);

        // TODO instantiate boss room and doors to the room
        GetComponent<Spawner>().SpawnBossRoom(outerRoomPosition, side, _mazeSettings.distanceBetweenCells);
    }

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

    private Vector3 CreateOuterRoom(int nodeID, int side)
    {
        PathfindingNode currentNode = _pathfindingNodes[nodeID];
        PathfindingNode newNode;

        switch (side)
        {
            case 0: // TOP
                while(currentNode.neighbours[0] != null)
                {
                    currentNode = currentNode.neighbours[0];
                }

                newNode = new PathfindingNode(_currentEmptyNode, currentNode.position.x, currentNode.position.y, currentNode.position.z + _mazeSettings.distanceBetweenCells, currentNode.TileType);
                ConnectTwoNodes(currentNode, newNode, Side.Top);
                break;
            case 1: // RIGHT
                while (currentNode.neighbours[2] != null)
                {
                    currentNode = currentNode.neighbours[2];
                }

                newNode = new PathfindingNode(_currentEmptyNode, currentNode.position.x + _mazeSettings.distanceBetweenCells, currentNode.position.y, currentNode.position.z, currentNode.TileType);
                ConnectTwoNodes(currentNode, newNode, Side.Right);
                break;
            case 2: // BOTTOM
                while (currentNode.neighbours[4] != null)
                {
                    currentNode = currentNode.neighbours[4];
                }

                newNode = new PathfindingNode(_currentEmptyNode, currentNode.position.x, currentNode.position.y, currentNode.position.z - _mazeSettings.distanceBetweenCells, currentNode.TileType);
                ConnectTwoNodes(currentNode, newNode, Side.Bottom);
                break;
            case 3: // LEFT
                while (currentNode.neighbours[6] != null)
                {
                    currentNode = currentNode.neighbours[6];
                }

                newNode = new PathfindingNode(_currentEmptyNode, currentNode.position.x - _mazeSettings.distanceBetweenCells, currentNode.position.y, currentNode.position.z, currentNode.TileType);
                ConnectTwoNodes(currentNode, newNode, Side.Left);
                
                break;
            default:
                throw new System.Exception("Index out of bounds");
        }

        int rotation = (side % 2 == 0) ? 0 : 1;
        //Instantiate(_tiles[newNode.TileType].tiles[2], new Vector3(newNode.position.x, newNode.position.y + 2, newNode.position.z), Quaternion.Euler(new Vector3(0, rotation * 90, 0)), transform); // TODO remove
        Instantiate(_tiles[newNode.TileType].tiles[2], newNode.position, Quaternion.Euler(new Vector3(0, rotation * 90, 0)), transform);

        _currentEmptyNode++;

        return newNode.position;
    }

    #endregion

    private void SpawnEnemies()
    {
        List<Vector3> positionsToSpawn = new List<Vector3>();
        for (int i = 0; i < _currentEmptyNode; i++)
        {
            if (_pathfindingNodes[i] != null)
            {
                if (Random.Range(0f, 1f) <= _mazeSettings.spawnChance)
                {
                    positionsToSpawn.Add(new Vector3(_pathfindingNodes[i].position.x, _pathfindingNodes[i].position.y + 0.5f, _pathfindingNodes[i].position.z)); // TODO hardcoded
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