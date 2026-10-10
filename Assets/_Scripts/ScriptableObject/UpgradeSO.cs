using UnityEngine;

public enum UpgradeType
{
    MagnetRadius,
    BurstSpawnCount,
    SpawnInterval,
    CoinMultiplier
}
[CreateAssetMenu(fileName = "NewUpgradeSO", menuName = "Incremental/Single Upgrade Data")]
public class UpgradeSO : ScriptableObject
{
    [Header("Basic Info")]
    public string upgradeId;
    public string upgradeTitle;
    [TextArea(2, 5)] public string description;
    public Sprite icon;
    public UpgradeType upgradeType;

    [Header("Progression Chain")]
    public UpgradeSO prerequisiteUpgrade;

    [Header("Cost & Stat Impact")]
    public int cost = 10;
    public float effectValue = 1f;
}