using System;
using System.Collections.Generic;

namespace MelonECS
{
    public class Query
    {
        private readonly HashSet<int> includeSet = new HashSet<int>();
        private readonly HashSet<int> excludeSet = new HashSet<int>();

        private Entity[] entities;
        private readonly HashSet<Entity> entitiesSet = new HashSet<Entity>();

        internal Query(IEnumerable<Type> include, IEnumerable<Type> exclude)
        {
            entities = new Entity[4];

            foreach (var type in include)
                includeSet.Add(ComponentType.Index(type));  
            foreach (var type in exclude)
                excludeSet.Add(ComponentType.Index(type));
        }

        internal void AddEntity(Entity entity)
        {
            if (entitiesSet.Contains(entity))
                return;
            
            ArrayUtil.EnsureLength(ref entities, entitiesSet.Count + 1);
            entities[entitiesSet.Count] = entity;
            entitiesSet.Add(entity);
        }

        internal void RemoveEntity(Entity entity)
        {
            if (!entitiesSet.Contains(entity))
                return;
            
            int index = Array.IndexOf(entities, entity);
            entities[index] = entities[entitiesSet.Count - 1];
            entitiesSet.Remove(entity);
        }

        internal bool IsMatch(HashSet<int> components) 
            => includeSet.IsSubsetOf(components) && (excludeSet.Count == 0 || !excludeSet.IsSubsetOf(components));

        internal bool IsMatch(int component) 
            => includeSet.Contains(component) && !excludeSet.Contains(component);

        internal bool IsMatch(Query query)
            => includeSet.SetEquals(query.includeSet) && excludeSet.SetEquals(query.excludeSet);

        public Span<Entity> GetEntities() => new Span<Entity>(entities, 0, entitiesSet.Count);
    }
}