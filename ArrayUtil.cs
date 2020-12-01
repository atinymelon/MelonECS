using System;

namespace MelonECS
{
    internal static class ArrayUtil
    {
        internal static void EnsureLength<T>(ref T[] array, int length)
        {
            if (length <= array.Length) 
                return;
            
            var temp = new T[array.Length * 2];
            Array.Copy(array, temp, array.Length);
            array = temp;
        }
    }
}