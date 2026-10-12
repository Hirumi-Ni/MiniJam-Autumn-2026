using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinalItemUI : MonoBehaviour
{
    [SerializeField] private int FinalCost = 10000;

    [Header("Item Info")]
    [SerializeField] private string itemTitle;
    [TextArea(2, 4)][SerializeField] private string itemDescription;
    [SerializeField] private Sprite itemIcon;

    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button buyButton;

    [Header("Hidden State")]
    [SerializeField] private Sprite questionMarkIcon;
    [SerializeField] private string hiddenTitle = "???";
    [SerializeField] private string hiddenDescription = "Get enought tokens to reveal.";

    [Header("Owned State")]
    [SerializeField] private string ownedLabel = "Bought";

    [Header("Reveal Animation")]
    [SerializeField] private float revealDuration = 0.3f;

    [Header("Ending")]
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeOutDuration = 1.5f;
    [Tooltip("Must be in Build Settings")]
    [SerializeField] private string endingSceneName = "Ending";

    private bool purchased;
    private bool revealed;

    private void OnEnable()
    {
        GameEventHandler.OnCurrencyChange += OnCurrencyChanged;
    }

    private void OnDisable()
    {
        GameEventHandler.OnCurrencyChange -= OnCurrencyChanged;
    }

    private void Start()
    {
        if (buyButton != null) buyButton.onClick.AddListener(TryBuy);

        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
        }

        SetText(costText, "$" + FinalCost);
        Refresh();
    }

    private void OnCurrencyChanged(int total) => Refresh(total);

    private bool CanAfford(int coins) => coins >= FinalCost;

    private void Refresh(int? currency = null)
    {
        if (purchased) return;

        int coins = currency ?? (CurrencyManager.Instance != null ? CurrencyManager.Instance.TotalCurrency : 0);

        if (CanAfford(coins)) RevealInfo();
        else HideInfo();

        if (buyButton != null) buyButton.interactable = CanAfford(coins);
    }

    // Shows the real title, description, and icon
    private void RevealInfo()
    {
        if (revealed) return;
        revealed = true;

        ShowFullInfo(animate: true);
    }

    private void ShowFullInfo(bool animate)
    {
        SetText(titleText, itemTitle);
        SetText(descriptionText, itemDescription);

        if (iconImage != null)
        {
            iconImage.sprite = itemIcon;
            iconImage.enabled = true;
            iconImage.transform.DOKill();

            if (animate)
            {
                iconImage.transform.localScale = Vector3.zero;
                iconImage.transform.DOScale(Vector3.one, revealDuration).SetEase(Ease.OutBack).SetLink(gameObject);
            }
            else
            {
                iconImage.transform.localScale = Vector3.one;
            }
        }

        if (animate && titleText != null)
        {
            titleText.transform.DOKill(true);
            titleText.transform.DOPunchScale(Vector3.one * 0.15f, revealDuration, 5, 0.5f).SetLink(gameObject);
        }
    }

    // Shows the question mark and hidden text
    private void HideInfo()
    {
        revealed = false;

        SetText(titleText, hiddenTitle);
        SetText(descriptionText, hiddenDescription);

        if (iconImage != null)
        {
            iconImage.transform.DOKill();
            iconImage.transform.localScale = Vector3.one;
            iconImage.sprite = questionMarkIcon;
            iconImage.enabled = questionMarkIcon != null;
        }
    }

    private void TryBuy()
    {
        if (purchased) return;
        if (CurrencyManager.Instance == null || !CurrencyManager.Instance.TrySpendCurrency(FinalCost)) return;

        purchased = true;

        revealed = false;
        ShowFullInfo(animate: true);
        SetText(costText, ownedLabel);

        if (buyButton != null) buyButton.interactable = false;

        FadeToEnding();
    }

    private void FadeToEnding()
    {
        if (fadeGroup == null)
        {
            LoadEnding();
            return;
        }

        fadeGroup.blocksRaycasts = true;

        DOTween.To(() => fadeGroup.alpha, a => fadeGroup.alpha = a, 1f, fadeOutDuration)
               .SetEase(Ease.InQuad)
               .SetLink(gameObject)
               .OnComplete(LoadEnding);
    }

    private void LoadEnding()
    {
        SceneManager.LoadScene(endingSceneName);
    }

    private void SetText(TMP_Text target, string value)
    {
        if (target != null) target.text = value;
    }
}