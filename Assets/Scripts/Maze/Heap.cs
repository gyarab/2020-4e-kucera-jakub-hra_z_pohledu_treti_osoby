using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingHeap
{
    private PathfindingNode[] _array;
    private int _count;


    // First field is going to be empty
    public PathfindingHeap(int size)
    {
        _count = 0;
        _array = new PathfindingNode[size + 2]; // IDK why
    }

    public void Insert(PathfindingNode node)
    {
        _count++;
        _array[_count] = node;

        MoveDown(_count);
    }

    public PathfindingNode RemoveFirst()
    {
        PathfindingNode first = _array[1];
        _array[1] = _array[_count];
        _count--;

        MoveUp(1);

        return first;
    }

    public void MoveDown(int index)
    {
        PathfindingNode node = _array[index];

        while (index > 1 && _array[index].CompareTo(_array[index / 2]) < 0)
        {
            _array[index] = _array[index / 2];
            _array[index].indexInHeap = index;
            _array[index / 2] = node;
            index /= 2;
        }

        node.indexInHeap = index;
    }

    public void MoveUp(int index)
    {
        PathfindingNode node = _array[index];

        while (true)
        {
            // Node doesn't have any children
            if (index * 2 > _count)
            {
                break;
            }
            else if (index * 2 + 1 > _count) // Only first child is not null
            {
                // Is node greater than it's first child
                if (node.CompareTo(_array[index * 2]) > 0)
                {
                    _array[index] = _array[index * 2];
                    _array[index].indexInHeap = index;
                    _array[index * 2] = node;
                    node.indexInHeap = index * 2;
                    index *= 2;
                }
                else
                {
                    break;
                }
            }
            else //if (index * 2 + 1 <= _count) // Both children are not null
            {
                // First child has higher value
                if (_array[index * 2].CompareTo(_array[index * 2 + 1]) > 0)
                {
                    // Is node greater than second child (with lesser value)
                    if (node.CompareTo(_array[index * 2 + 1]) > 0)
                    {
                        _array[index] = _array[index * 2 + 1];
                        _array[index].indexInHeap = index;
                        _array[index * 2 + 1] = node;
                        node.indexInHeap = index * 2 + 1;
                        index = index * 2 + 1;
                    }
                    else if (node.CompareTo(_array[index * 2]) > 0) // Is node greater than first child (with greater value)
                    {
                        _array[index] = _array[index * 2];
                        _array[index].indexInHeap = index;
                        _array[index * 2] = node;
                        node.indexInHeap = index * 2;
                        index *= 2;
                    } 
                    else
                    {
                        break;
                    }
                } else // Second child has higher value
                {
                    // Is node greater than first child (with lesser value)
                    if (node.CompareTo(_array[index * 2]) > 0)
                    {
                        _array[index] = _array[index * 2];
                        _array[index].indexInHeap = index;
                        _array[index * 2] = node;
                        node.indexInHeap = index * 2;
                        index *= 2;
                    }
                    else if (node.CompareTo(_array[index * 2 + 1]) > 0) // Is node greater than second child (with greater value)
                    {
                        _array[index] = _array[index * 2 + 1];
                        _array[index].indexInHeap = index;
                        _array[index * 2 + 1] = node;
                        node.indexInHeap = index * 2 + 1;
                        index = index * 2 + 1;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // Both children are not null;;; TODO remove
            /*if (index * 2 + 1 < itemCount)
            {
                // First child has higher value
                if(array[index * 2].CompareTo(array[index * 2 + 1]) > 0)
                {
                    if(node.CompareTo(array[index * 2]) > 0)
                    {
                        array[index] = array[index * 2];
                        array[index].indexInHeap = index;
                        array[index * 2] = node;
                        node.indexInHeap = index * 2;
                        index *= 2;
                    } else if (node.CompareTo(array[index * 2 + 1]) > 0)
                    {
                        array[index] = array[index * 2 + 1];
                        array[index].indexInHeap = index;
                        array[index * 2 + 1] = node;
                        node.indexInHeap = index * 2 + 1;
                        index = index * 2 + 1;
                    } else
                    {
                        break;
                    }
                } else // Second child has higher value
                {
                    if (node.CompareTo(array[index * 2 + 1]) > 0)
                    {
                        array[index] = array[index * 2 + 1];
                        array[index].indexInHeap = index;
                        array[index * 2 + 1] = node;
                        node.indexInHeap = index * 2 + 1;
                        index = index * 2 + 1;
                    } else if (node.CompareTo(array[index * 2]) > 0)
                    {
                        array[index] = array[index * 2];
                        array[index].indexInHeap = index;
                        array[index * 2] = node;
                        node.indexInHeap = index * 2;
                        index *= 2;
                    } else
                    {
                        break;
                    }
                }
            } else if (index * 2 < itemCount) // Only first child is not null
            {
                if(node.CompareTo(array[index * 2]) > 0)
                {
                    array[index] = array[index * 2];
                    array[index].indexInHeap = index;
                    array[index * 2] = node;
                    node.indexInHeap = index * 2;
                    index *= 2;
                } else
                {
                    break;
                }
            } else
            {
                // Both children are null
                break;
            }*/
        }
    }

    public void Reset()
    {
        _count = 0;
    }

    public int GetCount()
    {
        return _count;
    }
}
