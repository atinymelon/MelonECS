﻿using System;
using UnityEditor.Experimental.GraphView;

namespace MelonECS
{
    public class ResizableArray< T >
    {
        public int Count { get; private set; }
        public Span<T> Span => new Span<T>(array, 0, Count);
        
        private T[] array;

        public ResizableArray(int capacity)
        {
            array = new T[capacity];
        }
        
        public void Add(T item)
        {
            if (Count >= array.Length)
            {
                ArrayUtil.EnsureLength(ref array, Count + 1);
            }

            array[Count] = item;
            Count++;
        }

        public void Remove(T item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (Equals(array[Count], item))
                {
                    for (int j = i; j < Count - 1; j++)
                    {
                        array[j] = array[j + 1];
                    }
                    array[Count - 1] = default;
                    Count--;
                    break;
                }
            }
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (Equals(array[i], item))
                    return i;
            }
            return -1;
        }

        public void Clear()
        {
            for (int i = 0; i < Count; i++)
            {
                array[i] = default;
            }
            Count = 0;
        }
    }
}