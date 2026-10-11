using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHoverFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale")]
    [SerializeField] private float hoverScale = 1.05f;

    [Header("Tilt")]
    [SerializeField] private float hoverTiltZ = -3f;


    [Header("Timing")]
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private Ease ease = Ease.OutBack;

    private float baseHeight;

    private void OnDisable()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
    }

    public void OnPointerEnter(PointerEventData eventData) => Apply(true);

    public void OnPointerExit(PointerEventData eventData) => Apply(false);

    private void Apply(bool hovered)
    {
        float scale = hovered ? hoverScale : 1f;
        float tilt = hovered ? hoverTiltZ : 0f;

        transform.DOKill();
        transform.DOScale(Vector3.one * scale, duration).SetEase(ease).SetLink(gameObject);
        transform.DORotate(new Vector3(0f, 0f, tilt), duration).SetEase(ease).SetLink(gameObject);
        AudioManager.Instance.PlayAudio(AudioManager.Instance.SFX_UIPopHover);
    }
}
