using System;
using System.Collections.Generic;

 namespace MelonECS
{
    internal static class ComponentIndex
    {
        internal static int lastIndex;
        internal static Dictionary<Type, int> typeIndex;

        static ComponentIndex() => typeIndex = new Dictionary<Type, int>();
    }
    
    internal static class ComponentType<T> where T : struct, IComponent
    {
        internal static readonly int Index;

        static ComponentType()
        {
            Index = ComponentIndex.typeIndex.TryGetValue(typeof(T), out int index) ? index : ++ComponentIndex.lastIndex;
            ComponentIndex.typeIndex[typeof(T)] = Index;
        }
    }

    internal static class ComponentType
    {
        public static int Index(Type type)
        {
            if (ComponentIndex.typeIndex.TryGetValue(type, out int index)) 
                return index;
            
            index = ++ComponentIndex.lastIndex;
            ComponentIndex.typeIndex.Add(type, index);
            return index;
        }

        public static Type Type(int index)
        {
            foreach (var data in ComponentIndex.typeIndex)
            {
                if (data.Value == index)
                    return data.Key;
            }
            return null;
        }
    }
}