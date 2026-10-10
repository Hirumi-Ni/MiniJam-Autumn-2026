using UnityEngine;
using UnityEngine.InputSystem;

public class CursorController : MonoBehaviour
{
    #region [References]
    [SerializeField] private float pullForce = 25f; 
    [SerializeField] private float maxLeafSpeed = 12f;
    [SerializeField] private LayerMask leafLayer;
    [field: SerializeField] public float MagnetRadius { get; private set; } = 3.0f;

    [Header("Gizmos")]
    [SerializeField] private LineRenderer radiusVisualizer;
    [SerializeField] private int circleSegments = 40;
    #endregion

    #region [Attribute]
    private Camera mainCamera;
    private Vector2 mouseWorldPos;
    private bool isDragging = false;
    #endregion

    #region [Unity Lifecycle]
    private void Awake()
    {
        mainCamera = Camera.main;
        if (radiusVisualizer != null) radiusVisualizer.enabled = true;
    }

    private void Update()
    {
        if (Mouse.current == null) return;
        Vector2 screenPosition = Mouse.current.position.ReadValue();
        Vector3 mouseScreenPosition = new Vector3(screenPosition.x, screenPosition.y, -mainCamera.transform.position.z);
        mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isDragging = true;
            if (radiusVisualizer != null) radiusVisualizer.enabled = true;
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
            if (radiusVisualizer != null) radiusVisualizer.enabled = false;
        }

        if (isDragging) DrawRadiusCircle();
    }

    private void FixedUpdate()
    {
        if (!isDragging) return;

        Collider2D[] leavesInRadius = Physics2D.OverlapCircleAll(mouseWorldPos, MagnetRadius, leafLayer);
        foreach (Collider2D leafCollider in leavesInRadius)
        {
            Rigidbody2D rigidbody = leafCollider.attachedRigidbody;
            if (rigidbody == null) continue;

            LeafController leafController = leafCollider.GetComponent<LeafController>();
            if (leafController != null && leafController.IsDecaying) continue;

            Vector2 pullDirection = mouseWorldPos - rigidbody.position;
            float distance = pullDirection.magnitude;

            if (distance > 0.05f)
            {
                float targetSpeed = Mathf.Min(distance * 6f, maxLeafSpeed);
                Vector2 desiredVelocity = pullDirection.normalized * targetSpeed;
                Vector2 steeringForce = (desiredVelocity - rigidbody.linearVelocity) * pullForce;
                rigidbody.AddForce(steeringForce, ForceMode2D.Force);
                rigidbody.AddTorque(-rigidbody.linearVelocity.x * 1.0f, ForceMode2D.Force);
            }
        }
    }
    #endregion

    #region [Gizmos]
    private void DrawRadiusCircle()
    {
        if (radiusVisualizer == null) return;

        radiusVisualizer.positionCount = circleSegments + 1;
        radiusVisualizer.useWorldSpace = true;
        float deltaAngle = (2f * Mathf.PI) / circleSegments;
        for (int i = 0; i <= circleSegments; i++)
        {
            float angle = i * deltaAngle;
            float x = mouseWorldPos.x + Mathf.Cos(angle) * MagnetRadius;
            float y = mouseWorldPos.y + Mathf.Sin(angle) * MagnetRadius;
            radiusVisualizer.SetPosition(i, new Vector3(x, y, 0f));
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, MagnetRadius);
    }
    #endregion
}