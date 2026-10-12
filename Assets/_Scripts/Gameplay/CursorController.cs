using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorController : MonoBehaviour
{
    #region [References]
    [Header("Drag Radius")]
    [SerializeField] private float baseDragRadius = 1f;
    [SerializeField] private LayerMask leafLayer;

    [Header("Follow Feel")]
    [Tooltip("How quickly grabbed leaves catch up to the cursor. Higher = snappier.")]
    [SerializeField] private float followSharpness = 20f;
    [Tooltip("Desired leaf speed per unit of distance to the cursor, before clamping.")]
    [SerializeField] private float followGain = 10f;
    [SerializeField] private float maxLeafSpeed = 14f;
    [Tooltip("Fraction of a leaf's velocity kept when you let go")]
    [Range(0f, 1f)][SerializeField] private float releaseVelocityKeep = 0.5f;

    [Header("Gizmos")]
    [SerializeField] private LineRenderer radiusVisualizer;
    [SerializeField] private int circleSegments = 40;
    #endregion

    #region [Properties]
    public float MagnetRadius => UpgradeManager.Instance != null
        ? UpgradeManager.Instance.GetStat(StatType.DragRadius, baseDragRadius)
        : baseDragRadius;
    #endregion

    #region [Attributes]
    private Camera mainCamera;
    private Vector2 pointerWorldPos;
    private bool isDragging;

    // Leaves caught during this drag. They stay attached until release,
    // even when they're outside the radius.
    private readonly Dictionary<LeafController, Rigidbody2D> grabbedLeaves = new Dictionary<LeafController, Rigidbody2D>();
    private readonly List<LeafController> invalidLeaves = new List<LeafController>();
    private readonly Collider2D[] overlapBuffer = new Collider2D[64];
    #endregion

    #region [Unity Lifecycle]
    private void Awake()
    {
        mainCamera = Camera.main;
        if (radiusVisualizer != null) radiusVisualizer.enabled = false;
    }

    private void Update()
    {
        if (Pointer.current == null || mainCamera == null) return;

        Vector2 screenPosition = Pointer.current.position.ReadValue();
        Vector3 screenPosition3 = new Vector3(screenPosition.x, screenPosition.y, -mainCamera.transform.position.z);
        pointerWorldPos = mainCamera.ScreenToWorldPoint(screenPosition3);

        if (Pointer.current.press.wasPressedThisFrame) StartDrag();
        else if (Pointer.current.press.wasReleasedThisFrame) EndDrag();

        if (isDragging) DrawRadiusCircle();
    }

    private void FixedUpdate()
    {
        if (!isDragging) return;

        GrabLeavesInRadius();
        FollowGrabbedLeaves();
    }
    #endregion

    #region [Drag Flow]
    private void StartDrag()
    {
        isDragging = true;
        if (radiusVisualizer != null) radiusVisualizer.enabled = true;
    }

    private void EndDrag()
    {
        isDragging = false;
        if (radiusVisualizer != null) radiusVisualizer.enabled = false;
        foreach (KeyValuePair<LeafController, Rigidbody2D> pair in grabbedLeaves)
        {
            if (pair.Value != null) pair.Value.linearVelocity *= releaseVelocityKeep;
        }
        grabbedLeaves.Clear();
    }

    private void GrabLeavesInRadius()
    {
        int count = Physics2D.OverlapCircleNonAlloc(pointerWorldPos, MagnetRadius, overlapBuffer, leafLayer);

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = overlapBuffer[i];
            if (!hit.TryGetComponent(out LeafController leaf)) continue;
            if (!leaf.CanBeDragged || grabbedLeaves.ContainsKey(leaf)) continue;

            Rigidbody2D rb = hit.attachedRigidbody;
            if (rb == null) continue;

            grabbedLeaves.Add(leaf, rb);
        }
    }

    private void FollowGrabbedLeaves()
    {
        // Frame-rate independent smoothing factor
        float t = 1f - Mathf.Exp(-followSharpness * Time.fixedDeltaTime);

        invalidLeaves.Clear();

        foreach (KeyValuePair<LeafController, Rigidbody2D> pair in grabbedLeaves)
        {
            LeafController leaf = pair.Key;
            Rigidbody2D rb = pair.Value;

            // Collected, decayed, or destroyed leaves get dropped from the grab list
            if (leaf == null || rb == null || !leaf.CanBeDragged)
            {
                invalidLeaves.Add(leaf);
                continue;
            }

            Vector2 toPointer = pointerWorldPos - rb.position;
            Vector2 desiredVelocity = Vector2.ClampMagnitude(toPointer * followGain, maxLeafSpeed);

            // Set velocity directly so the leaf keeps up with the pointer no matter how far away it is
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, desiredVelocity, t);

            // Tumble with sideways movement, like the original torque did
            rb.AddTorque(-rb.linearVelocity.x, ForceMode2D.Force);
        }

        foreach (LeafController leaf in invalidLeaves)
        {
            grabbedLeaves.Remove(leaf);
        }
    }
    #endregion

    #region [Gizmos]
    private void DrawRadiusCircle()
    {
        if (radiusVisualizer == null) return;

        float radius = MagnetRadius;
        radiusVisualizer.positionCount = circleSegments + 1;
        radiusVisualizer.useWorldSpace = true;

        float deltaAngle = (2f * Mathf.PI) / circleSegments;
        for (int i = 0; i <= circleSegments; i++)
        {
            float angle = i * deltaAngle;
            float x = pointerWorldPos.x + Mathf.Cos(angle) * radius;
            float y = pointerWorldPos.y + Mathf.Sin(angle) * radius;
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