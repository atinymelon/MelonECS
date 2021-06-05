using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MelonECS
{
    public abstract class System<T> : System where T : IContext
    {
        protected T Context { get; private set; }
        
        public override void Init(World world)
        {
            base.Init(world);

            Context = ((World<T>) world).Context;
        }
    }
    
    public abstract class System
    {
        protected World world;
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
        }
        
        public abstract void Run();

        protected bool IsEntityAlive(in Entity entity) => world.IsEntityAlive(entity);

        protected ArrayRef<T> GetComponents<T>() where T : struct, IComponent
            => world.GetComponents<T>();

        protected ref T GetComponent<T>(in Entity entity) where T : struct, IComponent
            => ref world.GetComponent<T>(entity);
        
        protected bool HasComponent<T>(in Entity entity) where T : struct, IComponent
            => world.HasComponent<T>(entity);

        protected void PushEvent<T>(T evt) where T : struct, IEvent
            => world.PushEvent(evt);

        protected ArrayRef<T> ReadEvents<T>() where T : struct, IEvent
            => world.ReadEvents<T>();

        protected bool AnyEvents<T>() where T : struct, IEvent
            => world.ReadEvents<T>().Length > 0;

        protected Entity GetEntityWithComponent<T>() where T : struct, IComponent
            => world.GetEntitiesWithComponent<T>()[0];

        protected ArrayRef<Entity> GetEntitiesWithComponent<T>() where T : struct, IComponent
            => world.GetEntitiesWithComponent<T>();

        protected ArrayRef<Entity> QueryEntities()
            => query.GetEntities();
        
        protected ArrayRef<Entity> GetChanged<T>() where T : struct, IComponent
            => world.GetChanged<T>();

        protected void NotifyChange<T>(in Entity entity) where T : struct, IComponent
            => world.NotifyChange<T>(entity);
    }
}