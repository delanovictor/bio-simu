using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heap<T> where T : IHeapItem<T>
{
    T[] items;

    int currentItemCount;

    public Heap(int maxHeapSize)
    {
        items = new T[maxHeapSize];
    }

    public void Add(T item)
    {
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;

        SortUp(item);
        currentItemCount++;
    }

    public T RemoveFirst()
    {
        T firstItem = items[0];

        currentItemCount--;

        items[0] = items[currentItemCount];

        items[0].HeapIndex = 0;

        SortDown(items[0]);

        return firstItem;
    }

    void SortDown(T item)
    {
        while (true)
        {
            int leftChildIndex = (item.HeapIndex * 2) + 1;
            int rightChildIndex = (item.HeapIndex * 2) + 2;
            int swapIndex = 0;

            if (leftChildIndex < currentItemCount)
            {
                swapIndex = leftChildIndex;

                if (rightChildIndex < currentItemCount)
                {
                    if (items[leftChildIndex].CompareTo(items[rightChildIndex]) < 0)
                    {
                        swapIndex = rightChildIndex;
                    }
                }

                if (item.CompareTo(items[swapIndex]) < 0)
                {
                    Swap(item, items[swapIndex]);
                }
                else
                {
                    break;
                }

            }
            else
            {
                break;
            }
        }
    }

    public bool Contains(T item)
    {
        return Equals(items[item.HeapIndex], item);
    }

    public void UpdateItem(T item)
    {
        SortUp(item);
    }

    public int Count
    {
        get
        {
            return currentItemCount;
        }
    }

    void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;

        while (true)
        {
            T parentItem = items[parentIndex];

            if (item.CompareTo(parentItem) > 0)
            {
                Swap(item, parentItem);
            }
            else
            {
                break;
            }


            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    void Swap(T itemA, T itemB)
    {
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;

        int tempHeapIndex = itemA.HeapIndex;

        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = tempHeapIndex;
    }

}


public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex
    {
        get;
        set;
    }
}