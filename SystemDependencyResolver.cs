using System;
using System.Collections.Generic;
using UnityEngine;

namespace MelonECS
{
    internal class SystemDependencyResolver
    {
        class Node : IEquatable<Node>
        {
            public readonly System system;
            public HashSet<Type> readEvents = new HashSet<Type>();
            public HashSet<Type> writeEvents = new HashSet<Type>();
            public List<Node> edges = new List<Node>();

            public Node(System system)
            {
                this.system = system;
                
                var attributes = system.GetType().GetCustomAttributes(true);
                foreach (var attribute in attributes)
                {
                    switch (attribute)
                    {
                        case ReadEventsAttribute read:
                            foreach (Type type in read.types)
                                readEvents.Add(type);
                            break;
                        case WriteEventsAttribute write:
                            foreach (Type type in write.types)
                                writeEvents.Add(type);
                            break;
                    }
                }
            }

            public bool Equals(Node other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(system, other.system);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Node) obj);
            }

            public override int GetHashCode()
            {
                return (system != null ? system.GetHashCode() : 0);
            }
        }
        
        public System[] ResolveDependencies(IEnumerable<System> systems)
        {
            var readSystems = new Dictionary<Type, List<Node>>();
            var writeSystems = new Dictionary<Type, List<Node>>();
            
            var nodes = new List<Node>();
            foreach (var system in systems)
            {
                var node = new Node(system);
                nodes.Add(node);

                foreach (Type type in node.readEvents)
                    TryAdd(type, node, readSystems);
                foreach (Type type in node.writeEvents)
                    TryAdd(type, node, writeSystems);
            }

            foreach (Node node in nodes)
            {
                foreach (Type type in node.readEvents)
                {
                    if ( writeSystems.TryGetValue(type, out var list) )
                        node.edges.AddRange(list);
                }
            }

            // var toResolve = new Stack<Node>();
            // toResolve.Push(nodes[0]);
            // while (toResolve.Count > 0)
            // {
            //     var node = toResolve.Pop();
            //     for (int i = node.edges.Count - 1; i >= 0; i--)
            //     {
            //         var edge = node.edges[i];
            //         toResolve.Push(edge);
            //     }
            // }

            var resolved = new List<System>();
            var seen = new HashSet<Node>();
            Resolve(nodes[0], resolved, seen);

            return resolved.ToArray();
        }

        private void Resolve(Node node, List<System> resolved, HashSet<Node> seen)
        {
            Debug.Log(node.system.GetType().Name);
            seen.Add(node);
            foreach (var edge in node.edges)
            {
                if (resolved.Contains(edge.system))
                    continue;
                
                if (seen.Contains(edge))
                {
                    Debug.LogError($"Circular dependency detected: {node.system.GetType().Name} -> {edge.system.GetType().Name}");
                    continue;
                }

                Resolve(edge, resolved, seen);
            }
            resolved.Add(node.system);
        }

        private void TryAdd(Type type, Node node, Dictionary<Type, List<Node>> dictionary)
        {
            if (!dictionary.TryGetValue(type, out var nodes))
            {
                nodes = new List<Node>();
                dictionary.Add(type, nodes);
            }
            nodes.Add(node);
        }
    }
}