using System;
using System.Collections.Generic;

namespace MelonECS
{
    internal class EntityComponentMap
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
        
        public Span<HashSet<int>> Read() => new Span<HashSet<int>>(components);
    }
}