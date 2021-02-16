using System;

namespace MelonECS
{
    public abstract class System
    {
        private World world;
        private Query query;
        
        internal void AttachWorld(World world)
        {
            this.world = world;
            
            foreach (object attribute in GetType().GetCustomAttributes(true))
            {
                switch (attribute)
                {
                    case QueryWithAttribute queryWith:
                    {
                        query ??= new Query(world);
                        foreach (Type type in queryWith.types)
                        {
                            query.With(type);
                        }

                        break;
                    }
                    case QueryExcludeAttribute queryExclude:
                    {
                        query ??= new Query(world);
                        foreach (Type type in queryExclude.types)
                        {
                            query.Exclude(type);
                        }

                        break;
                    }
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

        protected T Resource<T>() where T : class => world.GetResource<T>();
    }
}