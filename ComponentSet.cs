using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MelonECS
{
    internal interface IComponentSet
    {
        void Remove(Entity entity);
        bool TryRemove(Entity entity);
        void Update();
        IComponent GetGeneric(in Entity entity);
        
        int Count { get; }
    }
    
    internal class ComponentSet<TComponent> : IComponentSet where TComponent : struct, IComponent
    {
        private const int INVALID_INDEX = -1;

        public int Count { get; private set; }
        
        private int[] indices;
        private Entity[] entities;
        private TComponent[] components;

        private Entity[] changed;
        private int changedCount;

        // private readonly List<(Entity, TComponent)> addedComponents = new List<(Entity, TComponent)>();
        // private readonly List<Entity> removedComponents = new List<Entity>();

        private readonly ResizableArray<Entity> addedEntities = new ResizableArray<Entity>(4);
        private readonly ResizableArray<TComponent> addedComponents = new ResizableArray<TComponent>(4);
        private readonly ResizableArray<Entity> removedEntities = new ResizableArray<Entity>(4);

        public ComponentSet(int entityCapacity, int componentCapacity)
        {
            indices = new int[entityCapacity];
            for (int i = 0; i < entityCapacity; i++)
                indices[i] = INVALID_INDEX;

            entities = new Entity[componentCapacity];
            components = new TComponent[componentCapacity];
            changed = new Entity[componentCapacity];
        }

        public void Add(Entity entity, TComponent component)
        {
            if (entity.Index >= indices.Length)
            {
                // If somehow an Entity's index is extremely large allocate just enough space, otherwise double what we have
                var temp = new int[Math.Max(entity.Index + 1, indices.Length * 2)];
                Array.Copy(indices, temp, indices.Length);
                for (int i = Count; i < temp.Length; i++)
                    temp[i] = INVALID_INDEX;
                indices = temp;
            }

            if (Has(entity))
            {
                throw new Exception($"Failed to add component. {entity} already has component {typeof(TComponent).Name}");
            }

            addedEntities.Add(entity);
            addedComponents.Add(component);
        }

        public void Remove(Entity entity)
        {
            if (!Has(entity))
            {
                throw new Exception($"Failed to remove component. {entity} does not have component {typeof(TComponent).Name}");
            }

            removedEntities.Add(entity);
        }

        public bool TryRemove(Entity entity)
        {
            if (!Has(entity))
                return false;
            Remove(entity);
            return true;
        }

        public void NotifyChange(Entity entity)
        {
            ArrayUtil.EnsureLength(ref changed, changedCount + 1);

            changed[changedCount] = entity;
            changedCount++;
        }

        public void Update()
        {
            changedCount = 0;

            if (addedComponents.Count == 0 && addedEntities.Count == 0 && removedEntities.Count == 0)
                return;
            
            // Process adds and removes
            foreach (ref Entity entity in removedEntities.Span)
            {
                // Swap with end of list rather than shifting everything
                int newIndex = indices[entity.Index];
                indices[entities[Count - 1].Index] = newIndex;
                entities[newIndex] = entities[Count - 1];
                components[newIndex] = components[Count - 1];
                indices[entity.Index] = INVALID_INDEX;
                Count--;
            }
            
            ArrayUtil.EnsureLength(ref entities, Count + addedEntities.Count + 1);
            ArrayUtil.EnsureLength(ref components, Count + addedComponents.Count + 1);
            for (int i = 0; i < addedEntities.Span.Length; i++)
            {
                Entity entity = addedEntities.Span[i];
                indices[entity.Index] = Count;
                entities[Count] = entity;
                components[Count] = addedComponents.Span[i];
                Count++;
            }

            addedEntities.Clear();
            addedComponents.Clear();
            removedEntities.Clear();
        }

        public Span<Entity> AllEntities() => new Span<Entity>(entities, 0, Count);
        public Span<TComponent> AllComponents() => new Span<TComponent>(components, 0, Count);
        public Span<Entity> AllChanged() => new Span<Entity>(changed, 0, changedCount);
        
        public Span<Entity> AllAddedEntities() => addedEntities.Span;
        public ref TComponent GetAddedComponent(in Entity entity) => ref addedComponents.Span[addedEntities.IndexOf(entity)];

        public Span<Entity> AllRemovedEntities() => removedEntities.Span;
        public ref TComponent GetRemovedComponent(in Entity entity) => ref Get(entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(in Entity entity) => indices[entity.Index] != INVALID_INDEX;
        
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TComponent Get(in Entity entity) => ref components[indices[entity.Index]];

        public IComponent GetGeneric(in Entity entity) => (IComponent)components[indices[entity.Index]];
    }
}