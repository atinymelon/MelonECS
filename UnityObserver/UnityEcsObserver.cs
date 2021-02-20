using System.Runtime.CompilerServices;
using UnityEngine;

namespace MelonECS.UnityObserver
{
    public class UnityEcsObserver : MonoBehaviour
    {
        public World World { get; set; }

        public static void Observe(World world)
        {
            GameObject gameObject = new GameObject("EcsObserver");
            gameObject.AddComponent<UnityEcsObserver>().World = world;
        }
    }
}