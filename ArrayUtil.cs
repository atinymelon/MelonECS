using System;
using UnityEngine;

namespace MelonECS
{
    public static class ArrayUtil
    {
        public static void EnsureLength<T>(ref T[] array, int length)
        {
            if (length <= array.Length) 
                return;
            
            var temp = new T[Mathf.Max(array.Length * 2, length)];
            Array.Copy(array, temp, array.Length);
            array = temp;
        }
    }
}