using System;
using System.Collections.Generic;

namespace MelonECS
{
    public class EntityComponentMap
    {
        private HashSet<int>[] components;

        public EntityComponentMap(int capacity)
        {
            components = new HashSet<int>[capacity];
        }
        
        public void Add(Entity entity, int componentType)
        {
            ArrayUtil.EnsureLength(ref components, entity.Index + 1);
            
            if (components[entity.Index] == null)
                components[entity.Index] = new HashSet<int>();
            components[entity.Index].Add(componentType);
        }
        
        public void Remove(Entity entity, int componentType)
        {
            components[entity.Index].Remove(componentType);
        }

        public void RemoveAll(Entity entity)
        {
            components[entity.Index].Clear();
        }

        public HashSet<int> Get(Entity entity) => components[entity.Index];
        
        public ArrayRef<HashSet<int>> Read() => new ArrayRef<HashSet<int>>(components, 0, components.Length);
    }
}