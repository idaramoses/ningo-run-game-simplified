using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Centralized object pooling system for coins, letters, obstacles, and other frequently spawned objects.
/// Eliminates expensive Instantiate/Destroy calls that cause lag.
/// </summary>
public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int initialSize = 20;
    }

    [Header("Pool Configurations")]
    public List<Pool> pools = new List<Pool>();

    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, GameObject> prefabDictionary;
    private Dictionary<int, string> objectTagCache = new Dictionary<int, string>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializePools();
    }

    void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        prefabDictionary = new Dictionary<string, GameObject>();

        foreach (Pool pool in pools)
        {
            if (pool.prefab == null)
            {
                Debug.LogWarning($"[ObjectPoolManager] Pool '{pool.tag}' has null prefab!");
                continue;
            }

            Queue<GameObject> objectPool = new Queue<GameObject>();
            prefabDictionary[pool.tag] = pool.prefab;

            for (int i = 0; i < pool.initialSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
                objectTagCache[obj.GetInstanceID()] = pool.tag;
            }

            poolDictionary[pool.tag] = objectPool;
            Debug.Log($"[ObjectPoolManager] Initialized pool '{pool.tag}' with {pool.initialSize} objects");
        }
    }

    /// <summary>
    /// Get an object from the pool. Creates new instance if pool is empty.
    /// </summary>
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            // Try to find if we have a prefab for this tag in the list to auto-initialize
            Pool foundPool = pools.Find(p => p.tag == tag);
            if (foundPool != null && foundPool.prefab != null)
            {
                CreatePool(foundPool);
            }
            else
            {
                Debug.LogWarning($"[ObjectPoolManager] Pool with tag '{tag}' doesn't exist and no prefab is configured!");
                return null;
            }
        }

        GameObject objectToSpawn;
        if (poolDictionary[tag].Count > 0)
        {
            objectToSpawn = poolDictionary[tag].Dequeue();
        }
        else
        {
            objectToSpawn = Instantiate(prefabDictionary[tag], transform);
            objectTagCache[objectToSpawn.GetInstanceID()] = tag;
        }

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }

    private void CreatePool(Pool pool)
    {
        if (poolDictionary.ContainsKey(pool.tag)) return;

        Queue<GameObject> objectPool = new Queue<GameObject>();
        prefabDictionary[pool.tag] = pool.prefab;

        for (int i = 0; i < pool.initialSize; i++)
        {
            GameObject obj = Instantiate(pool.prefab, transform);
            obj.SetActive(false);
            objectPool.Enqueue(obj);
            objectTagCache[obj.GetInstanceID()] = pool.tag;
        }

        poolDictionary[pool.tag] = objectPool;
        Debug.Log($"[ObjectPoolManager] Dynamically created pool '{pool.tag}' with {pool.initialSize} objects");
    }

    /// <summary>
    /// Add a new pool at runtime
    /// </summary>
    public bool HasPool(string tag)
    {
        return poolDictionary != null && poolDictionary.ContainsKey(tag);
    }

    public void AddPool(string tag, GameObject prefab, int initialSize = 10)
    {
        if (poolDictionary.ContainsKey(tag)) return;
        
        Pool newPool = new Pool { tag = tag, prefab = prefab, initialSize = initialSize };
        pools.Add(newPool);
        CreatePool(newPool);
    }

    /// <summary>
    /// Return an object to the pool for reuse.
    /// </summary>
    public void ReturnToPool(string tag, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"[ObjectPoolManager] Pool with tag '{tag}' doesn't exist! Destroying object.");
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        obj.transform.SetParent(transform);
        poolDictionary[tag].Enqueue(obj);
    }

    /// <summary>
    /// Alternative method that auto-detects pool tag from object name.
    /// </summary>
    public void ReturnToPool(GameObject obj)
    {
        string tag = GetPoolTagFromObject(obj);
        if (!string.IsNullOrEmpty(tag))
        {
            ReturnToPool(tag, obj);
        }
        else
        {
            Debug.LogWarning($"[ObjectPoolManager] Could not determine pool tag for {obj.name}");
            Destroy(obj);
        }
    }

    private string GetPoolTagFromObject(GameObject obj)
    {
        if (obj == null) return null;
        
        // Fast, garbage-free O(1) lookup
        if (objectTagCache.TryGetValue(obj.GetInstanceID(), out string tag))
        {
            return tag;
        }

        // Fallback to string matching if not in cache (e.g., if instantiated outside of ObjectPoolManager)
        foreach (var kvp in poolDictionary)
        {
            if (obj.name.Contains(kvp.Key) || obj.name.Contains(prefabDictionary[kvp.Key].name))
            {
                return kvp.Key;
            }
        }
        return null;
    }

    /// <summary>
    /// Clear all pools (useful for scene transitions).
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var pool in poolDictionary.Values)
        {
            while (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                if (obj != null)
                    Destroy(obj);
            }
        }
        poolDictionary.Clear();
        prefabDictionary.Clear();
        objectTagCache.Clear();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
