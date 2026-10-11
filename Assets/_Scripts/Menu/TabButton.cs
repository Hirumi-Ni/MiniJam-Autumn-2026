using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Put on each tab button. Place the tab in the editor where it sticks out when selected.
// When deselected it slides by tuckOffset, goes behind the panels, and gets darker.
// Hovering sticks it out again. Don't put tabs inside a LayoutGroup, because the layout would ignore these positions.
public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tuck")]
    [SerializeField] private Vector2 tuckOffset = new Vector2(60f, 0f);

    [Header("Color")]
    [SerializeField] private Image tabImage;
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color deselectedColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    [Header("Timing")]
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private Ease ease = Ease.OutBack;

    private RectTransform rect;
    private Vector2 stickOutPosition;
    private bool selected;
    private bool hovered;

    private bool StickOut => selected || hovered;

    private void Awake()
    {
        rect = (RectTransform)transform;
        stickOutPosition = rect.anchoredPosition;

        if (tabImage == null) tabImage = GetComponent<Image>();
    }

    public void SetSelected(bool isSelected, bool instant = false)
    {
        selected = isSelected;
        Refresh(instant);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        Refresh(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
        Refresh(false);
    }

    private void Refresh(bool instant)
    {
        if (StickOut) rect.SetAsLastSibling();
        else rect.SetAsFirstSibling();

        Vector2 targetPosition = StickOut ? stickOutPosition : stickOutPosition + tuckOffset;
        Color targetColor = (selected || hovered) ? selectedColor : deselectedColor;

        rect.DOKill();
        DOTween.Kill(this);

        if (instant)
        {
            rect.anchoredPosition = targetPosition;
            if (tabImage != null) tabImage.color = targetColor;
            return;
        }

        rect.DOAnchorPos(targetPosition, duration).SetEase(ease).SetLink(gameObject);

        if (tabImage != null)
        {
            DOTween.To(
                () => tabImage.color,
                c => tabImage.color = c,
                targetColor,
                duration
            )
            .SetEase(Ease.OutQuad)
            .SetTarget(this)
            .SetLink(gameObject);
        }
    }
}