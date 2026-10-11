using DG.Tweening;
using UnityEngine;

public class AutoReturnToPool : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;

    private Tween returnTween;

    private void OnEnable()
    {
        returnTween?.Kill();
        returnTween = DOVirtual.DelayedCall(lifetime, () => PooledObject.Return(gameObject)).SetTarget(this).SetLink(gameObject);
    }

    private void OnDisable()
    {
        returnTween?.Kill();
        returnTween = null;
    }
}