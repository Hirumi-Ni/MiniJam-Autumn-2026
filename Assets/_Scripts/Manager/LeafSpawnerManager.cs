using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class LeafSpawnerManager : MonoBehaviour
{
    #region [References]
    [Header("Spawn Data")]
    [SerializeField] private GameObject leafPrefab;
    [SerializeField] private List<LeafSO> leafTypeList = new List<LeafSO>();

    [Header("Spawn Area")]
    [SerializeField] private BoxCollider2D spawnAreaCollider;
    #endregion

    #region [Spawn Rate]
    [Header("Spawn Rate")]
    [SerializeField] private float baseSpawnRate = 1f / 3f;
    [SerializeField] private float maxSpawnRate = 8f;

    [Header("Swarm Scatter")]
    [SerializeField] private float swarmDelay = 0.15f;

    [Header("Performance Cap")]
    [SerializeField] private int maxLeavesInWorld = 150;
    #endregion

    #region [Attributes]
    private float spawnTimer;
    private int pendingSpawns;
    #endregion

    #region [Unity Lifecycle]
    private void Update()
    {
        spawnTimer += Time.deltaTime;

        float rate = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetStat(StatType.SpawnRate, baseSpawnRate) : baseSpawnRate;
        rate = Mathf.Min(rate, maxSpawnRate);

        float interval = 1f / Mathf.Max(0.01f, rate);

        if (spawnTimer >= interval)
        {
            spawnTimer -= interval;
            TriggerBurst();
        }
    }
    #endregion

    #region [Methods]
    private void TriggerBurst()
    {
        if (leafPrefab == null || leafTypeList.Count == 0 || spawnAreaCollider == null || PoolManager.Instance == null) return;

        int burstCount = UpgradeManager.Instance != null ? Mathf.Max(1, Mathf.RoundToInt(UpgradeManager.Instance.GetStat(StatType.BurstCount, 1f))) : 1;
        int room = maxLeavesInWorld - LeafController.ActiveCount - pendingSpawns;
        int toSpawn = Mathf.Min(burstCount, room);

        for (int i = 0; i < toSpawn; i++)
        {
            pendingSpawns++;
            DOVirtual.DelayedCall(i * swarmDelay, () =>
            {
                pendingSpawns--;
                SpawnSingleLeaf();
            }).SetLink(gameObject);
        }
    }

    private void SpawnSingleLeaf()
    {
        LeafSO data = GetRandomWeightedLeaf();
        if (data == null) return;

        Vector2 spawnPos = GetRandomSpawnPosition();
        Quaternion rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        GameObject instance = PoolManager.Instance.Spawn(leafPrefab, spawnPos, rotation);
        if (instance != null && instance.TryGetComponent(out LeafController leaf))
        {
            leaf.Spawn(data);
        }
    }

    private LeafSO GetRandomWeightedLeaf()
    {
        float totalWeight = 0f;
        foreach (LeafSO leaf in leafTypeList)
        {
            if (leaf != null) totalWeight += leaf.spawnWeight;
        }
        if (totalWeight <= 0f) return null;

        float roll = Random.Range(0f, totalWeight);
        float sum = 0f;

        foreach (LeafSO leaf in leafTypeList)
        {
            if (leaf == null) continue;
            sum += leaf.spawnWeight;
            if (roll <= sum) return leaf;
        }

        return leafTypeList[0];
    }

    private Vector2 GetRandomSpawnPosition()
    {
        Bounds bounds = spawnAreaCollider.bounds;
        return new Vector2(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y));
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnAreaCollider == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(spawnAreaCollider.bounds.center, spawnAreaCollider.bounds.size);
    }
    #endregion
}