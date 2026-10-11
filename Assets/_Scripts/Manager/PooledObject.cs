using UnityEngine;
using UnityEngine.Pool;

public class PooledObject : MonoBehaviour
{
    public ObjectPool<GameObject> Pool { get; set; }

    public void ReturnToPool()
    {
        if (!gameObject.activeSelf) return;

        if (Pool != null) Pool.Release(gameObject);
        else SafeDestroy(gameObject);
    }

    public static void Return(GameObject go)
    {
        if (go == null) return;

        if (go.TryGetComponent(out PooledObject member)) member.ReturnToPool();
        else SafeDestroy(go);
    }

    public static void SafeDestroy(GameObject go)
    {
        if (go == null) return;

        if (Application.isPlaying) Object.Destroy(go);
        else Object.DestroyImmediate(go);
    }
}