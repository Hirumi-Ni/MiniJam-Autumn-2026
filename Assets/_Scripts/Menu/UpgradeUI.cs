using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    #region [References]
    [Header("Data Config")]
    [SerializeField] private UpgradeSO upgradeData;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button buyButton;
    #endregion

    #region [Unity Lifecycle]
    private void OnEnable()
    {
        GameEventHandler.OnCurrencyChange += OnCurrencyUpdated;
        GameEventHandler.OnUpgradePurchased += OnUpgradePurchased;

        RefreshUI();
    }

    private void OnDisable()
    {
        GameEventHandler.OnCurrencyChange -= OnCurrencyUpdated;
        GameEventHandler.OnUpgradePurchased -= OnUpgradePurchased;
    }

    private void Start()
    {
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }

        SetupStaticDisplay();
        RefreshUI();
    }
    #endregion

    #region [Methods]
    private void SetupStaticDisplay()
    {
        if (upgradeData == null) return;

        if (titleText != null) titleText.text = upgradeData.upgradeTitle;
        if (descriptionText != null) descriptionText.text = upgradeData.description;
        if (iconImage != null && upgradeData.icon != null) iconImage.sprite = upgradeData.icon;
    }

    private void OnBuyButtonClicked()
    {
        if (UpgradeManager.Instance != null && upgradeData != null)
        {
            UpgradeManager.Instance.TryBuyUpgrade(upgradeData);
        }
    }

    private void OnCurrencyUpdated(int currentCurrency)
    {
        RefreshUI(currentCurrency);
    }

    private void OnUpgradePurchased(UpgradeSO upgrade, int level)
    {
        RefreshUI();
    }

    private void RefreshUI(int? currentCurrency = null)
    {
        if (upgradeData == null || UpgradeManager.Instance == null) return;

        bool isPurchased = UpgradeManager.Instance.IsUpgradePurchased(upgradeData);
        bool isPrerequisiteMet = UpgradeManager.Instance.IsPrerequisiteMet(upgradeData);
        int availableCurrency = currentCurrency ?? (CurrencyManager.Instance != null ? CurrencyManager.Instance.TotalCurrency : 0);

        if (isPurchased)
        {
            //if (costText != null) costText.text = "PURCHASED";
            if (buyButton != null) buyButton.interactable = false;
        }
        else if (!isPrerequisiteMet)
        {
            string reqName = upgradeData.prerequisiteUpgrade != null ? upgradeData.prerequisiteUpgrade.upgradeTitle : "Previous Item";
            //if (costText != null) costText.text = $"Locked (Requires: {reqName})";
            if (buyButton != null) buyButton.interactable = false;
        }
        else
        {
            if (costText != null) costText.text = $"${upgradeData.cost}";
            if (buyButton != null) buyButton.interactable = availableCurrency >= upgradeData.cost;
        }
    }
    #endregion
}