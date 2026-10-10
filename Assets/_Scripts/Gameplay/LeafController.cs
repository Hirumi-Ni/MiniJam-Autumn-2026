using System;
using UnityEngine;
using DG.Tweening;

public class LeafController : MonoBehaviour
{
    #region [References]
    [SerializeField] private LeafSO leafData;
    [SerializeField] private int leafCoinAmount = 1;
    [SerializeField] private float leafLifetimeMin = 5f;
    [SerializeField] private float leafLifetimeMax = 10f;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidbodyComponent;
    #endregion

    #region [Attributes]
    private bool isDecay = false;
    public bool IsDecaying => isDecay;
    private float currentLifetime;
    #endregion

    #region [Unity Lifecycle]
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidbodyComponent = GetComponent<Rigidbody2D>();
        currentLifetime = UnityEngine.Random.Range(leafLifetimeMin, leafLifetimeMax);

        if (leafData != null)
        {
            Initialize(leafData);
        }
    }

    private void Update()
    {
        if (isDecay) return;

        if (rigidbodyComponent != null && rigidbodyComponent.linearVelocity.magnitude > 0.1f)
        {
            currentLifetime = UnityEngine.Random.Range(leafLifetimeMin, leafLifetimeMax);
        }
        else
        {
            currentLifetime -= Time.deltaTime;
            if (currentLifetime <= 0f)
            {
                TriggerDecay();
            }
        }
    }
    #endregion

    #region [Methods]
    public void Initialize(LeafSO data)
    {
        leafData = data;
        leafCoinAmount = data.coinAmount;
        if (spriteRenderer != null && data.leafSprite != null)
        {
            spriteRenderer.sprite = data.leafSprite;
        }
    }

    private void TriggerDecay()
    {
        if (isDecay) return;
        isDecay = true;
        spriteRenderer.DOFade(0f, 0.5f).OnComplete(() => Destroy(gameObject));
    }

    public void OnCollected()
    {
        GameEventHandler.OnLeafCollected?.Invoke(leafCoinAmount);
        transform.DOKill();
        if (spriteRenderer != null) spriteRenderer.DOKill();
        Destroy(gameObject);
    }
    #endregion
}