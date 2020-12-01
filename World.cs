using System;
using System.Collections.Generic;

namespace MelonECS
{
    public class World
    {
        // To prevent entity ids from being reused too often, we prevent reuse until this many entities have been destroyed
        private const int MINIMUM_ENTITY_FREE_INDICES = 1024;

        private readonly List<int> entityGenerations = new List<int>();
        private readonly Queue<int> entityFreeIndices = new Queue<int>();

        private IComponentSet[] componentSets = new IComponentSet[16];
        private readonly EntityComponentMap entityComponentMap = new EntityComponentMap(MINIMUM_ENTITY_FREE_INDICES);

        private readonly List<Query> queries = new List<Query>();
        private IMessageQueue[] messageQueues = new IMessageQueue[16];
        private readonly Dictionary<Type, object> resources = new Dictionary<Type, object>();

        #region Entities

        public Entity CreateEntity()
        {
            int index;
            if (entityFreeIndices.Count > MINIMUM_ENTITY_FREE_INDICES)
            {
                index = entityFreeIndices.Dequeue();
            }
            else
            {
                entityGenerations.Add(0);
                index = entityGenerations.Count - 1;
            }

            Entity entity = MakeEntity(this, index, entityGenerations[index]);
            return entity;
        }

        public static Entity MakeEntity(World world, int index, int generation) 
            => new Entity(world, index, generation);

        public bool IsEntityAlive(Entity entity) => entityGenerations[entity.Index] == entity.Generation;

        public void DestroyEntity(Entity entity)
        {
            int index = entity.Index;
            ++entityGenerations[index];
            entityFreeIndices.Enqueue(index);

            foreach (int componentType in entityComponentMap.Get(entity))
            {   
                componentSets[componentType].Remove(entity);
            }

            foreach (Query query in queries)
            {
                if (query.IsMatch(entityComponentMap.Get(entity)))
                    query.RemoveEntity(entity);
            }
            
            entityComponentMap.RemoveAll(entity);
        }

        #endregion

        #region Components

        public void AddComponent<T>(Entity entity, T component) where T : struct, IComponent
        {
            RegisterComponentType<T>();
            ((ComponentSet<T>) componentSets[ComponentType<T>.Index]).Add(entity, component);
            entityComponentMap.Add(entity, ComponentType<T>.Index);

            foreach (Query query in queries)
            {
                if (query.IsMatch(entityComponentMap.Get(entity)))
                    query.AddEntity(entity);
                else
                    query.RemoveEntity(entity);
            }
        }
        
        public void RemoveComponent<T>(Entity entity) where T : struct, IComponent
        {
            RegisterComponentType<T>();
            ((ComponentSet<T>) componentSets[ComponentType<T>.Index]).Remove(entity);
            entityComponentMap.Remove(entity, ComponentType<T>.Index);
            
            foreach (Query query in queries)
            {
                if (query.IsMatch(entityComponentMap.Get(entity)))
                    query.AddEntity(entity);
                else
                    query.RemoveEntity(entity);
            }
        }
        
        public bool HasComponent<T>(in Entity entity) where T : struct, IComponent
        {
            // RegisterComponentType<T>();
            return ((ComponentSet<T>) componentSets[ComponentType<T>.Index]).Has(entity);
        }

        public ref T GetComponent<T>(in Entity entity) where T : struct, IComponent
        {
            // RegisterComponentType<T>();
            return ref ((ComponentSet<T>) componentSets[ComponentType<T>.Index]).Get(entity);
        }
        
        public Span<T> GetComponents<T>() where T : struct, IComponent
        {
            // RegisterComponentType<T>();
            return ((ComponentSet<T>) componentSets[ComponentType<T>.Index]).AllComponents();
        }
        
        public Span<Entity> GetEntities<T>() where T : struct, IComponent
        {
            // RegisterComponentType<T>();
            return ((ComponentSet<T>) componentSets[ComponentType<T>.Index]).AllEntities();
        }

        private void RegisterComponentType<T>() where T : struct, IComponent
        {
            ArrayUtil.EnsureLength(ref componentSets, ComponentType<T>.Index + 1);

            if (componentSets[ComponentType<T>.Index] == null)
            {
                var set = new ComponentSet<T>(1024, 16);
                componentSets[ComponentType<T>.Index] = set;
            }
        }

        #endregion

        #region Resources

        public void AddResource<T>(T resource) where T : class
        {
            resources.Add(typeof(T), resource);
        }

        public T GetResource<T>() where T : class => (T)resources[typeof(T)];

        #endregion
        
        #region Queries
        
        public void RegisterQuery(Query query)
        {
            queries.Add(query);
        }

        #endregion
        
        #region Systems
        
        // public void RegisterSystem<T>() where T : System, new()
        // {
        //     var system = new T();
        //     system.AttachWorld(this);
        //     systems.Add(system);
        // }
        
        public void Update()
        {
            // for (int i = 0; i < systems.Count; i++)
            // {
            //     systems[i].Run();
            // }
        
            for (int i = 0; i < messageQueues.Length; i++)
            {
                messageQueues[i]?.Clear();
            }
        }
        
        #endregion

        #region Messages

        public void SendMessage<T>(T evt) where T : struct, IMessage 
            => ((MessageQueue<T>) messageQueues[MessageType<T>.Index]).Push(evt);

        public void RegisterMessage<T>() where T : struct, IMessage
        {
            ArrayUtil.EnsureLength(ref messageQueues, MessageType<T>.Index + 1);
            messageQueues[MessageType<T>.Index] = new MessageQueue<T>();
        }

        public Span<T> GetMessages<T>() where T : struct, IMessage 
            => ((MessageQueue<T>) messageQueues[MessageType<T>.Index]).Read();

        #endregion
    }
}