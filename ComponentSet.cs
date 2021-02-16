using System;
using System.Runtime.CompilerServices;

namespace MelonECS
{
    internal interface IComponentSet
    {
        void Remove(Entity entity);
        bool TryRemove(Entity entity);
        void ClearChanged();
    }
    
    internal class ComponentSet<TComponent> : IComponentSet where TComponent : struct, IComponent
    {
        private const int INVALID_INDEX = -1;
        
        private int[] indices;
        private Entity[] entities;
        private TComponent[] components;
        private int count;
        
        private Entity[] changed;
        private int changedCount;

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
                for (int i = count; i < temp.Length; i++)
                    temp[i] = INVALID_INDEX;
                indices = temp;
            }

            if (Has(entity))
            {
                throw new Exception($"Failed to add component. {entity} already has component {typeof(TComponent).Name}");
            }

            indices[entity.Index] = count;
            
            ArrayUtil.EnsureLength(ref entities, count + 1);
            ArrayUtil.EnsureLength(ref components, count + 1);

            entities[count] = entity;
            components[count] = component;
            count++;
        }

        public void Remove(Entity entity)
        {
            if (!Has(entity))
            {
                throw new Exception($"Failed to remove component. {entity} does not have component {typeof(TComponent).Name}");
            }

            // Swap with end of list rather than shifting everything
            int newIndex = indices[entity.Index];
            indices[entities[count - 1].Index] = newIndex;
            entities[newIndex] = entities[count - 1];
            components[newIndex] = components[count - 1];
            indices[entity.Index] = INVALID_INDEX;
            
            count--;
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

        public void ClearChanged() => changedCount = 0;

        public Span<Entity> AllEntities() => new Span<Entity>(entities, 0, count);
        public Span<TComponent> AllComponents() => new Span<TComponent>(components, 0, count);
        
        public Span<Entity> AllChanged() => new Span<Entity>(changed, 0, changedCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(in Entity entity) => indices[entity.Index] != INVALID_INDEX;
        
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TComponent Get(in Entity entity) => ref components[indices[entity.Index]];
    }
}