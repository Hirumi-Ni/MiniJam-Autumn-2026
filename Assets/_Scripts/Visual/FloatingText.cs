using DG.Tweening;
using TMPro;
using UnityEngine;

// Prefab: TextMeshPro (3D) text with this script. Pooled by FloatingTextSpawner.
[RequireComponent(typeof(TMP_Text))]
public class FloatingText : MonoBehaviour
{
    [SerializeField] private Vector3 startOffset = new Vector3(0f, 0.3f, 0f);
    [SerializeField] private float riseDistance = 1f;
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private float popScale = 1.3f;
    [SerializeField] private Ease riseEase = Ease.OutCubic;

    private TMP_Text label;
    private Sequence sequence;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    private void OnDisable()
    {
        sequence?.Kill();
        sequence = null;
    }

    public void Show(int amount, Vector3 worldPosition)
    {
        sequence?.Kill();

        label.text = "+" + amount;
        Color c = label.color;
        label.color = new Color(c.r, c.g, c.b, 1f);

        transform.position = worldPosition + startOffset;
        transform.localScale = Vector3.one * 0.5f;

        sequence = DOTween.Sequence().SetLink(gameObject);

        // Pop in, then settle
        sequence.Append(transform.DOScale(Vector3.one * popScale, 0.12f).SetEase(Ease.OutBack));
        sequence.Append(transform.DOScale(Vector3.one, 0.1f));

        // Float up for the whole duration, and fade out during the second half
        sequence.Insert(0f, transform.DOMoveY(transform.position.y + riseDistance, duration).SetEase(riseEase));
        sequence.Insert(duration * 0.5f, DOTween.ToAlpha(() => label.color, a => label.color = a, 0f, duration * 0.5f));

        sequence.OnComplete(() => PooledObject.Return(gameObject));
    }
}
