using System;
using System.Collections.Generic;

namespace MelonECS
{
    public class Query
    {
        private readonly HashSet<int> withSet = new HashSet<int>();
        private readonly HashSet<int> excludeSet = new HashSet<int>();

        private Entity[] entities;
        private readonly HashSet<Entity> entitiesSet = new HashSet<Entity>();

        internal Query(World world)
        {
            entities = new Entity[16];
            world.RegisterQuery(this);
        }

        public void AddEntity(Entity entity)
        {
            if (entitiesSet.Contains(entity))
                return;
            
            ArrayUtil.EnsureLength(ref entities, entitiesSet.Count + 1);
            entities[entitiesSet.Count] = entity;
            entitiesSet.Add(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            if (!entitiesSet.Contains(entity))
                return;
            
            int index = Array.IndexOf(entities, entity);
            entities[index] = entities[entitiesSet.Count - 1];
            entitiesSet.Remove(entity);
        }

        public Query With<T>() where T : struct, IComponent => With(typeof(T));
        public Query With(Type type)
        {
            withSet.Add(ComponentType.Index(type));
            return this;
        }

        public Query Exclude<T>() where T : struct, IComponent => Exclude(typeof(T));
        public Query Exclude(Type type)
        {
            excludeSet.Add(ComponentType.Index(type));
            return this;
        }

        public bool IsMatch(HashSet<int> components) 
            => withSet.IsSubsetOf(components) && (excludeSet.Count == 0 || !excludeSet.IsSubsetOf(components));

        public bool IsMatch(int component) 
            => withSet.Contains(component) && !excludeSet.Contains(component);

        public Span<Entity> GetEntities() => new Span<Entity>(entities, 0, entitiesSet.Count);
    }
}