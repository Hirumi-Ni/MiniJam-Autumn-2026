using UnityEngine;

// Shows a pooled "+N" text wherever coins are earned
public class FloatingTextSpawner : MonoBehaviour
{
    [Tooltip("Prefab with a TMP_Text (3D) and a FloatingText component")]
    [SerializeField] private GameObject floatingTextPrefab;

    private void OnEnable()
    {
        GameEventHandler.OnCoinsEarned += ShowText;
    }

    private void OnDisable()
    {
        GameEventHandler.OnCoinsEarned -= ShowText;
    }

    private void ShowText(int amount, Vector3 position)
    {
        if (floatingTextPrefab == null || PoolManager.Instance == null) return;

        GameObject instance = PoolManager.Instance.Spawn(floatingTextPrefab, position, Quaternion.identity);
        if (instance != null && instance.TryGetComponent(out FloatingText text))
        {
            text.Show(amount, position);
        }
    }
}
