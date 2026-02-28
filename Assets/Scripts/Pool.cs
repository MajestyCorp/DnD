using System.Collections.Generic;
using UnityEngine;

namespace DnD
{
    public class Pool : MonoBehaviour
    {
        private static Pool Instance = null;
        private Dictionary<Component, PrefabPool> _pools = new();
        private Transform _rootFolder;

        public static PrefabPool GetPrefabPool(Component prefab, int count = 0)
        {
            var instance = Instance ?? CreateInstance();

            var pools = instance._pools;
            if (!pools.TryGetValue(prefab, out var pool))
            {
                pool = new PrefabPool(prefab, instance._rootFolder, count);
                pools[prefab] = pool;
            }
            return pool;
        }

        private static Pool CreateInstance()
        {
            var obj = new GameObject("Pool");
            Instance = obj.AddComponent<Pool>();
            Instance._rootFolder = obj.transform;
            return Instance;
        }  

        public static T Take<T>(Component prefab) where T : Component
        {
            var instance = Instance ?? CreateInstance();
            var pools = instance._pools;

            if (!pools.TryGetValue(prefab, out var pool))
            {
                pool = new PrefabPool(prefab, instance._rootFolder);
                pools[prefab] = pool;
            }

            return pool.Take<T>();
        }
    }
}