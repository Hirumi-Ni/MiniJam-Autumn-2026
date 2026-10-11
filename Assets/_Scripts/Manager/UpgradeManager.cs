using System.Collections.Generic;
using UnityEngine;

public enum UpgradeState
{
    Hidden,      
    Available,   
    Owned      
}

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    private HashSet<UpgradeSO> purchased = new HashSet<UpgradeSO>();
    private Dictionary<StatType, float> addBonus = new Dictionary<StatType, float>();
    private Dictionary<StatType, float> mulBonus = new Dictionary<StatType, float>();

    #region [Unity Lifecycle]
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion

    #region [Upgrade State]
    public bool IsUpgradePurchased(UpgradeSO upgrade)
    {
        return upgrade != null && purchased.Contains(upgrade);
    }

    public bool IsPrerequisiteMet(UpgradeSO upgrade)
    {
        if (upgrade == null) return false;
        return upgrade.prerequisiteUpgrade == null || IsUpgradePurchased(upgrade.prerequisiteUpgrade);
    }

    public UpgradeState GetState(UpgradeSO upgrade)
    {
        if (IsUpgradePurchased(upgrade)) return UpgradeState.Owned;
        if (IsPrerequisiteMet(upgrade)) return UpgradeState.Available;
        return UpgradeState.Hidden;
    }

    public bool TryBuyUpgrade(UpgradeSO upgrade)
    {
        if (upgrade == null) return false;
        if (GetState(upgrade) != UpgradeState.Available) return false;
        if (CurrencyManager.Instance == null) return false;
        if (!CurrencyManager.Instance.TrySpendCurrency(upgrade.cost)) return false;

        purchased.Add(upgrade);
        RebuildStats();
        GameEventHandler.OnUpgradePurchased?.Invoke(upgrade, 1);
        return true;
    }
    #endregion

    #region [Stats]
    public float GetStat(StatType stat, float baseValue)
    {
        float add = addBonus.TryGetValue(stat, out float a) ? a : 0f;
        float mul = mulBonus.TryGetValue(stat, out float m) ? m : 1f;
        return (baseValue + add) * mul;
    }

    private void RebuildStats()
    {
        addBonus.Clear();
        mulBonus.Clear();

        foreach (UpgradeSO upgrade in purchased)
        {
            foreach (UpgradeEffect effect in upgrade.effects)
            {
                if (effect.mode == EffectMode.Add)
                {
                    float current = addBonus.TryGetValue(effect.stat, out float a) ? a : 0f;
                    addBonus[effect.stat] = current + effect.value;
                }
                else
                {
                    float current = mulBonus.TryGetValue(effect.stat, out float m) ? m : 1f;
                    mulBonus[effect.stat] = current * effect.value;
                }
            }
        }
    }
    #endregion
}