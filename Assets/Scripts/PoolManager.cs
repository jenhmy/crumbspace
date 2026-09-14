using System;
using System.Collections.Generic;
using UnityEngine;

public enum PoolId
{
    Asteroid01,
    Asteroid02,
    Asteroid03,
    Asteroid04,
    Asteroid05,
    Star,
    Croissant,
    Magnet,
    Missile,
    Invisibility,
    Snow,
    Dynamite,
    BonusCollectibleStar,
    BonusCollectibleCroissant,
    BonusObstacle01,
    BonusObstacle02,

    AsteroidExplosion,
    CollectEffect,
    PlayerExplosion,
    AlienExplosion
}

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [Serializable]
    private class PoolEntry
    {
        public PoolId poolId;
        public GameObject prefab;
        [Min(0)] public int initialSize = 3;
        public bool allowRuntimeExpansion = true;

        [NonSerialized] public List<GameObject> instances;
    }

    [SerializeField] private PoolEntry[] pools;

    private readonly Dictionary<PoolId, PoolEntry> poolsById = new Dictionary<PoolId, PoolEntry>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (pools == null)
        {
            return;
        }

        for (int i = 0; i < pools.Length; i++)
        {
            PoolEntry pool = pools[i];
            if (pool == null)
            {
                continue;
            }

            if (pool.prefab == null)
            {
                Debug.LogError($"Pool {pool.poolId} has no prefab assigned.", this);
                continue;
            }

            if (poolsById.ContainsKey(pool.poolId))
            {
                Debug.LogError($"PoolId {pool.poolId} is configured more than once.", this);
                continue;
            }

            pool.instances = new List<GameObject>(pool.initialSize);
            poolsById.Add(pool.poolId, pool);

            for (int instanceIndex = 0; instanceIndex < pool.initialSize; instanceIndex++)
            {
                CreateInstance(pool);
            }
        }
    }

    public GameObject Get(PoolId poolId)
    {
        if (!poolsById.TryGetValue(poolId, out PoolEntry pool))
        {
            Debug.LogError($"No pool is configured for {poolId}.", this);
            return null;
        }

        for (int instanceIndex = 0;
             instanceIndex < pool.instances.Count;
             instanceIndex++)
        {
            GameObject instance = pool.instances[instanceIndex];

            if (instance == null)
            {
                pool.instances.RemoveAt(instanceIndex);
                instanceIndex--;
                continue;
            }

            if (!instance.activeSelf)
            {
                return instance;
            }
        }

        if (!pool.allowRuntimeExpansion)
        {
            Debug.LogError(
                $"Pool {poolId} has no inactive instances available.",
                this);

            return null;
        }

        return CreateInstance(pool);
    }

    public IEnumerable<GameObject> GetActiveInstances(PoolId poolId)
    {
        if (!poolsById.TryGetValue(poolId, out PoolEntry pool))
        {
            yield break;
        }

        for (int instanceIndex = 0;
             instanceIndex < pool.instances.Count;
             instanceIndex++)
        {
            GameObject instance = pool.instances[instanceIndex];
            if (instance != null && instance.activeSelf)
            {
                yield return instance;
            }
        }
    }

    private GameObject CreateInstance(PoolEntry pool)
    {
        GameObject instance = Instantiate(pool.prefab, transform);
        instance.SetActive(false);
        pool.instances.Add(instance);
        return instance;
    }
}
