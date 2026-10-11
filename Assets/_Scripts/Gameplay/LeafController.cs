using DG.Tweening;
using UnityEngine;

public class LeafController : MonoBehaviour
{
    #region [Static]
    public static int ActiveCount { get; private set; }
    #endregion

    #region [References]
    [Header("Visual Children")]
    [SerializeField] private SpriteRenderer leafRenderer;
    [SerializeField] private SpriteRenderer shadowRenderer;

    [Header("Data")]
    [SerializeField] private LeafSO leafData;
    [SerializeField] private int leafCoinAmount = 1;

    [Header("Lifetime")]
    [SerializeField] private float leafLifetimeMin = 5f;
    [SerializeField] private float leafLifetimeMax = 10f;

    [Header("Intro Sequence")]
    [SerializeField] private float shadowAppearDuration = 0.25f;
    [SerializeField] private float leafSpawnDelay = 0.2f;

    [Header("Fall")]
    [SerializeField] private float fallStartScale = 2.5f;
    [SerializeField] private float fallDuration = 1.2f;
    [Range(0f, 0.5f)][SerializeField] private float fallDurationVariance = 0.15f;
    [SerializeField] private float leafFadeInDuration = 0.2f;
    [SerializeField] private Ease fallEase = Ease.InQuad;

    [Header("Shadow")]
    [SerializeField] private float shadowStartScale = 1.4f;
    [SerializeField] private float shadowLandedScale = 0.7f;
    [Range(0f, 1f)][SerializeField] private float shadowStartAlpha = 0.25f;
    [Range(0f, 1f)][SerializeField] private float shadowLandedAlpha = 0.7f;

    [Header("Landing Feedback")]
    [SerializeField] private float landPunchStrength = 0.15f;
    [SerializeField] private float landPunchDuration = 0.25f;

    [Header("Collect Feedback")]
    [SerializeField] private float collectPopDuration = 0.15f;

    private Rigidbody2D rb;
    private Collider2D col;
    private Vector3 leafBaseScale;
    private Vector3 shadowBaseScale;
    private Color leafAuthoredColor;
    private Color shadowAuthoredColor;
    private float currentLifetime;
    private Sequence introSequence;
    #endregion

    #region [State]
    public bool IsLanded { get; private set; }
    public bool IsDecaying { get; private set; }
    public bool IsCollected { get; private set; }
    public bool IsHeld { get; set; }

    public bool CanBeDragged => IsLanded && !IsDecaying && !IsCollected;
    #endregion

