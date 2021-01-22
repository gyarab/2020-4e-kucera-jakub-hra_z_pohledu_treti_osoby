using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pathfinding<T> where T : IPathfindingNode<T>
{
    private T[] _nodes;
    private int _nodeCount;
    private PathfindingHeap<T> _openSet;

    // Vytvoří objekt pathfinding, který obdrží pole s uzly a jejich počet
    public Pathfinding(T[] nodes, int count)
    {
        _nodes = nodes;
        _nodeCount = count;
        _openSet = new PathfindingHeap<T>(_nodeCount);
    }

    // Vrátí cestu z pozice A do pozice B
    public List<Vector3> GetPath(Vector3 position, Vector3 targetPosition)
    {
        return FindPath(GetCurrentNodeID(position), GetCurrentNodeID(targetPosition));
    }

    // Najde optimální cestu z uzlu s daným ID do uzlu s dalším ID
    private List<Vector3> FindPath(int startID, int endID)
    {
        // Nastaví hodnoty uzlů na počáteční
        _openSet.Reset();
        T node;

        float currentGCost;
        for (int i = 0; i < _nodeCount; i++)
        {
            if(_nodes[i] != null)
            {
                _nodes[i].Status = Status.NOWHERE;
            }
        }

        // Vloží do haldy s otevřenými uzly
        _openSet.Insert(_nodes[startID]);

        // Dokud halda není prázdná
        while(_openSet.GetCount() > 0)
        {
            // Z otevřené množiny (haldy) je odebrán uzel s nejnižší hodnotou a zařazen do uzavřené množiny
            node = _openSet.RemoveFirst();

            // Po dosažení cíle zrekonsrtuuje cestu
            if (node.ID == endID)
            {
                return ReconstructPath(startID, endID);
            }

            node.Status = Status.CLOSEDSET;
            
            // Projde všechny sousední uzly
            for (int i = 0; i < 8; i++)
            {
                if(node.Neighbours[i] != null)
                {
                    // Když je soused v uzavřeném uzlu, nic se nemění
                    if (node.Neighbours[i].Status == Status.CLOSEDSET)
                    {
                        continue;
                    }

                    currentGCost = node.GCost + Vector2.Distance(new Vector2(node.Position.x, node.Position.z), new Vector2(node.Neighbours[i].Position.x, node.Neighbours[i].Position.z));

                    // Když uzel není zařazen, tak je vložen do otevřené množiny
                    if (node.Neighbours[i].Status == Status.NOWHERE)
                    {
                        node.Neighbours[i].GCost = currentGCost;
                        node.Neighbours[i].HCost = Vector2.Distance(new Vector2(node.Neighbours[i].Position.x, node.Neighbours[i].Position.z), new Vector2(_nodes[endID].Position.x, _nodes[endID].Position.z));
                        node.Neighbours[i].CameFromID = node.ID;
                        node.Neighbours[i].Status = Status.OPENSET;
                        _openSet.Insert(node.Neighbours[i]);
                    } else if (currentGCost < node.Neighbours[i].GCost) // Jinak je porovnána nová hodnota uzlu s jeho předchozí hodnotou a případně změněna
                    {
                        node.Neighbours[i].GCost = currentGCost;
                        node.Neighbours[i].CameFromID = node.ID;
                        _openSet.MoveDown(node.Neighbours[i].IndexInHeap);
                    }
                }
            }
        }

        return null;
    }

    // Vrátí cestu z cíle do počátečního uzlu
    private List<Vector3> ReconstructPath(int startID, int endID)
    {
        List<Vector3> result = new List<Vector3>();
        T currentNode = _nodes[endID];

        return AddStop(result, currentNode, startID);
    }

    // Rekurzivní funkcem, která se zavolá a přidá do seznamu uzel z parametru, dokud nenarazí na počáteční uzel
    private List<Vector3> AddStop(List<Vector3> list, T currentNode, int startID)
    {
        if (currentNode.ID == startID)
        {
            list.Add(new Vector3(currentNode.Position.x, 0, currentNode.Position.z));
            return list;
        }

        AddStop(list, _nodes[currentNode.CameFromID], startID);
        list.Add(new Vector3(currentNode.Position.x, 0, currentNode.Position.z));

        return list;
    }

    // Najde uzel nejblíže k dané pozici
    private int GetCurrentNodeID(Vector3 position)
    {
        float minDist = GetApproximateDistance(position, _nodes[0].Position);
        float temp;
        int minDistID = 0;

        for (int i = 1; i < _nodeCount; i++)
        {
            if (_nodes[i] != null)
            {

                temp = GetApproximateDistance(position, _nodes[i].Position);
                if (temp < minDist)
                {
                    minDist = temp;
                    minDistID = i;
                }
            }
        }

        return minDistID;
    }

    // Sppočítá druhou mocninu vzdáleností
    private float GetApproximateDistance(Vector3 objectPosition, Vector3 nodePosition)
    {
        return Mathf.Pow(nodePosition.x - objectPosition.x, 2) + Mathf.Pow(nodePosition.z - objectPosition.z, 2);
    }

    // Vrátí částečně náhodný seznam sousedícíh vrcholů
    public List<Vector3> GetRandomPath(Vector3 position, int cycles, int cycleLength)
    {
        int direction = UnityEngine.Random.Range(0, 8);
        T currentNode = _nodes[GetCurrentNodeID(position)];
        List<Vector3> path = new List<Vector3>(); 

        for (int i = 0; i < cycles; i++)
        {
            for (int j = 0; j < cycleLength; j++)
            {
                if(currentNode.Neighbours[direction] != null)
                {
                    currentNode = currentNode.Neighbours[direction];
                } else
                {
                    break;
                }
            }

            path.Add(currentNode.Position);
            direction += (UnityEngine.Random.Range(0, 5) - 2 + 8);
            direction = direction % 8;
        }

        return path;
    }
}
