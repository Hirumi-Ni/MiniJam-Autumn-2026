using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorController : MonoBehaviour
{
    #region [References]
    [SerializeField] private float pullForce = 25f;
    [SerializeField] private float maxLeafSpeed = 14f;
    [SerializeField] private LayerMask leafLayer;

    [Header("Drag Radius")]
    [SerializeField] private float baseDragRadius = 1f;

    [Header("Follow Feel")]
    [SerializeField] private float followSharpness = 20f;
    [SerializeField] private float followGain = 10f;
    [Range(0f, 1f)][SerializeField] private float releaseVelocityKeep = 0.5f;

    [Header("Gizmos")]
    [SerializeField] private LineRenderer radiusVisualizer;
    [SerializeField] private int circleSegments = 40;
    #endregion

    #region [Properties]
    public float MagnetRadius => UpgradeManager.Instance != null ? UpgradeManager.Instance.GetStat(StatType.DragRadius, baseDragRadius) : baseDragRadius;
    #endregion

    #region [Attributes]
    private Camera mainCamera;
    private Vector2 mouseWorldPos;
    private bool isDragging;

    private Dictionary<LeafController, Rigidbody2D> grabbedLeaves = new Dictionary<LeafController, Rigidbody2D>();
    private List<LeafController> invalidLeaves = new List<LeafController>();
    private List<Collider2D> overlapResults = new List<Collider2D>(64);
    private ContactFilter2D leafContactFilter;
    #endregion

    #region [Unity Lifecycle]
    private void Awake()
    {
        mainCamera = Camera.main;
        if (radiusVisualizer != null) radiusVisualizer.enabled = false;

        leafContactFilter = new ContactFilter2D();
        leafContactFilter.SetLayerMask(leafLayer);
        leafContactFilter.useTriggers = true;
    }

    private void Update()
    {
        if (Mouse.current == null || mainCamera == null) return;

        Vector2 screen = Mouse.current.position.ReadValue();
        Vector3 screenPos3 = new Vector3(screen.x, screen.y, -mainCamera.transform.position.z);
        mouseWorldPos = mainCamera.ScreenToWorldPoint(screenPos3);

        if (Mouse.current.leftButton.wasPressedThisFrame) StartDrag();
        else if (Mouse.current.leftButton.wasReleasedThisFrame) EndDrag();

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
        overlapResults.Clear();

        Physics2D.OverlapCircle(mouseWorldPos, MagnetRadius, leafContactFilter, overlapResults);

        for (int i = 0; i < overlapResults.Count; i++)
        {
            Collider2D hit = overlapResults[i];

            if (!hit.TryGetComponent(out LeafController leaf)) continue;
            if (!leaf.CanBeDragged || grabbedLeaves.ContainsKey(leaf)) continue;

            Rigidbody2D rb = hit.attachedRigidbody;

            if (rb == null) continue;
            grabbedLeaves.Add(leaf, rb);

            AudioManager.Instance.PlayAudio(AudioManager.Instance.SFX_LeafMoving);

        }
    }

    private void FollowGrabbedLeaves()
    {
        float t = 1f - Mathf.Exp(-followSharpness * Time.fixedDeltaTime);

        invalidLeaves.Clear();

        foreach (KeyValuePair<LeafController, Rigidbody2D> pair in grabbedLeaves)
        {
            LeafController leaf = pair.Key;
            Rigidbody2D rb = pair.Value;

            if (leaf == null || rb == null || !leaf.CanBeDragged)
            {
                invalidLeaves.Add(leaf);
                continue;
            }

            Vector2 toCursor = mouseWorldPos - rb.position;
            Vector2 desiredVelocity = Vector2.ClampMagnitude(toCursor * followGain, maxLeafSpeed);

            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, desiredVelocity, t);
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
            float x = mouseWorldPos.x + Mathf.Cos(angle) * radius;
            float y = mouseWorldPos.y + Mathf.Sin(angle) * radius;
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