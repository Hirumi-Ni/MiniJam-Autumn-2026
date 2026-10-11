using Unity.VisualScripting;
using UnityEngine;

public class CampfireController : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent<LeafController>(out LeafController leaf))
        {
            if (!leaf.IsLanded) return;
            leaf.OnCollected();
        }
    }
}
