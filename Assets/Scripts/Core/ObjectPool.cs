using System.Collections.Generic;
using UnityEngine;

namespace BogatyriMoba.Core
{
    /// <summary>
    /// Generic object pool for GameObjects with IPoolable interface.
    /// Eliminates runtime instantiation overhead for projectiles, effects, and entities.
    /// </summary>
    public class ObjectPool<T> where T : Component, IPoolable
    {
        private readonly Queue<T> _pool = new Queue<T>();
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly int _initialCapacity;
        private readonly int _maxCapacity;

        public int CountActive { get; private set; }
        public int CountInactive => _pool.Count;
        public int CountAll => CountActive + CountInactive;

        public ObjectPool(T prefab, Transform parent, int initialCapacity = 32, int maxCapacity = 256)
        {
            _prefab = prefab;
            _parent = parent;
            _initialCapacity = initialCapacity;
            _maxCapacity = maxCapacity;

            Prewarm(initialCapacity);
        }

        private void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var instance = CreateInstance();
                instance.gameObject.SetActive(false);
                _pool.Enqueue(instance);
            }
        }

        private T CreateInstance()
        {
            var go = Object.Instantiate(_prefab.gameObject, _parent);
            var component = go.GetComponent<T>();
            component.OnPoolCreate();
            return component;
        }

        public T Get()
        {
            T item;
            if (_pool.Count > 0)
            {
                item = _pool.Dequeue();
            }
            else if (CountAll < _maxCapacity)
            {
                item = CreateInstance();
            }
            else
            {
                Debug.LogWarning($"[ObjectPool] Max capacity ({_maxCapacity}) reached for {_prefab.name}. Reusing oldest.");
                // Fallback: create anyway in critical situations
                item = CreateInstance();
            }

            item.gameObject.SetActive(true);
            item.OnPoolGet();
            CountActive++;
            return item;
        }

        public void Release(T item)
        {
            if (item == null) return;
            if (!item.gameObject.activeSelf) return; // already released
            item.OnPoolRelease();
            item.gameObject.SetActive(false);
            _pool.Enqueue(item);
            CountActive = Mathf.Max(0, CountActive - 1);
        }

        public void ReleaseAll()
        {
            // Note: requires tracking active items externally or iterating parent
            // For simplicity, callers should track their own instances
        }

        public void Clear()
        {
            while (_pool.Count > 0)
            {
                var item = _pool.Dequeue();
                if (item != null)
                    Object.Destroy(item.gameObject);
            }
            CountActive = 0;
        }
    }

    public interface IPoolable
    {
        void OnPoolCreate();
        void OnPoolGet();
        void OnPoolRelease();
    }

    /// <summary>
    /// Central pool manager for all prefab types.
    /// Attach to a GameObject in the scene.
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [System.Serializable]
        public class PoolConfig
        {
            public string key;
            public GameObject prefab;
            public int initialCapacity = 32;
            public int maxCapacity = 256;
        }

        [SerializeField] private PoolConfig[] poolConfigs;
        private readonly Dictionary<string, object> _pools = new Dictionary<string, object>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public ObjectPool<T> GetPool<T>(string key) where T : Component, IPoolable
        {
            if (_pools.TryGetValue(key, out var pool))
                return (ObjectPool<T>)pool;

            foreach (var config in poolConfigs)
            {
                if (config.key == key && config.prefab != null)
                {
                    var tPrefab = config.prefab.GetComponent<T>();
                    if (tPrefab != null)
                    {
                        var newPool = new ObjectPool<T>(tPrefab, transform, config.initialCapacity, config.maxCapacity);
                        _pools[key] = newPool;
                        return newPool;
                    }
                }
            }
            Debug.LogError($"[PoolManager] Pool not found for key: {key}");
            return null;
        }

        public T Get<T>(string key) where T : Component, IPoolable
        {
            var pool = GetPool<T>(key);
            return pool?.Get();
        }

        public void Release<T>(string key, T item) where T : Component, IPoolable
        {
            var pool = GetPool<T>(key);
            pool?.Release(item);
        }
    }
}
