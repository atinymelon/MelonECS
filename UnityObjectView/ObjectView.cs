using UnityEngine;

namespace MelonECS.UnityObjectView
{
    public abstract class ObjectView : MonoBehaviour
    {
        public T As<T>() where T : ObjectView => this as T;

        public ViewPool Pool { get; set; }

        public void Recycle()
        {
            Pool.Recycle(this);
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}