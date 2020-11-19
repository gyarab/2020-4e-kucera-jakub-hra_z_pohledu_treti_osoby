using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pathfinding<T> where T : IPathfindingNode<T>
{
    private T[] _nodes;
    private int _nodeCount;
    private PathfindingHeap<T> _openSet;

    public Pathfinding(T[] nodes, int count)
    {
        _nodes = nodes;
        _nodeCount = count;
        _openSet = new PathfindingHeap<T>(_nodeCount); // TODO prob optimize 
    }

    public void GetPath(Vector3 position, Vector3 targetPosition, Action<List<Vector3>> action)
    {
        action(FindPath(GetCurrentNodeID(position), GetCurrentNodeID(targetPosition)));
    }

    private List<Vector3> FindPath(int startID, int endID)
    {
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

        _openSet.Insert(_nodes[startID]);

        while(_openSet.GetCount() > 0)
        {
            node = _openSet.RemoveFirst();

            // After reaching goal return path
            if (node.ID == endID)
            {
                return ReconstructPath(startID, endID);
            }

            node.Status = Status.CLOSEDSET;
            
            // Check every neighbour
            for (int i = 0; i < 8; i++)
            {
                if(node.Neighbours[i] != null)
                {
                    if (node.Neighbours[i].Status == Status.CLOSEDSET)
                    {
                        continue; // if it doesnt work put return here and instead of return return null;
                    }

                    currentGCost = node.GCost + Vector2.Distance(new Vector2(node.Position.x, node.Position.z), new Vector2(node.Neighbours[i].Position.x, node.Neighbours[i].Position.z));

                    if (node.Neighbours[i].Status == Status.NOWHERE)
                    {
                        node.Neighbours[i].GCost = currentGCost;
                        node.Neighbours[i].HCost = Vector2.Distance(new Vector2(node.Neighbours[i].Position.x, node.Neighbours[i].Position.z), new Vector2(_nodes[endID].Position.x, _nodes[endID].Position.z));
                        node.Neighbours[i].CameFromID = node.ID;
                        node.Neighbours[i].Status = Status.OPENSET;
                        _openSet.Insert(node.Neighbours[i]);
                    } else if (currentGCost < node.Neighbours[i].GCost)
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

    private List<Vector3> ReconstructPath(int startID, int endID)
    {
        List<Vector3> result = new List<Vector3>();
        T currentNode = _nodes[endID];

        return AddStop(result, currentNode, startID);
    }

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

    private float GetApproximateDistance(Vector3 objectPosition, Vector3 nodePosition)
    {
        return Mathf.Pow(nodePosition.x - objectPosition.x, 2) + Mathf.Pow(nodePosition.z - objectPosition.z, 2);
    }
}
