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

        protected void SendMessage<T>(T message) where T : struct, IMessage
            => world.SendMessage(message);

        protected Span<T> GetMessages<T>() where T : struct, IMessage
            => world.GetMessages<T>();

        protected Span<Entity> GetEntities<T>() where T : struct, IComponent
            => world.GetEntities<T>();

        protected Span<Entity> QueryEntities()
            => query.GetEntities();

        protected T Resource<T>() where T : class => world.GetResource<T>();
    }
}