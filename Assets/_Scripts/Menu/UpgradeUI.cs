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

    [Header("Hidden State")]
    [SerializeField] private Sprite questionMarkIcon;
    [SerializeField] private string hiddenTitle = "???";
    [SerializeField] private string hiddenDescription = "Buy the previous upgrade to reveal.";
    [SerializeField] private string hiddenCost = "???";

    [Header("Owned State")]
    [SerializeField] private string ownedLabel = "Owned";
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
        if (buyButton != null) buyButton.onClick.AddListener(OnBuyButtonClicked);
        RefreshUI();
    }
    #endregion

    #region [Methods]
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

        UpgradeState state = UpgradeManager.Instance.GetState(upgradeData);
        int currency = currentCurrency ?? (CurrencyManager.Instance != null ? CurrencyManager.Instance.TotalCurrency : 0);

        switch (state)
        {
            case UpgradeState.Hidden:
                SetText(titleText, hiddenTitle);
                SetText(descriptionText, hiddenDescription);
                SetText(costText, hiddenCost);
                if (iconImage != null && questionMarkIcon != null) iconImage.sprite = questionMarkIcon;
                SetButton(false);
                break;

            case UpgradeState.Available:
                ShowFullInfo();
                SetText(costText, $"${upgradeData.cost}");
                SetButton(currency >= upgradeData.cost);
                break;

            case UpgradeState.Owned:
                ShowFullInfo();
                SetText(costText, ownedLabel);
                SetButton(false);
                break;
        }
    }

    private void ShowFullInfo()
    {
        SetText(titleText, upgradeData.upgradeTitle);
        SetText(descriptionText, upgradeData.description);
        if (iconImage != null && upgradeData.icon != null) iconImage.sprite = upgradeData.icon;
    }

    private void SetText(TMP_Text target, string value)
    {
        if (target != null) target.text = value;
    }

    private void SetButton(bool interactable)
    {
        if (buyButton != null) buyButton.interactable = interactable;
    }
    #endregion
}