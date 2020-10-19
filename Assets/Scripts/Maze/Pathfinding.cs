using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pathfinding : MonoBehaviour
{
    private PathfindingNode[] _nodes;
    private int _nodeCount;
    private PathfindingHeap _openSet;

    public void SetVariables(PathfindingNode[] array, int count)
    {
        _nodes = array;
        _nodeCount = count;

        _openSet = new PathfindingHeap(_nodeCount); // TODO prob optimize 
    }

    public void GetPath(Vector3 position, Vector3 targetPosition, Action<List<Vector3>> action)
    {
        action(FindPath(GetCurrentNodeID(position), GetCurrentNodeID(targetPosition)));
    }

    private List<Vector3> FindPath(int startID, int endID)
    {
        _openSet.Reset();
        PathfindingNode node;

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
            if (node.id == endID)
            {
                return ReconstructPath(startID, endID);
            }

            node.Status = Status.CLOSEDSET;
            
            // Check every neighbour
            for (int i = 0; i < 8; i++)
            {
                if(node.neighbours[i] != null)
                {
                    if (node.neighbours[i].Status == Status.CLOSEDSET)
                    {
                        continue; // if it doesnt work put return here and instead of return return null;
                    }

                    currentGCost = node.gCost + Vector2.Distance(node.position, node.neighbours[i].position);

                    if(node.neighbours[i].Status == Status.NOWHERE)
                    {
                        node.neighbours[i].gCost = currentGCost;
                        node.neighbours[i].hCost = Vector2.Distance(node.neighbours[i].position, _nodes[endID].position);
                        node.neighbours[i].cameFromID = node.id;
                        node.neighbours[i].Status = Status.OPENSET;
                        _openSet.Insert(node.neighbours[i]);
                    } else if (currentGCost < node.neighbours[i].gCost)
                    {
                        node.neighbours[i].gCost = currentGCost;
                        node.neighbours[i].cameFromID = node.id;
                        _openSet.MoveDown(node.neighbours[i].indexInHeap);
                    }
                }
            }
        }

        return null;
    }

    private List<Vector3> ReconstructPath(int startID, int endID)
    {
        List<Vector3> result = new List<Vector3>();
        PathfindingNode currentNode = _nodes[endID];

        return AddStop(result, currentNode, startID);
    }

    private List<Vector3> AddStop(List<Vector3> list, PathfindingNode currentNode, int startID)
    {
        if (currentNode.id == startID)
        {
            list.Add(new Vector3(currentNode.position.x, 0, currentNode.position.y));
            return list;
        }

        AddStop(list, _nodes[currentNode.cameFromID], startID);
        list.Add(new Vector3(currentNode.position.x, 0, currentNode.position.y));

        return list;
    }

    private int GetCurrentNodeID(Vector3 position)
    {
        float minDist = GetApproximateDistance(position, _nodes[0].position);
        float temp;
        int minDistID = 0;

        for (int i = 1; i < _nodeCount; i++)
        {
            if (_nodes[i] != null)
            {

                temp = GetApproximateDistance(position, _nodes[i].position);
                if (temp < minDist)
                {
                    minDist = temp;
                    minDistID = i;
                }
            }
        }

        return minDistID;
    }

    private float GetApproximateDistance(Vector3 objectPosition, Vector2 nodePosition)
    {
        return Mathf.Pow(nodePosition.x - objectPosition.x, 2) + Mathf.Pow(nodePosition.y - objectPosition.z, 2);
    }
}
