using Unity.VisualScripting;
using UnityEngine;

public class CampfireController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<LeafController>(out LeafController leaf))
        {
            leaf.OnCollected();
        }
    }
}
