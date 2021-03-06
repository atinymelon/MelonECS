using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MelonECS.UnityObjectView
{
    public class ViewPool : MonoBehaviour
    {
        [SerializeField] private ObjectView prefab;
        [SerializeField] private bool isScenePrefab;

        private readonly Queue<ObjectView> freeViews = new Queue<ObjectView>();
        private readonly List<ObjectView> allViews = new List<ObjectView>();

        private void Awake()
        {
            if (isScenePrefab)
                prefab.Hide();
        }

        public T Instantiate<T>() where T : ObjectView
        {
            T view = freeViews.Count == 0 ? CreateInstance<T>() : freeViews.Dequeue().As<T>();
            view.Show();
            return view;
        }

        public T Instantiate<T>(Vector3 worldPosition) where T : ObjectView
        {
            var instance = Instantiate<T>();
            instance.transform.position = worldPosition;
            return instance;
        }

        public void Recycle(ObjectView view)
        {
            if (!allViews.Contains(view))
            {
                Debug.LogError($"Attempted to recycle object to wrong pool!", view);
                return;
            }

            view.Hide();
            freeViews.Enqueue(view);
        }

        public void RecycleAll()
        {
            foreach (ObjectView view in allViews)
            {
                view.Hide();
                freeViews.Enqueue(view);
            }
        }
        
        private T CreateInstance<T>() where T : ObjectView
        {
            var instance = Object.Instantiate(prefab, transform, false).GetComponent<T>();
            instance.Pool = this;
            
            allViews.Add(instance);
            
            return instance;
        }
    }
}