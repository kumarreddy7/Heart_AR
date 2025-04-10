using UnityEngine;
using TMPro;

[RequireComponent(typeof(LineRenderer))]
public class LabelFollow : MonoBehaviour
{
    [Header("Label Settings")]
    public Vector3 labelOffset = new Vector3(0.2f, 0.1f, 0.5f);
    public float labelScale = 0.005f;

    private Transform target;
    private Camera cam;

    private LineRenderer lineRenderer;
    private TextMeshPro labelText;
    private SpriteRenderer bgRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        labelText = GetComponentInChildren<TextMeshPro>();
        bgRenderer = transform.Find("BG")?.GetComponent<SpriteRenderer>();

        labelText.renderer.sortingOrder = 1;
        bgRenderer.sortingOrder = 0;

        cam = Camera.main;
        SetupLineRenderer();
    }

    void SetupLineRenderer()
    {
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.005f;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
    }

    void LateUpdate()
    {
        if (target == null || cam == null) return;

        // Position label with offset
        transform.position = target.position + labelOffset;
        transform.localScale = Vector3.one * labelScale;

        // Face camera
        Vector3 toCam = cam.transform.position - transform.position;
        toCam.y = 0;
        transform.rotation = Quaternion.LookRotation(-toCam);

        // Resize background to fit text
        if (labelText != null && bgRenderer != null)
        {
            // Get rendered size in local space
            Vector2 textSize = labelText.GetRenderedValues(false);

            float paddingX = 6;
            float paddingY = 3;

            bgRenderer.size = new Vector2(textSize.x + paddingX, textSize.y + paddingY);

            // Center background behind text
            bgRenderer.transform.localPosition = labelText.transform.localPosition;
        }

        // Draw line from part to label
        Vector3 direction = (transform.position - target.position).normalized;
        Vector3 startPoint = target.position + direction * 0.01f;

        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, transform.position);
    }

    public void Initialize(Transform targetTransform, Camera camera)
    {
        target = targetTransform;
        cam = camera != null ? camera : Camera.main;
    }
}