    #region [Unity Lifecycle]
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        leafBaseScale = leafRenderer.transform.localScale;
        shadowBaseScale = shadowRenderer.transform.localScale;
        leafAuthoredColor = leafRenderer.color;
        shadowAuthoredColor = shadowRenderer.color;
    }

    private void OnEnable() => ActiveCount++;

    private void OnDisable()
    {
        ActiveCount--;
        KillTween();
    }

    private void LateUpdate()
    {
        Transform shadowTransform = shadowRenderer.transform;
        shadowTransform.rotation = Quaternion.identity;
        shadowTransform.position = (Vector2)transform.position;
    }

    private void Update()
    {
        if (IsHeld)
        {
            currentLifetime = leafLifetimeMax;
            return;
        }

        if (!CanBeDragged) return;

        if (rb.linearVelocity.magnitude > 0.1f)
        {
            currentLifetime = Random.Range(leafLifetimeMin, leafLifetimeMax);
            return;
        }

        currentLifetime -= Time.deltaTime;
        if (currentLifetime <= 0f) TriggerDecay();
    }
    #endregion

    #region [Methods]
    public void Spawn(LeafSO data)
    {
        ResetState();
        Initialize(data);
        PlayFallIntro();
    }

    public void Initialize(LeafSO data)
    {
        leafData = data;
        leafCoinAmount = data.coinAmount;
        if (data.leafSprite != null) leafRenderer.sprite = data.leafSprite;
    }

    private void ResetState()
    {
        IsLanded = false;
        IsDecaying = false;
        IsCollected = false;
        IsHeld = false;

        currentLifetime = Random.Range(leafLifetimeMin, leafLifetimeMax);

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        col.enabled = false;
    }

    private void PlayFallIntro()
    {
        col.enabled = false;

        Transform leafT = leafRenderer.transform;
        Transform shadowT = shadowRenderer.transform;

        float duration = fallDuration * Random.Range(1f - fallDurationVariance, 1f + fallDurationVariance);
        float leafStartTime = shadowAppearDuration + leafSpawnDelay;

        Color shadowRgb = shadowAuthoredColor;
        Color shadowStart = new Color(shadowRgb.r, shadowRgb.g, shadowRgb.b, shadowStartAlpha);
        Color shadowLanded = new Color(shadowRgb.r, shadowRgb.g, shadowRgb.b, shadowLandedAlpha);

        shadowRenderer.color = new Color(shadowRgb.r, shadowRgb.g, shadowRgb.b, 0f);
        shadowT.localScale = shadowBaseScale * shadowStartScale;

        leafRenderer.color = new Color(leafAuthoredColor.r, leafAuthoredColor.g, leafAuthoredColor.b, 0f);
        leafT.localScale = leafBaseScale * fallStartScale;

        introSequence = DOTween.Sequence().SetLink(gameObject);

        introSequence.Append(shadowRenderer.DOColor(shadowStart, shadowAppearDuration));

        introSequence.Insert(leafStartTime, leafT.DOScale(leafBaseScale, duration).SetEase(fallEase));
        introSequence.Insert(leafStartTime, leafRenderer.DOFade(leafAuthoredColor.a, leafFadeInDuration));

        introSequence.Insert(leafStartTime, shadowT.DOScale(shadowBaseScale * shadowLandedScale, duration).SetEase(fallEase));
        introSequence.Insert(leafStartTime, shadowRenderer.DOColor(shadowLanded, duration));

        introSequence.AppendCallback(OnLanded);
    }

    private void OnLanded()
    {
        IsLanded = true;
        col.enabled = true;

        leafRenderer.transform.DOPunchScale(Vector3.one * -landPunchStrength, landPunchDuration, 6, 0.5f).SetLink(gameObject);

        AudioManager.Instance.PlayAudio(AudioManager.Instance.SFX_LeafPop);
    }

    private void TriggerDecay()
    {
        IsDecaying = true;

        shadowRenderer.DOFade(0f, 0.5f).SetLink(gameObject);
        leafRenderer.DOFade(0f, 0.5f).SetLink(gameObject).OnComplete(ReturnToPool);
    }

    public void OnCollected()
    {
        if (IsCollected) return;
        IsCollected = true;
        col.enabled = false;

        GameEventHandler.OnLeafCollected?.Invoke(leafCoinAmount, transform.position);
        KillTween();

        shadowRenderer.DOFade(0f, collectPopDuration).SetLink(gameObject);
        shadowRenderer.transform.DOScale(Vector3.zero, collectPopDuration).SetEase(Ease.InBack).SetLink(gameObject);
        leafRenderer.transform.DOScale(Vector3.zero, collectPopDuration).SetEase(Ease.InBack).SetLink(gameObject).OnComplete(ReturnToPool);

        AudioManager.Instance.PlayAudio(AudioManager.Instance.SFX_Fire);
        AudioManager.Instance.PlayAudio(AudioManager.Instance.SFX_LeafCollected);
    }

    private void ReturnToPool()
    {
        KillTween();
        PooledObject.Return(gameObject);
    }

    private void KillTween()
    {
        introSequence?.Kill();
        introSequence = null;
        leafRenderer.DOKill();
        shadowRenderer.DOKill();
        leafRenderer.transform.DOKill();
        shadowRenderer.transform.DOKill();
    }
    #endregion
}
