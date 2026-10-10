using System.Collections.Generic;
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

    #region [Spawn Settings]
    [Header("Performance Cap")]
    [SerializeField] private int maxLeavesInWorld = 100;
    #endregion

    #region [Attributes]
    private float spawnTimer = 0f;
    #endregion

    #region [Unity Lifecycle]
    private void Update()
    {
        spawnTimer += Time.deltaTime;

        // Fetch current interval stat from UpgradeManager (fallback to 3.0s if UpgradeManager isn't in scene)
        float currentInterval = UpgradeManager.Instance != null
            ? UpgradeManager.Instance.GetSpawnInterval()
            : 3.0f;

        if (spawnTimer >= currentInterval)
        {
            spawnTimer = 0f;
            TriggerBurstSpawn();
        }
    }
    #endregion

    #region [Methods]
    private void TriggerBurstSpawn()
    {
        if (leafPrefab == null || leafTypeList == null || leafTypeList.Count == 0 || spawnAreaCollider == null) return;

        // Fetch current burst count from UpgradeManager (fallback to 1 if UpgradeManager isn't in scene)
        int burstCount = UpgradeManager.Instance != null
            ? UpgradeManager.Instance.GetBurstSpawnCount()
            : 1;

        // Check how many active leaves currently exist in the scene
        int currentLeafCount = FindObjectsByType<LeafController>(FindObjectsSortMode.None).Length;

        for (int i = 0; i < burstCount; i++)
        {
            if (currentLeafCount >= maxLeavesInWorld) break;

            SpawnSingleLeaf();
            currentLeafCount++;
        }
    }

    private void SpawnSingleLeaf()
    {
        LeafSO selectedLeafData = GetRandomWeightedLeaf();
        if (selectedLeafData == null) return;

        Vector2 spawnPos = GetRandomSpawnPosition();
        Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        GameObject instance = Instantiate(leafPrefab, spawnPos, randomRotation, transform);

        if (instance.TryGetComponent<LeafController>(out LeafController leafController))
        {
            leafController.Initialize(selectedLeafData);
        }
    }

    private LeafSO GetRandomWeightedLeaf()
    {
        if (leafTypeList.Count == 0) return null;

        float totalWeight = 0f;
        foreach (LeafSO leaf in leafTypeList)
        {
            if (leaf != null)
            {
                totalWeight += leaf.spawnWeight;
            }
        }

        float randomRoll = Random.Range(0f, totalWeight);
        float currentWeightSum = 0f;

        foreach (LeafSO leaf in leafTypeList)
        {
            if (leaf == null) continue;

            currentWeightSum += leaf.spawnWeight;
            if (randomRoll <= currentWeightSum)
            {
                return leaf;
            }
        }

        return leafTypeList[0];
    }

    private Vector2 GetRandomSpawnPosition()
    {
        if (spawnAreaCollider == null) return transform.position;

        Bounds bounds = spawnAreaCollider.bounds;

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);

        return new Vector2(randomX, randomY);
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnAreaCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(spawnAreaCollider.bounds.center, spawnAreaCollider.bounds.size);
        }
    }
    #endregion
}