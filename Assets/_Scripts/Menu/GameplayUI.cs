using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameplayUI : MonoBehaviour
{
    #region [References]
    [SerializeField] private TMP_Text currencyCounter;
    [SerializeField] private GameObject pausePanelArea;

    [Header("Count Tween")]
    [SerializeField] private float countDuration = 0.25f;
    [SerializeField] private Ease countEase = Ease.Linear;

    [Header("Pop Feedback")]
    [SerializeField] private float popStrength = 0.2f;
    [SerializeField] private float popDuration = 0.2f;
    #endregion

    #region [Attributes]
    private float displayedValue;
    private Transform counterTransform;
    #endregion

    #region [Unity Lifecycle]
    private void Awake()
    {
        counterTransform = currencyCounter.transform;
        Time.timeScale = 1f;
    }

    private void OnEnable()
    {
        GameEventHandler.OnCurrencyChange += OnCurrencyChanged;
    }

    private void OnDisable()
    {
        GameEventHandler.OnCurrencyChange -= OnCurrencyChanged;
        DOTween.Kill(this);
        counterTransform.DOKill();
    }

    private void Start()
    {
        displayedValue = 0f;
        currencyCounter.text = "0";
        pausePanelArea.SetActive(false);
    }
    #endregion

    #region [Methods]
    private void OnCurrencyChanged(int newTotal)
    {
        CountTo(newTotal);
        PlayPop();
    }

    private void CountTo(int newTotal)
    {
        DOTween.Kill(this);
        DOTween.To(() => displayedValue, value => { displayedValue = value; UpdateText(); }, newTotal, countDuration).SetTarget(this).SetEase(countEase).SetLink(gameObject);
    }

    private void UpdateText()
    {
        currencyCounter.text = Mathf.RoundToInt(displayedValue).ToString();
    }

    private void PlayPop()
    {
        counterTransform.DOKill(true);
        counterTransform.DOPunchScale(Vector3.one * popStrength, popDuration, 6, 0.6f).SetLink(gameObject);
    }

    public void OpenPauseMenu()
    {
        Time.timeScale = 0f;
        pausePanelArea.SetActive(true);
        AudioManager.Instance.PlayAudio(AudioManager.Instance.SFX_UIPopOpen);
    }

    public void ClosePauseMenu()
    {
        Time.timeScale = 1f;
        pausePanelArea.SetActive(false);
        AudioManager.Instance.PlayAudio(AudioManager.Instance.SFX_UIPopClose);
    }
    #endregion
}