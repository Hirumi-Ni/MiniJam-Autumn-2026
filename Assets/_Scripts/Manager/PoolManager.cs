using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [SerializeField] private int defaultCapacity = 32;
    [SerializeField] private int maxSize = 500;

    private readonly Dictionary<GameObject, ObjectPool<GameObject>> pools = new Dictionary<GameObject, ObjectPool<GameObject>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        if (!pools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
        {
            pool = CreatePool(prefab);
            pools.Add(prefab, pool);
        }

        GameObject instance = pool.Get();
        instance.transform.SetPositionAndRotation(position, rotation);
        return instance;
    }

    private ObjectPool<GameObject> CreatePool(GameObject prefab)
    {
        ObjectPool<GameObject> pool = null;
        pool = new ObjectPool<GameObject>(
            createFunc: () => CreateInstance(prefab, pool),
            actionOnGet: go => go.SetActive(true),
            actionOnRelease: go => go.SetActive(false),
            actionOnDestroy: go => PooledObject.SafeDestroy(go),
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize);
        return pool;
    }

    private GameObject CreateInstance(GameObject prefab, ObjectPool<GameObject> pool)
    {
        GameObject go = Instantiate(prefab, transform);
        go.SetActive(false);

        if (!go.TryGetComponent(out PooledObject member))
        {
            member = go.AddComponent<PooledObject>();
        }
        member.Pool = pool;
        return go;
    }
}