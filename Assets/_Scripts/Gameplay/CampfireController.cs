using DG.Tweening;
using UnityEngine;

public class CampfireController : MonoBehaviour
{
    [Header("Punch")]
    [SerializeField] private Transform punchTarget;
    [SerializeField] private float hitPunch = 0.05f;
    [SerializeField] private float hitDuration = 0.15f;
    [SerializeField] private float collectPunch = 0.15f;
    [SerializeField] private float collectDuration = 0.25f;

    private void Awake()
    {
        if (punchTarget == null) punchTarget = transform;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.TryGetComponent(out LeafController leaf)) return;

        if (leaf.IsLanded && !leaf.IsCollected)
        {
            leaf.OnCollected();
            Punch(collectPunch, collectDuration);
        }
        else
        {
            Punch(hitPunch, hitDuration);
        }
    }

    private void Punch(float strength, float duration)
    {
        punchTarget.DOKill(true);
        punchTarget.DOPunchScale(Vector3.one * strength, duration, 6, 0.5f).SetLink(gameObject);
    }
}
