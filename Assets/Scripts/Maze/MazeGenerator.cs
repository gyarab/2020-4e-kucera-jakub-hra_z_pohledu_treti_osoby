using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    // TODO remove
    [Header("Debug")]
    public float sphereRadius = 0.1f;

    [Header("Prefabs")]
    public GameObject floor;

    private Vector3 _spawnPoint;
    // TODO rework
    public float spawnChance; // TODO move to MazeSettings
    public GameObject enemyPrefab; // TODO add variation

    // TODO remove SerializeField?
    [SerializeField]
    private TilesSO tiles; // TODO change to field?

    private Stack<Vector2Int> cellStack;
    private List<int[]> paths;
    private Vector3 startPoint;
    private Cell[,] cells;
    private int[] xDistance, zDistance;
    private Vector2Int firstCell;
    private int maxNodeCount;
    private int currentEmptyNode;
    private PathfindingNode[] pathfindingNodes;
    private Transform _parent;
    private MazeSettingsSO _mazeSettings;

    // TODO remove
    [SerializeField]
    private int _debug;

    private void Start()
    {
        GameManager.Instance.MazeGen = this;
    }

    // Start is called before the first frame update
    public void GenerateMaze(MazeSettingsSO mazeSettings)
    {
        _mazeSettings = mazeSettings;

        // TODO move somwhere else or add initialization
        int generationCounter = 0;
        while (generationCounter < _mazeSettings.triesToGenerateMaze)
        {
            if ((float)GenerateCells() / (float)(_mazeSettings.length * _mazeSettings.width) >= _mazeSettings.minTilesPercentage)
            {
                break;
            }
            generationCounter++;
        }
        //Debug.Log("Tries: " + generationCounter);
        Debug.Log("Max Nodes: " + maxNodeCount);

        CreateNodes();

        SpawnTiles();

        Debug.Log("Generation Done");

        _parent.gameObject.AddComponent<Pathfinding>();
        _parent.GetComponent<Pathfinding>().SetVariables(pathfindingNodes, maxNodeCount); // TODO change?
        GameManager.Instance.Pathfinding = _parent.GetComponent<Pathfinding>();

        // TODO remove?
        GameManager.Instance.Player.transform.position = _spawnPoint;

        // TODO make better & move?
        for (int i = 0; i < currentEmptyNode; i++)
        {
            if (pathfindingNodes[i] != null) {
                if (Random.Range(0f, 1f) <= spawnChance)
                {
                    Instantiate(enemyPrefab, new Vector3(pathfindingNodes[i].position.x , startPoint.y + 1, pathfindingNodes[i].position.y), Quaternion.identity);
                }
            }
        }

        GameManager.Instance.MazeGen = null;
        Destroy(this);
    }

    // TODO remove
    #region Visualization in Editor

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(new Vector3(startPoint.x, startPoint.y, startPoint.z), sphereRadius * 5);

        // draw cells
        /*if (cells != null)
        {
            float xPos, zPos;

            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (cells[i, j] != null)
                    {
                        Gizmos.color = Color.cyan;
                        xPos = xDistance[j] * distanceBetweenCells;
                        zPos = zDistance[i] * distanceBetweenCells;

                        Gizmos.DrawSphere(new Vector3(startPoint.x + xPos, startPoint.y, startPoint.z + zPos), sphereRadius);

                        Gizmos.color = Color.red;
                        DrawPaths(i, j);
                    }
                }
            }
        }*/

        if (pathfindingNodes != null)
        {
            for (int i = 0; i < currentEmptyNode; i++)
            {
                if (pathfindingNodes[i] == null)
                {
                    continue;
                }

                Vector3 start = new Vector3(pathfindingNodes[i].position.x, startPoint.y - 0.1f, pathfindingNodes[i].position.y);
                Vector3 end;

                Gizmos.color = Color.green;
                if (pathfindingNodes[i].neighbours[_debug] != null)
                    Gizmos.color = Color.magenta;

                Gizmos.DrawSphere(start, sphereRadius);

                Gizmos.color = Color.yellow;

                for (int j = 0; j < 8; j++)
                {
                    if (pathfindingNodes[i].neighbours[j] != null)
                    {
                        end = new Vector3(pathfindingNodes[i].neighbours[j].position.x, startPoint.y - 0.1f, pathfindingNodes[i].neighbours[j].position.y);
                        Gizmos.DrawLine(start, end);
                    }
                }
            }
        }
    }

    private void DrawPaths(int x, int y)
    {
        float xPos = xDistance[x] * _mazeSettings.distanceBetweenCells;
        float yPos = zDistance[y] * _mazeSettings.distanceBetweenCells;

        Vector3 start = new Vector3(startPoint.x + xPos, startPoint.y, startPoint.z + yPos);
        Vector3 end;

        // Top
        if (cells[x, y].IsDoor(Side.Top))
        {
            xPos = xDistance[x + 1] * _mazeSettings.distanceBetweenCells;
            yPos = zDistance[y] * _mazeSettings.distanceBetweenCells;
            end = new Vector3(startPoint.x + xPos, startPoint.y, startPoint.z + yPos);
            Gizmos.DrawLine(start, end);
        }
        // Right
        if (cells[x, y].IsDoor(Side.Right))
        {
            xPos = xDistance[x] * _mazeSettings.distanceBetweenCells;
            yPos = zDistance[y + 1] * _mazeSettings.distanceBetweenCells;
            end = new Vector3(startPoint.x + xPos, startPoint.y, startPoint.z + yPos);
            Gizmos.DrawLine(start, end);
        }
        // Bottom
        if (cells[x, y].IsDoor(Side.Bottom))
        {
            xPos = xDistance[x - 1] * _mazeSettings.distanceBetweenCells;
            yPos = zDistance[y] * _mazeSettings.distanceBetweenCells;
            end = new Vector3(startPoint.x + xPos, startPoint.y, startPoint.z + yPos);
            Gizmos.DrawLine(start, end);
        }
        // Left
        if (cells[x, y].IsDoor(Side.Left))
        {
            xPos = xDistance[x] * _mazeSettings.distanceBetweenCells;
            yPos = zDistance[y - 1] * _mazeSettings.distanceBetweenCells;
            end = new Vector3(startPoint.x + xPos, startPoint.y, startPoint.z + yPos);
            Gizmos.DrawLine(start, end);
        }
    }

    #endregion

    #region Generation
    private int GenerateCells()
    {
        int tileCounter = 1;
        // Z, X
        cells = new Cell[_mazeSettings.length, _mazeSettings.width];
        startPoint = new Vector3(_mazeSettings.centerPoint.x - ((float)_mazeSettings.width / 2f) * _mazeSettings.distanceBetweenCells, _mazeSettings.centerPoint.y, _mazeSettings.centerPoint.z - ((float)_mazeSettings.length / 2f) * _mazeSettings.distanceBetweenCells); // RLpos
        cellStack = new Stack<Vector2Int>();
        xDistance = new int[_mazeSettings.width + 1];
        zDistance = new int[_mazeSettings.length + 1];

        zDistance[0] = xDistance[0] = 0;
        for (int i = 1; i < _mazeSettings.length + 1; i++)
        {
            if (_mazeSettings.randomDistanceBetweenCells)
            {
                zDistance[i] = zDistance[i - 1] + Random.Range(_mazeSettings.minDistanceMultiplyer, _mazeSettings.maxDistanceMultiplyer);
            }
            else
            {
                zDistance[i] = i;
            }
        }

        for (int i = 1; i < _mazeSettings.width + 1; i++)
        {
            if (_mazeSettings.randomDistanceBetweenCells)
            {
                xDistance[i] = xDistance[i - 1] + Random.Range(_mazeSettings.minDistanceMultiplyer, _mazeSettings.maxDistanceMultiplyer);
            }
            else
            {
                xDistance[i] = i;
            }
        }

        // First Cell - add all possible neighbours, Z, X
        Vector2Int currentCellPositionInArray = firstCell = new Vector2Int(Random.Range(1, _mazeSettings.length - 1), Random.Range(1, _mazeSettings.width - 1));
        cells[currentCellPositionInArray.x, currentCellPositionInArray.y] = new Cell(true, true, true, true);
        PushNeighbouringCells(currentCellPositionInArray);
        maxNodeCount = (zDistance[currentCellPositionInArray.x + 1] - zDistance[currentCellPositionInArray.x]) * (xDistance[currentCellPositionInArray.y + 1] - xDistance[currentCellPositionInArray.y]);

        // All other Cells
        while (cellStack.Count > 0)
        {
            currentCellPositionInArray = cellStack.Pop();
            if (cells[currentCellPositionInArray.x, currentCellPositionInArray.y] != null)
            {
                continue;
            }
            CreateNewCell(currentCellPositionInArray);
            CreateNewDoors(currentCellPositionInArray);
            PushNeighbouringCells(currentCellPositionInArray);
            tileCounter++;
            maxNodeCount += (zDistance[currentCellPositionInArray.x + 1] - zDistance[currentCellPositionInArray.x]) * (xDistance[currentCellPositionInArray.y + 1] - xDistance[currentCellPositionInArray.y]);
        }

        return tileCounter;
    }

    // TODO move somwhere else, cuz used from other region
    // Pushes neighbouring Cells into Stack
    private void PushNeighbouringCells(Vector2Int position)
    {
        Cell cell = cells[position.x, position.y];

        if (cell.IsDoor(Side.Top))
        {
            cellStack.Push(new Vector2Int(position.x + 1, position.y));
        }
        if (cell.IsDoor(Side.Right))
        {
            cellStack.Push(new Vector2Int(position.x, position.y + 1));
        }
        if (cell.IsDoor(Side.Bottom))
        {
            cellStack.Push(new Vector2Int(position.x - 1, position.y));
        }
        if (cell.IsDoor(Side.Left))
        {
            cellStack.Push(new Vector2Int(position.x, position.y - 1));
        }
    }

    // Creates new Cell and copies the door positions of surrounding Cells
    private void CreateNewCell(Vector2Int position)
    {
        Cell cell = new Cell();

        // Top - Z
        if (position.x < _mazeSettings.length - 1)
        {
            if (cells[position.x + 1, position.y] != null)
            {
                if (cells[position.x + 1, position.y].IsDoor(Side.Bottom))
                {
                    cell.OpenWall(Side.Top);
                }
            }
        }
        // Right - X
        if (position.y < _mazeSettings.width - 1)
        {
            if (cells[position.x, position.y + 1] != null)
            {
                if (cells[position.x, position.y + 1].IsDoor(Side.Left))
                {
                    cell.OpenWall(Side.Right);
                }
            }
        }
        // Bottom - Z
        if (position.x > 0)
        {
            if (cells[position.x - 1, position.y] != null)
            {
                if (cells[position.x - 1, position.y].IsDoor(Side.Top))
                {
                    cell.OpenWall(Side.Bottom);
                }
            }
        }
        // Left - X
        if (position.y > 0)
        {
            if (cells[position.x, position.y - 1] != null)
            {
                if (cells[position.x, position.y - 1].IsDoor(Side.Right))
                {
                    cell.OpenWall(Side.Left);
                }
            }
        }

        cells[position.x, position.y] = cell;
    }

    // Try to create new doors
    private void CreateNewDoors(Vector2Int position)
    {
        int doorCount = cells[position.x, position.y].GetDoorCount();
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
                        if (cells[position.x + 1, position.y] == null)
                        {
                            if (Random.Range(0f, 1) <= _mazeSettings.doorDirectionChance[0] * _mazeSettings.doorChanceFallOff[doorCount])
                            {
                                doorCount++;
                                cells[position.x, position.y].OpenWall(Side.Top);
                            }
                        }
                    }
                    break;
                case 1:
                    // Rigth
                    if (position.y < _mazeSettings.width - 1)
                    {
                        if (cells[position.x, position.y + 1] == null)
                        {
                            if (Random.Range(0f, 1) <= _mazeSettings.doorDirectionChance[1] * _mazeSettings.doorChanceFallOff[doorCount])
                            {
                                doorCount++;
                                cells[position.x, position.y].OpenWall(Side.Right);
                            }
                        }
                    }
                    break;
                case 2:
                    // bottom
                    if (position.x > 0)
                    {
                        if (cells[position.x - 1, position.y] == null)
                        {
                            if (Random.Range(0f, 1) <= _mazeSettings.doorDirectionChance[2] * _mazeSettings.doorChanceFallOff[doorCount])
                            {
                                doorCount++;
                                cells[position.x, position.y].OpenWall(Side.Bottom);
                            }
                        }
                    }
                    break;
                case 3:
                    // Left
                    if (position.y > 0)
                    {
                        if (cells[position.x, position.y - 1] == null)
                        {
                            if (Random.Range(0f, 1) <= _mazeSettings.doorDirectionChance[3] * _mazeSettings.doorChanceFallOff[doorCount])
                            {
                                doorCount++;
                                cells[position.x, position.y].OpenWall(Side.Left);
                            }
                        }
                    }
                    break;
            }
        }
    }

    #endregion

    #region Node Generation

    private void CreateNodes()
    {
        currentEmptyNode = 0;
        pathfindingNodes = new PathfindingNode[maxNodeCount];

        // First Cell
        _spawnPoint = new Vector3(startPoint.x + (xDistance[firstCell.y] + 0.5f) * _mazeSettings.distanceBetweenCells, startPoint.y + 0.3f, startPoint.z + (zDistance[firstCell.x] + 0.5f) * _mazeSettings.distanceBetweenCells); // TODO only points to the right cell; rework hardcoded?; TODO spawns somwhere else // RLpos
        CreateRoom(firstCell);
        PushNeighbouringCells(firstCell);
        cells[firstCell.x, firstCell.y].generated = true;

        // Other Cells, Z, X
        Vector2Int currentCell;
        while (cellStack.Count > 0)
        {
            currentCell = cellStack.Pop();
            if (cells[currentCell.x, currentCell.y].generated == true)
            {
                continue;
            }

            if (Random.Range(0f, 1f) <= _mazeSettings.roomChance[cells[currentCell.x, currentCell.y].GetDoorCount() - 1])
            {
                CreateRoom(currentCell);
                //Debug.Log("Room");
            }
            else
            {
                CreateCorridor(currentCell);
                //Debug.Log("Corridor");
            }

            PushNeighbouringCells(currentCell);
            cells[currentCell.x, currentCell.y].generated = true;
        }
    }

    private void CreateRoom(Vector2Int position)
    {
        Vector2Int dimensions = GetDimensions(position);
        // X, Z
        Vector2 realPosition = new Vector2(startPoint.x + xDistance[position.y] * _mazeSettings.distanceBetweenCells, startPoint.z + zDistance[position.x] * _mazeSettings.distanceBetweenCells); // RLpos
        cells[position.x, position.y].lowestPathfindingNodeID = currentEmptyNode;

        // From bottom left corner 
        for (int i = 0; i < dimensions.x; i++) // Z
        {
            for (int j = 0; j < dimensions.y; j++) // X
            {
                pathfindingNodes[currentEmptyNode] = new PathfindingNode(currentEmptyNode, realPosition.x + _mazeSettings.distanceBetweenCells * j, realPosition.y + _mazeSettings.distanceBetweenCells * i); // RLpos

                // 2nd column + (Y)
                if (j > 0)
                {
                    ConnectTwoNodes(pathfindingNodes[currentEmptyNode], pathfindingNodes[currentEmptyNode - 1], Side.Left);
                }

                // 2nd row + (X)
                if (i > 0)
                {
                    ConnectTwoNodes(pathfindingNodes[currentEmptyNode], pathfindingNodes[currentEmptyNode - dimensions.y], Side.Bottom);

                    // Diagonal ones
                    if (j > 0)
                    {
                        ConnectTwoNodes(pathfindingNodes[currentEmptyNode], pathfindingNodes[currentEmptyNode - dimensions.y - 1], Side.BottomLeft);
                    }

                    if (j < dimensions.y - 1)
                    {
                        ConnectTwoNodes(pathfindingNodes[currentEmptyNode], pathfindingNodes[currentEmptyNode - dimensions.y + 1], Side.BottomRight);
                    }
                }

                currentEmptyNode++;

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
        // X, Z
        Vector2 realPosition = new Vector2(startPoint.x + xDistance[position.y] * _mazeSettings.distanceBetweenCells, startPoint.z + zDistance[position.x] * _mazeSettings.distanceBetweenCells); // RLpos
        Vector2Int dimensions = GetDimensions(position);
        Vector2Int pathCoords = GetPath(position, dimensions);
        Vector2 centerPosition = new Vector2(realPosition.x + _mazeSettings.distanceBetweenCells * pathCoords.y, realPosition.y + _mazeSettings.distanceBetweenCells * pathCoords.x); // RLpos
        // Z, X
        int centerID = currentEmptyNode + (dimensions.y * pathCoords.x) + pathCoords.y;
        cells[position.x, position.y].lowestPathfindingNodeID = currentEmptyNode;

        PathfindingNode centerNode = new PathfindingNode(centerID, centerPosition.x, centerPosition.y);
        pathfindingNodes[centerID] = centerNode;

        // Top
        if (cells[position.x, position.y].IsDoor(Side.Top))
        {
            if (pathCoords.x < (dimensions.x - 1))
            {
                int id = centerID + dimensions.y;
                pathfindingNodes[id] = new PathfindingNode(centerID + dimensions.y, centerPosition.x, centerPosition.y + _mazeSettings.distanceBetweenCells); // RLpos
                ConnectTwoNodes(pathfindingNodes[id], centerNode, Side.Bottom);
            }
        }
        // Right
        if (cells[position.x, position.y].IsDoor(Side.Right))
        {
            if (pathCoords.y < (dimensions.y - 1))
            {
                int id = centerID + 1;
                pathfindingNodes[id] = new PathfindingNode(centerID + 1, centerPosition.x + _mazeSettings.distanceBetweenCells, centerPosition.y); // RLpos
                ConnectTwoNodes(pathfindingNodes[id], centerNode, Side.Left);
            }
        }
        // Bottom
        if (cells[position.x, position.y].IsDoor(Side.Bottom))
        {
            if (pathCoords.x > 0)
            {
                int id = centerID - dimensions.y;
                pathfindingNodes[id] = new PathfindingNode(centerID - dimensions.y, centerPosition.x, centerPosition.y - _mazeSettings.distanceBetweenCells); // RLpos
                ConnectTwoNodes(pathfindingNodes[id], centerNode, Side.Top);
            }
        }
        // Left
        if (cells[position.x, position.y].IsDoor(Side.Left))
        {
            if (pathCoords.y > 0)
            {
                int id = centerID - 1;
                pathfindingNodes[id] = new PathfindingNode(centerID - 1, centerPosition.x - _mazeSettings.distanceBetweenCells, centerPosition.y); // RLpos
                ConnectTwoNodes(pathfindingNodes[id], centerNode, Side.Right);
            }
        }

        currentEmptyNode += dimensions.x * dimensions.y;

        // Connect nodes with surrounding Cells
        ConnectNodesFromDifferentCells(position, dimensions);

        // TODO walls (spawn different tiles), spawn items, spawn enemies; prob do later
    }

    private Vector2Int GetDimensions(Vector2Int position)
    {
        return new Vector2Int(zDistance[position.x + 1] - zDistance[position.x], xDistance[position.y + 1] - xDistance[position.y]);
    }

    private Vector2Int GetDimensions(int positionZ, int positionX)
    {
        return new Vector2Int(zDistance[positionZ + 1] - zDistance[positionZ], xDistance[positionX + 1] - xDistance[positionX]);
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
        Cell cell = cells[position.x, position.y];
        Vector2Int paths = GetPath(position, firstDimensions);
        Vector2Int secondDimension;
        int firstBaseID = cell.lowestPathfindingNodeID;
        int secondBaseID;

        //Debug.Log("pos " + position.x + ", " + position.y);
        //Debug.Log(cell.ToString2());
        // Top
        if (cell.IsDoor(Side.Top))
        {
            if (cells[position.x + 1, position.y].generated)
            {
                secondBaseID = cells[position.x + 1, position.y].lowestPathfindingNodeID;
                ConnectTwoNodes(pathfindingNodes[firstBaseID + (firstDimensions.y * (firstDimensions.x - 1)) + paths.y], pathfindingNodes[secondBaseID + paths.y], Side.Top);
            }
        }
        // Right
        if (cell.IsDoor(Side.Right))
        {
            if (cells[position.x, position.y + 1].generated)
            {
                secondBaseID = cells[position.x, position.y + 1].lowestPathfindingNodeID;
                secondDimension = GetDimensions(position.x, position.y + 1);
                ConnectTwoNodes(pathfindingNodes[firstBaseID + (firstDimensions.y * paths.x) + firstDimensions.y - 1], pathfindingNodes[secondBaseID + (secondDimension.y * paths.x)], Side.Right);
            }
        }
        // Bottom
        if (cell.IsDoor(Side.Bottom))
        {
            if (cells[position.x - 1, position.y].generated)
            {
                secondBaseID = cells[position.x - 1, position.y].lowestPathfindingNodeID;
                secondDimension = GetDimensions(position.x - 1, position.y);
                ConnectTwoNodes(pathfindingNodes[firstBaseID + paths.y], pathfindingNodes[secondBaseID + (secondDimension.y * (secondDimension.x - 1)) + paths.y], Side.Bottom);
            }
        }
        // Left
        if (cell.IsDoor(Side.Left))
        {
            if (cells[position.x, position.y - 1].generated)
            {
                secondBaseID = cells[position.x, position.y - 1].lowestPathfindingNodeID;
                secondDimension = GetDimensions(position.x, position.y - 1);
                ConnectTwoNodes(pathfindingNodes[firstBaseID + (firstDimensions.y * paths.x)], pathfindingNodes[secondBaseID + (secondDimension.y * paths.x) + secondDimension.y - 1], Side.Left);
            }
        }
    }

    #endregion

    #region Tile Spawning

    private void SpawnTiles()
    {
        _parent = new GameObject("Tiles").transform;
        Vector2 dimensions;

        for (int i = 0; i < _mazeSettings.length; i++)
        {
            for (int j = 0; j < _mazeSettings.width; j++)
            {
                if (cells[i, j] != null)
                {

                    dimensions = GetDimensions(i, j);
                    for (int k = 0; k < dimensions.x * dimensions.y; k++)
                    {
                        SpawnTilePrefab(pathfindingNodes[cells[i, j].lowestPathfindingNodeID + k]);
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
                    Instantiate(tiles.tiles[0], new Vector3(node.position.x, _parent.position.y, node.position.y), Quaternion.Euler(new Vector3(0, firstDoor * 90, 0)), _parent);
                    break;
                case 2:
                    if (node.neighbours[((firstDoor + 2) * 2) % 8] == null)
                    {
                        Instantiate(tiles.tiles[1], new Vector3(node.position.x, _parent.position.y, node.position.y), Quaternion.Euler(new Vector3(0, firstDoor * 90, 0)), _parent);
                    }
                    else
                    {
                        Instantiate(tiles.tiles[2], new Vector3(node.position.x, _parent.position.y, node.position.y), Quaternion.Euler(new Vector3(0, firstDoor * 90, 0)), _parent);
                    }
                    break;
                case 3:
                    Instantiate(tiles.tiles[3], new Vector3(node.position.x, _parent.position.y, node.position.y), Quaternion.Euler(new Vector3(0, (firstDoor - 1) * 90, 0)), _parent);
                    break;
                case 4:
                    Instantiate(tiles.tiles[4], new Vector3(node.position.x, _parent.position.y, node.position.y), Quaternion.identity, _parent);
                    break;
                default:
                    throw new System.Exception("Cell can't have more than four doors");
            }
        }
    }

    #endregion
}
