using System;
using System.Collections.Generic;

namespace MelonECS
{
    public class World
    {
        // To prevent entity ids from being reused too often, we prevent reuse until this many entities have been destroyed
        private const int MINIMUM_ENTITY_FREE_INDICES = 1024;

        internal readonly List<int> entityGenerations = new List<int>();
        internal readonly Queue<int> entityFreeIndices = new Queue<int>();

        internal IComponentSet[] componentSets = new IComponentSet[16];
        internal readonly EntityComponentMap entityComponentMap = new EntityComponentMap(MINIMUM_ENTITY_FREE_INDICES);

        internal readonly List<Query> queries = new List<Query>();
        internal readonly List<System> systems = new List<System>();
        internal IMessageQueue[] messageQueues = new IMessageQueue[16];
        private readonly Dictionary<Type, object> resources = new Dictionary<Type, object>();
        private bool areEntityComponentChanges = false;

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

            areEntityComponentChanges = true;
            
            Entity entity = MakeEntity(this, index, entityGenerations[index]);
            return entity;
        }

        public static Entity MakeEntity(World world, int index, int generation) 
            => new Entity(world, index, generation);

        public bool IsEntityAlive(in Entity entity) => entity.Index != -1 && entityGenerations[entity.Index] == entity.Generation;

        public void DestroyEntity(in Entity entity)
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
            
            areEntityComponentChanges = true;
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
            
            areEntityComponentChanges = true;
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
            
            areEntityComponentChanges = true;
        }
        
        public bool HasComponent<T>(in Entity entity) where T : struct, IComponent
        {
            return ComponentType<T>.Index < componentSets.Length && (((ComponentSet<T>) componentSets[ComponentType<T>.Index])?.Has(entity) ?? false);
        }

        public bool HasComponent(in Entity entity, Type componentType)
        {
            int index = ComponentType.Index(componentType);
            return index < componentSets.Length && (componentSets[index]?.Has(entity) ?? false);
        }

        public ref T GetComponent<T>(in Entity entity) where T : struct, IComponent
        {
            return ref ((ComponentSet<T>) componentSets[ComponentType<T>.Index]).Get(entity);
        }

        public Span<T> GetComponents<T>() where T : struct, IComponent
        {
            return ((ComponentSet<T>) componentSets[ComponentType<T>.Index]).AllComponents();
        }
        
        public Span<Entity> GetEntitiesWithComponent<T>() where T : struct, IComponent
        {
            return ((ComponentSet<T>) componentSets[ComponentType<T>.Index]).AllEntities();
        }

        public void NotifyChange<T>(in Entity entity) where T : struct, IComponent
        {
            ((ComponentSet<T>) componentSets[ComponentType<T>.Index]).NotifyChange(entity);
        }

        public Span<Entity> GetChanged<T>() where T : struct, IComponent
        {
            return ((ComponentSet<T>) componentSets[ComponentType<T>.Index]).AllChanged();
        }

        public Span<Entity> GetAddedEntities<T>() where T : struct, IComponent
        {
            return ((ComponentSet<T>) componentSets[ComponentType<T>.Index]).AllAddedEntities();
        }

        public ref T GetAddedComponent<T>(in Entity entity) where T : struct, IComponent
        {
            return ref ((ComponentSet<T>) componentSets[ComponentType<T>.Index]).GetAddedComponent(entity);
        }

        public Span<Entity> GetRemovedEntities<T>() where T : struct, IComponent
        {
            return ((ComponentSet<T>) componentSets[ComponentType<T>.Index]).AllRemovedEntities();
        }

        public ref T GetRemovedComponent<T>(in Entity entity) where T : struct, IComponent
        {
            return ref ((ComponentSet<T>) componentSets[ComponentType<T>.Index]).GetRemovedComponent(entity);
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

        public void AddResource<T>(T resource) where T : class => resources.Add(typeof(T), resource);
        public T GetResource<T>() where T : class => (T)resources[typeof(T)];
        public bool TryGetResource(Type type, out object resource) => resources.TryGetValue(type, out resource);

        #endregion
        
        #region Queries
        
        public Query CreateQuery(IEnumerable<Type> include, IEnumerable<Type> exclude)
        {
            var query = new Query(include, exclude);
            queries.Add(query);
            return query;
        }

        #endregion
        
        #region Systems
        
        public void RegisterSystem<T>() where T : System, new() 
            => systems.Add(new T());

        public void Init()
        {
            foreach (System system in systems)
            {
                system.Init(this);
            }
        }

        public void Update()
        {
            for (int i = 0; i < systems.Count; i++)
            {
                systems[i].Run();
                Flush();
            }
        
            for (int i = 0; i < messageQueues.Length; i++)
            {
                messageQueues[i]?.Clear();
            }

            for (int i = 0; i < componentSets.Length; i++)
            {
                componentSets[i]?.EndOfFrameCleanup();
            }
        }

        public void Flush()
        {
            if (!areEntityComponentChanges)
                return;

            areEntityComponentChanges = false;
            
            for (int i = 0; i < componentSets.Length; i++)
            {
                componentSets[i]?.FlushAddsAndRemoves();
            }
            
            for (int i = 0; i < queries.Count; i++)
            {
                queries[i]?.FlushAddsAndRemoves();
            }
        }
        
        #endregion

        #region Messages

        public void PushMessage<T>(T evt) where T : struct, IMessage
        {
            if (messageQueues[MessageType<T>.Index] == null)
            {
                ArrayUtil.EnsureLength(ref messageQueues, MessageType<T>.Index + 1);
                messageQueues[MessageType<T>.Index] = new MessageQueue<T>();
            }
            ((MessageQueue<T>) messageQueues[MessageType<T>.Index]).Push(evt);
        }

        public Span<T> ReadMessages<T>() where T : struct, IMessage
        {
            if (messageQueues[MessageType<T>.Index] == null)
            {
                ArrayUtil.EnsureLength(ref messageQueues, MessageType<T>.Index + 1);
                messageQueues[MessageType<T>.Index] = new MessageQueue<T>();
            }
            return ((MessageQueue<T>) messageQueues[MessageType<T>.Index]).Read();
        }

        #endregion
    }
}