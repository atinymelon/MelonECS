using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace MelonECS.UnityObserver.Editor
{
    [CustomEditor(typeof(UnityEcsObserver))]
    public class UnityEcsObserverInspector : UnityEditor.Editor
    {
        private readonly string[] tabs = new string[] { "Stats", "Systems", "Entities", "Messages" };
        private int tab;

        private Dictionary<int, bool> entityFoldouts = new Dictionary<int, bool>();
        private string entitySearch = string.Empty;
        
        private World world => (target as UnityEcsObserver).World;

        public override void OnInspectorGUI()
        {
            tab = GUILayout.Toolbar(tab, tabs);
            
            switch (tabs[tab])
            {
                case "Stats": DrawStats(); break;
                case "Systems": DrawSystems(); break;
                case "Components": DrawComponents(); break;
                case "Entities": DrawEntities(); break;
                case "Messages": DrawMessages(); break;
            }
        }

        private void DrawHeader(string text) => EditorGUILayout.LabelField(text, EditorStyles.boldLabel);

        private void DrawStats()
        {
            EditorGUILayout.LabelField("Entities", (world.entityGenerations.Count - world.entityFreeIndices.Count).ToString());
            EditorGUILayout.LabelField("Events", world.eventQueues.Where(x => x != null).Sum(x => x.Count).ToString());
            EditorGUILayout.LabelField("Systems", world.systems.Count.ToString());
            EditorGUILayout.LabelField("Queries", world.queries.Count.ToString());
        }
        
        private void DrawSystems()
        {
            foreach (var system in world.systems)
            {
                EditorGUILayout.LabelField(system.GetType().Name);
            }
        }

        private void DrawComponents()
        {
            for (int index = 0; index < world.componentSets.Length; index++)
            {
                IComponentSet componentSet = world.componentSets[index];
                if (componentSet == null)
                    continue;

                var componentType = componentSet.GetType().GetGenericArguments()[0];
                var componentFields =
                    componentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                var componentsArray = (Array) componentSet.GetType()
                    .GetField("components", BindingFlags.Instance | BindingFlags.NonPublic)?
                    .GetValue(componentSet);

                var entitiesArray = (Array) componentSet.GetType()
                    .GetField("entities", BindingFlags.Instance | BindingFlags.NonPublic)?
                    .GetValue(componentSet);

                EditorGUILayout.LabelField(componentType.Name);
                EditorGUI.indentLevel++;
                for (int i = 0; i < componentSet.Count; i++)
                {
                    object component = componentsArray?.GetValue(i);
                    Entity entity = (Entity) entitiesArray.GetValue(i);

                    EditorGUILayout.LabelField(entity.ToString());
                    EditorGUI.indentLevel++;
                    foreach (var componentField in componentFields)
                    {
                        if (componentField.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                        {
                            GUI.enabled = false;
                            EditorGUILayout.ObjectField(componentField.Name,
                                (UnityEngine.Object) componentField.GetValue(component), componentField.FieldType,
                                false);
                            GUI.enabled = true;
                        }
                        else
                        {
                            EditorGUILayout.LabelField(componentField.Name,
                                componentField.GetValue(component)?.ToString());
                        }
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }
        }

        private void DrawEntities()
        {
            EditorGUILayout.BeginHorizontal();
            entitySearch = EditorGUILayout.TextField("Search", entitySearch);

            if (GUILayout.Button("x", GUILayout.Width(24)))
            {
                entitySearch = string.Empty;
            }
            
            bool isSearching = !string.IsNullOrEmpty(entitySearch) && !string.IsNullOrWhiteSpace(entitySearch);
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < world.entityGenerations.Count; i++)
            {
                int generation = world.entityGenerations[i];
                Entity entity = new Entity(world, i, generation);
                
                var componentTypes = world.entityComponentMap.Get(entity)?.Select(ComponentType.Type).ToArray();
                if (componentTypes != null && !componentTypes.Any(x => x.Name.IndexOf(entitySearch, StringComparison.InvariantCultureIgnoreCase) != -1))
                    continue;
                
                // Skip if we're searching and it has no components
                if (isSearching && componentTypes == null)
                    continue;

                if (componentTypes == null || isSearching)
                {
                    EditorGUILayout.LabelField(entity.ToString());
                }
                else
                {
                    if (!entityFoldouts.TryGetValue(entity.Index, out bool foldout))
                        entityFoldouts.Add(entity.Index, false);
                    entityFoldouts[entity.Index] = EditorGUILayout.Foldout(foldout, entity.ToString(), true);
                }

                // Show entity but not components if it has none
                if (componentTypes == null || (entityFoldouts[entity.Index] == false && !isSearching))
                    continue;
                
                EditorGUI.indentLevel++;
                foreach (var componentType in componentTypes)
                {
                    var componentFields =
                        componentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var component = world.componentSets[ComponentType.Index(componentType)].GetGeneric(in entity);
                    
                    EditorGUILayout.LabelField(componentType.Name);
                    
                    EditorGUI.indentLevel++;
                    foreach (var componentField in componentFields)
                    {
                        if (componentField.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(componentField.Name);
                            GUI.enabled = false;
                            EditorGUILayout.ObjectField((UnityEngine.Object) componentField.GetValue(component), componentField.FieldType, false);
                            GUI.enabled = true;
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.LabelField(componentField.Name, componentField.GetValue(component)?.ToString());
                        }
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
        }

        private void DrawMessages()
        {
            foreach (var queue in world.eventQueues)
            {
                if (queue == null)
                    continue;

                EditorGUILayout.LabelField(queue.GetType().GetGenericArguments()[0].Name, queue.Count.ToString());
            }
        }
    }
}