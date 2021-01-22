using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingHeap<T> where T : IPathfindingNode<T>
{
    // První pole v poli bude prázdné
    private T[] _array;
    private int _count;


    // Konstruktor
    public PathfindingHeap(int size)
    {
        _count = 0;
        _array = new T[size + 2]; // Why?
    }

    // Vloží do haldy prvek na poslední index a obnoví haldu
    public void Insert(T node)
    {
        _count++;
        _array[_count] = node;

        MoveDown(_count);
    }

    // Odstraní první prvek a poté obnoví haldu
    public T RemoveFirst()
    {
        T first = _array[1];
        _array[1] = _array[_count];
        _count--;

        MoveUp(1);

        return first;
    }

    // Posouvá prvek haldou dolu, dokud se nestane prvním prvkem nebo jeho hodnota mu to nedovoluje
    public void MoveDown(int index)
    {
        T node = _array[index];

        while (index > 1 && _array[index].CompareTo(_array[index / 2]) < 0)
        {
            _array[index] = _array[index / 2];
            _array[index].IndexInHeap = index;
            _array[index / 2] = node;
            index /= 2;
        }

        node.IndexInHeap = index;
    }

    // Posouvá prvek haldou nahoru, dokud se nestane na okraj nebo jeho hodnota mu to nedovoluje
    public void MoveUp(int index)
    {
        T node = _array[index];

        while (true)
        {
            // Prvek nemá potomka
            if (index * 2 > _count)
            {
                break;
            }
            else if (index * 2 + 1 > _count) // Pouze první potomek je nenulový
            {
                // Je prvek větší než hodnota jeho prvního potomka
                if (node.CompareTo(_array[index * 2]) > 0)
                {
                    _array[index] = _array[index * 2];
                    _array[index].IndexInHeap = index;
                    _array[index * 2] = node;
                    node.IndexInHeap = index * 2;
                    index *= 2;
                }
                else
                {
                    break;
                }
            }
            else // Prvek má oba potomky
            {
                // První potomek má větší hodnotu
                if (_array[index * 2].CompareTo(_array[index * 2 + 1]) > 0)
                {
                    // Je prvek větší než hodnota jeho druhého potomka (s menší hodnotou)
                    if (node.CompareTo(_array[index * 2 + 1]) > 0)
                    {
                        _array[index] = _array[index * 2 + 1];
                        _array[index].IndexInHeap = index;
                        _array[index * 2 + 1] = node;
                        node.IndexInHeap = index * 2 + 1;
                        index = index * 2 + 1;
                    }
                    else if (node.CompareTo(_array[index * 2]) > 0) // Je prvek větší než hodnota jeho prvního potomka (s větší hodnotou)
                    {
                        _array[index] = _array[index * 2];
                        _array[index].IndexInHeap = index;
                        _array[index * 2] = node;
                        node.IndexInHeap = index * 2;
                        index *= 2;
                    } 
                    else
                    {
                        break;
                    }
                } else // Druhý potomek má větší hodnotu
                {
                    // Je prvek větší než hodnota jeho prvního potomka (s menší hodnotou)
                    if (node.CompareTo(_array[index * 2]) > 0)
                    {
                        _array[index] = _array[index * 2];
                        _array[index].IndexInHeap = index;
                        _array[index * 2] = node;
                        node.IndexInHeap = index * 2;
                        index *= 2;
                    }
                    else if (node.CompareTo(_array[index * 2 + 1]) > 0) // Je prvek větší než hodnota jeho druhý potomka (s větší hodnotou)
                    {
                        _array[index] = _array[index * 2 + 1];
                        _array[index].IndexInHeap = index;
                        _array[index * 2 + 1] = node;
                        node.IndexInHeap = index * 2 + 1;
                        index = index * 2 + 1;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }

    // Nastaví počet prvků na 0
    public void Reset()
    {
        _count = 0;
    }

    // Vrátí počet prvků v haldě
    public int GetCount()
    {
        return _count;
    }
}
