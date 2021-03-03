using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MelonECS
{
    public abstract class System
    {
        private World world;
        private Query query;
        
        public virtual void Init(World world)
        {
            this.world = world;

            var attributes = GetType().GetCustomAttributes(true);

            IEnumerable<Type> queryIncludeTypes = null;
            IEnumerable<Type> queryExcludeTypes = null;
            foreach (var attribute in attributes)
            {
                switch (attribute)
                {
                    case QueryIncludeAttribute includeAttribute:
                        queryIncludeTypes = includeAttribute.types;
                        break;
                    case QueryExcludeAttribute excludeAttribute:
                        queryExcludeTypes = excludeAttribute.types;
                        break;
                }
            }
  
            if ( queryIncludeTypes != null || queryExcludeTypes != null )
                query = world.CreateQuery(queryIncludeTypes, queryExcludeTypes);
            
            // Setup resources
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                if (field.IsStatic || !field.IsDefined(typeof(ResourceAttribute)))
                    continue;

                if (world.TryGetResource(field.FieldType, out object resource))
                {
                    field.SetValue(this, resource);
                }
                else
                {
                    Debug.LogError($"No resource of type {field.FieldType.FullName} exists");
                }
            }
        }
        
        public abstract void Run();

        protected Span<T> GetComponents<T>() where T : struct, IComponent
            => world.GetComponents<T>();

        protected ref T GetComponent<T>(in Entity entity) where T : struct, IComponent
            => ref world.GetComponent<T>(entity);
        
        protected bool HasComponent<T>(in Entity entity) where T : struct, IComponent
            => world.HasComponent<T>(entity);

        protected void PushMessage<T>(T message) where T : struct, IMessage
            => world.PushMessage(message);

        protected Span<T> ReadMessages<T>() where T : struct, IMessage
            => world.ReadMessages<T>();

        protected Span<Entity> GetEntitiesWithComponent<T>() where T : struct, IComponent
            => world.GetEntitiesWithComponent<T>();

        protected Span<Entity> QueryEntities()
            => query.GetEntities();
        
        protected Span<Entity> GetChanged<T>() where T : struct, IComponent
            => world.GetChanged<T>();

        protected void NotifyChange<T>(in Entity entity) where T : struct, IComponent
            => world.NotifyChange<T>(entity);

        protected delegate void ForEachDelegate<T>(in Entity entity, ref T comp) 
            where T : struct, IComponent;
        protected delegate void ForEachDelegate<T1, T2>(in Entity entity, ref T1 comp1, ref T2 comp2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent;
        protected delegate void ForEachDelegate<T1, T2, T3>(in Entity entity, ref T1 comp1, ref T2 comp2, ref T3 comp3) 
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent;

        protected void ForEach<T>(Span<Entity> entities, ForEachDelegate<T> func) where T : struct, IComponent
        {
            foreach (ref Entity entity in entities)
            {
                func(entity, ref GetComponent<T>(entity));
            }
        }

        protected void ForEach<T1, T2>(Span<Entity> entities, ForEachDelegate<T1, T2> func)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            foreach (ref Entity entity in entities)
            {
                func(entity, ref GetComponent<T1>(entity), ref GetComponent<T2>(entity));
            }
        }
        
        protected void ForEach<T1, T2, T3>(Span<Entity> entities, ForEachDelegate<T1, T2, T3> func) 
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            foreach (ref Entity entity in entities)
            {
                func(entity, ref GetComponent<T1>(entity), ref GetComponent<T2>(entity), ref GetComponent<T3>(entity));
            }
        }
    }
}