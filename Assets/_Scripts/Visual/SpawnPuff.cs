using DG.Tweening;
using UnityEngine;

// Prefab: SpriteRenderer with this script. Pooled by LeafSpawnerManager.
[RequireComponent(typeof(SpriteRenderer))]
public class SpawnPuff : MonoBehaviour
{
    [Header("Sprite Sheet")]
    [Tooltip("Slice the sprite sheet (Sprite Mode: Multiple), select the frames, and drag them here in order")]
    [SerializeField] private Sprite[] frames;

    [Header("Motion")]
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private float startScale = 0.6f;
    [SerializeField] private float endScale = 1.2f;
    [SerializeField] private Ease scaleEase = Ease.OutQuad;
    [SerializeField] private float fadeOutDuration = 0.15f;

    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;
    private Color baseColor;
    private Sequence sequence;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        baseColor = spriteRenderer.color;
    }

    private void OnDisable()
    {
        KillTween();
    }

    public void Play()
    {
        KillTween();

        spriteRenderer.color = baseColor;
        transform.localScale = baseScale * startScale;
        if (frames != null && frames.Length > 0) spriteRenderer.sprite = frames[0];

        sequence = DOTween.Sequence().SetLink(gameObject);

        // Step through the sprite sheet frames over the duration
        if (frames != null && frames.Length > 1)
        {
            sequence.Join(DOVirtual.Int(0, frames.Length - 1, duration, index => spriteRenderer.sprite = frames[index]).SetEase(Ease.Linear));
        }

        sequence.Join(transform.DOScale(baseScale * endScale, duration).SetEase(scaleEase));
        sequence.Insert(duration - fadeOutDuration, spriteRenderer.DOFade(0f, fadeOutDuration));
        sequence.OnComplete(() => PooledObject.Return(gameObject));
    }

    private void KillTween()
    {
        sequence?.Kill();
        sequence = null;
        spriteRenderer.DOKill();
        transform.DOKill();
    }
}
