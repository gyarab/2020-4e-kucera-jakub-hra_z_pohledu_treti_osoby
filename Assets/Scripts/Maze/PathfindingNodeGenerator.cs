using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingNodeGenerator : MonoBehaviour
{
    private PathfindingNode[] _pathfindingNodes;
    private int _currentEmptyIndex;

    // Vytvoří uzly k vyhledávání cesty podle podbuňek, které dostane v objektu Subcell Data
    public PathfindingNode[] GenerateNodes(MazeSettingsSO mazeSettings, SubcellData subcellData)
    {
        _pathfindingNodes = new PathfindingNode[subcellData.EmptySpotInArray * mazeSettings.pathfindingNodesInSubcell * mazeSettings.pathfindingNodesInSubcell];
        _currentEmptyIndex = 0;
        float step = mazeSettings.distanceBetweenCells / mazeSettings.pathfindingNodesInSubcell * mazeSettings.nodeSpreadPercentage;
        Vector3 startPosition;
        Vector3 startOffset = new Vector3(((float)mazeSettings.distanceBetweenCells * mazeSettings.nodeSpreadPercentage - step) / 2f, 0,((float)mazeSettings.distanceBetweenCells * mazeSettings.nodeSpreadPercentage - step) / 2f);

        foreach (Subcell subcell in subcellData.Subcells)
        {
            if(subcell == null)
            {
                continue;
            }

            // Projde všechny buňky a vytvoří v nich uzly k vyhledávání cesty
            subcell.LowestPathfindingNodeID = _currentEmptyIndex;
            startPosition = subcell.Position - startOffset;
            CreatePathfindingNodeGrid(startPosition, mazeSettings.pathfindingNodesInSubcell, step);

            ConnectNodesInSubcellToNeighbours(subcell, mazeSettings.pathfindingNodesInSubcell);
            subcell.NodesCreated = true;
        }

        return _pathfindingNodes;
    }

    // Vytvoří čtvercovou síť vyhledávacích uzlů
    private void CreatePathfindingNodeGrid(Vector3 startPosition, int dimension, float step)
    {
        // Začne v levém spodním rohu
        for (int i = 0; i < dimension; i++) // Z
        {
            for (int j = 0; j < dimension; j++) // X
            {
                _pathfindingNodes[_currentEmptyIndex] = new PathfindingNode(_currentEmptyIndex, startPosition.x + step * j, startPosition.y, startPosition.z + step * i);

                // Ve druhém a každém dalším sloupci propojí uzel s uzlem v předchozím sloupci (s uzlem vlevo)
                if (j > 0)
                {
                    ConnectTwoNodes(_pathfindingNodes[_currentEmptyIndex], _pathfindingNodes[_currentEmptyIndex - 1], Side.Left);
                }

                // Ve druhé a každé další řadě propojí uzel s uzlem v předchozí řadě (s uzlem pod ním)
                if (i > 0)
                {
                    ConnectTwoNodes(_pathfindingNodes[_currentEmptyIndex], _pathfindingNodes[_currentEmptyIndex - dimension], Side.Bottom);

                    // V aspoň druhé řadě a v minimálně druhém sloupci propojí uzel s uzlem vlevo dole
                    if (j > 0)
                    {
                        ConnectTwoNodes(_pathfindingNodes[_currentEmptyIndex], _pathfindingNodes[_currentEmptyIndex - dimension - 1], Side.BottomLeft);
                    }

                    // V aspoň druhé řadě a v maximálně předposledním sloupci propojí uzel s uzlem vpravo dole
                    if (j < dimension - 1)
                    {
                        ConnectTwoNodes(_pathfindingNodes[_currentEmptyIndex], _pathfindingNodes[_currentEmptyIndex - dimension + 1], Side.BottomRight);
                    }
                }

                _currentEmptyIndex++;
            }
        }
    }

    // Propojí dva uzly podle daného směru propojení
    private void ConnectTwoNodes(PathfindingNode first, PathfindingNode second, Side direction)
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

    // Propojí uzly v podbuňce s uzly v sousedních buňkách
    private void ConnectNodesInSubcellToNeighbours(Subcell subcell, int dimension)
    {
        for (int i = 0; i < 8; i++)
        {
            //  Přeskakuje diagonální propojení buňek - odstranit, kdyby to bylo potřeba
            if (i % 2 == 1)
            {
                continue;
            }

            if (subcell.Neighbours[i] != null)
            {
                if (subcell.Neighbours[i].NodesCreated)
                {
                    ConnectNodesFromDifferentSubcells(subcell, subcell.Neighbours[i], i, dimension);
                }
            }
        }
    }

    // Propojí vyhledávací uzly z dvou různých podbuňek
    private void ConnectNodesFromDifferentSubcells(Subcell first, Subcell second, int direction, int dimension)
    {
        switch (direction)
        {
            case 0:
                {
                    int firstID = first.LowestPathfindingNodeID + (dimension) * (dimension - 1);
                    int secondID = second.LowestPathfindingNodeID;

                    for (int i = 0; i < dimension; i++)
                    {
                        ConnectTwoNodes(_pathfindingNodes[firstID], _pathfindingNodes[secondID], Side.Top);

                        if (i < dimension - 1)
                        {
                            ConnectTwoNodes(_pathfindingNodes[firstID], _pathfindingNodes[secondID + 1], Side.TopRight);
                        }

                        if(i > 0)
                        {
                            ConnectTwoNodes(_pathfindingNodes[firstID], _pathfindingNodes[secondID - 1], Side.TopLeft);
                        }

                        firstID++;
                        secondID++;
                    }
                    break;
                }
            case 1:
                ConnectTwoNodes(_pathfindingNodes[first.LowestPathfindingNodeID + (dimension * dimension) - 1], _pathfindingNodes[second.LowestPathfindingNodeID], Side.TopRight);
                break;
            case 2:
                {
                    int firstID = first.LowestPathfindingNodeID + dimension - 1;
                    int secondID = second.LowestPathfindingNodeID;

                    for (int i = 0; i < dimension; i++)
                    {
                        ConnectTwoNodes(_pathfindingNodes[firstID], _pathfindingNodes[secondID], Side.Right);

                        if (i < dimension - 1)
                        {
                            ConnectTwoNodes(_pathfindingNodes[firstID], _pathfindingNodes[secondID + dimension], Side.TopRight);
                        }

                        if (i > 0)
                        {
                            ConnectTwoNodes(_pathfindingNodes[firstID], _pathfindingNodes[secondID - dimension], Side.BottomRight);
                        }

                        firstID += dimension;
                        secondID += dimension;
                    }
                    break;
                }
            case 3:
                ConnectTwoNodes(_pathfindingNodes[first.LowestPathfindingNodeID + (dimension - 1)], _pathfindingNodes[second.LowestPathfindingNodeID + (dimension) * (dimension - 1)], Side.BottomRight);
                break;
            case 4:
                {
                    int firstID = first.LowestPathfindingNodeID;
                    int secondID = second.LowestPathfindingNodeID + (dimension) * (dimension - 1);

                    for (int i = 0; i < dimension; i++)
                    {
                        ConnectTwoNodes(_pathfindingNodes[firstID], _pathfindingNodes[secondID], Side.Bottom);

                        if (i < dimension - 1)
                        {
                            ConnectTwoNodes(_pathfindingNodes[firstID], _pathfindingNodes[secondID + 1], Side.BottomRight);
                        }

                        if (i > 0)
                        {
                            ConnectTwoNodes(_pathfindingNodes[firstID], _pathfindingNodes[secondID - 1], Side.BottomLeft);
                        }

                        firstID++;
                        secondID++;
                    }
                    break;
                }
            case 5:
                ConnectTwoNodes(_pathfindingNodes[first.LowestPathfindingNodeID], _pathfindingNodes[second.LowestPathfindingNodeID + (dimension * dimension) - 1], Side.BottomLeft);
                break;
            case 6:
                {
                    int firstID = first.LowestPathfindingNodeID;
                    int secondID = second.LowestPathfindingNodeID + dimension - 1;

                    for (int i = 0; i < dimension; i++)
                    {
                        ConnectTwoNodes(_pathfindingNodes[firstID], _pathfindingNodes[secondID], Side.Left);

                        if (i < dimension - 1)
                        {
                            ConnectTwoNodes(_pathfindingNodes[firstID], _pathfindingNodes[secondID + dimension], Side.TopLeft);
                        }

                        if (i > 0)
                        {
                            ConnectTwoNodes(_pathfindingNodes[firstID], _pathfindingNodes[secondID - dimension], Side.BottomLeft);
                        }

                        firstID += dimension;
                        secondID += dimension;
                    }
                    break;
                }
            case 7:
                ConnectTwoNodes(_pathfindingNodes[first.LowestPathfindingNodeID + (dimension) * (dimension - 1)], _pathfindingNodes[second.LowestPathfindingNodeID + (dimension - 1)], Side.TopLeft);
                break;
        }
    }
}
