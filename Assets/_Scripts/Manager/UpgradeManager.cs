using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    #region [References]
    [Header("Upgrades Library")]
    [SerializeField] private List<UpgradeSO> allUpgrades;
    #endregion

    #region [Attributes]
    private HashSet<string> purchasedUpgradeIds = new HashSet<string>();
    #endregion

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

    #region [Methods]
    public bool IsUpgradePurchased(UpgradeSO upgrade)
    {
        if (upgrade == null) return false;
        return purchasedUpgradeIds.Contains(upgrade.upgradeId);
    }

    public bool IsPrerequisiteMet(UpgradeSO upgrade)
    {
        if (upgrade == null) return false;
        // If there's no prerequisite, it's the start of a chain and is immediately available
        if (upgrade.prerequisiteUpgrade == null) return true;

        return IsUpgradePurchased(upgrade.prerequisiteUpgrade);
    }

    public bool TryBuyUpgrade(UpgradeSO upgrade)
    {
        if (upgrade == null) return false;

        // Cannot buy if already owned
        if (IsUpgradePurchased(upgrade)) return false;

        // Cannot buy if previous item hasn't been bought yet
        if (!IsPrerequisiteMet(upgrade)) return false;

        // Try spend currency
        if (CurrencyManager.Instance != null && CurrencyManager.Instance.TrySpendCurrency(upgrade.cost))
        {
            purchasedUpgradeIds.Add(upgrade.upgradeId);
            GameEventHandler.OnUpgradePurchased?.Invoke(upgrade, 1);
            return true;
        }

        return false;
    }

    // --- Dynamic Stat Getters ---

    public float GetMagnetRadius()
    {
        float baseValue = 2.5f; // Default base radius
        foreach (UpgradeSO u in allUpgrades)
        {
            if (u != null && u.upgradeType == UpgradeType.MagnetRadius && IsUpgradePurchased(u))
            {
                baseValue += u.effectValue;
            }
        }
        return baseValue;
    }

    public int GetBurstSpawnCount()
    {
        int baseValue = 1; // Default base count
        foreach (UpgradeSO u in allUpgrades)
        {
            if (u != null && u.upgradeType == UpgradeType.BurstSpawnCount && IsUpgradePurchased(u))
            {
                baseValue += Mathf.RoundToInt(u.effectValue);
            }
        }
        return baseValue;
    }

    public float GetSpawnInterval()
    {
        float baseInterval = 3.0f; // Default base interval in seconds
        foreach (UpgradeSO u in allUpgrades)
        {
            if (u != null && u.upgradeType == UpgradeType.SpawnInterval && IsUpgradePurchased(u))
            {
                baseInterval -= u.effectValue;
            }
        }
        return Mathf.Max(0.4f, baseInterval); // Cap minimum spawn rate
    }

    public float GetCoinMultiplier()
    {
        float baseMultiplier = 1.0f;
        foreach (UpgradeSO u in allUpgrades)
        {
            if (u != null && u.upgradeType == UpgradeType.CoinMultiplier && IsUpgradePurchased(u))
            {
                baseMultiplier += u.effectValue;
            }
        }
        return baseMultiplier;
    }
    #endregion
}