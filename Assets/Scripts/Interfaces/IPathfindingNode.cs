using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IPathfindingNode<T> : IComparable<T>
{
    Status Status { get; set; }
    float GCost { get; set; }
    float HCost { get; set; }
    float FCost { get; }
    int ID { get; set; }
    int CameFromID { get; set; }
    int IndexInHeap { get; set; }
    T[] Neighbours { get; set; }
    Vector3 Position { get; set; }
}
