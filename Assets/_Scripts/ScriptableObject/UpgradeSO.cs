using System;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    DragRadius,
    LeafCoinValue,
    SpawnRate,
    BurstCount
}

public enum EffectMode
{
    Add,   
    Multiply   
}

[Serializable]
public class UpgradeEffect
{
    public StatType stat;
    public EffectMode mode = EffectMode.Add;
    public float value = 1f;
}

[CreateAssetMenu(fileName = "NewUpgradeSO", menuName = "Incremental/Upgrade Data")]
public class UpgradeSO : ScriptableObject
{
    [Header("Basic Info")]
    public string upgradeTitle;
    [TextArea(2, 5)] public string description;
    public Sprite icon;
    public UpgradeSO prerequisiteUpgrade;

    [Header("Cost")]
    public int cost = 10;

    [Header("Effects")]
    public List<UpgradeEffect> effects = new List<UpgradeEffect>();
}