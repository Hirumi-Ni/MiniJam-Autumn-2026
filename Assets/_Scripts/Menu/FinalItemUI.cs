using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The last item in the game. Separate from UpgradeUI, with no ScriptableObject.
// - Cost is always visible.
// - Until the player can afford it, the title and description are hidden and the icon shows a question mark.
// - Buying it only needs enough coins. Then it fades to black and loads the ending scene.
public class FinalItemUI : MonoBehaviour
{
    private const int FinalCost = 50000;

    [Header("Item Info")]
    [SerializeField] private string itemTitle = "The Final Offering";
    [TextArea(2, 4)][SerializeField] private string itemDescription = "Rekindle the fire. The story ends here.";
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
    [SerializeField] private string hiddenDescription = "";

    [Header("Reveal Animation")]
    [SerializeField] private float revealDuration = 0.3f;

    [Header("Ending")]
    [Tooltip("Full-screen black CanvasGroup, alpha 0, used for the fade out")]
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

        buyButton.enabled = false;

        // Cost is always shown, so set it once
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

    private void RevealInfo()
    {
        if (revealed) return;
        buyButton.enabled = true;
        revealed = true;

        SetText(titleText, itemTitle);
        SetText(descriptionText, itemDescription);

        if (iconImage != null)
        {
            iconImage.sprite = itemIcon;
            iconImage.enabled = true;
            iconImage.transform.DOKill();
            iconImage.transform.localScale = Vector3.zero;
            iconImage.transform.DOScale(Vector3.one, revealDuration).SetEase(Ease.OutBack).SetLink(gameObject);
        }

        if (titleText != null)
        {
            titleText.transform.DOKill(true);
            titleText.transform.DOPunchScale(Vector3.one * 0.15f, revealDuration, 5, 0.5f).SetLink(gameObject);
        }
    }

    private void HideInfo()
    {
        revealed = false;

        SetText(titleText, hiddenTitle);
        SetText(descriptionText, hiddenDescription);

        // Keep the icon slot, but show the question mark instead of the real icon
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
        if (buyButton != null) buyButton.interactable = false;
        SetText(costText, "Bought");

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