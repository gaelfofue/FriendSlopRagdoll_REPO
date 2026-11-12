using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WarningRing : MonoBehaviour
{
    [Header("Forma")]
    [SerializeField] private float radius = 2f;
    [SerializeField] private int segments = 64;

    [Header("Pulso")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minScale = 0.85f;
    [SerializeField] private float maxScale = 1.1f;

    [Header("Brillo")]
    [SerializeField] private float minIntensity = 1f;
    [SerializeField] private float maxIntensity = 3f;

    private LineRenderer line;
    private Material ringMaterial;
    private float pulse;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;

        ringMaterial = line.material;
        line.startColor = Color.red;
        line.endColor = Color.red;

        DrawCircle();
    }

    private void Update()
    {
        pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;

        float currentScale = Mathf.Lerp(minScale, maxScale, pulse);

        transform.localScale = new Vector3(
            currentScale,
            1f,
            currentScale
        );

        if (ringMaterial != null && ringMaterial.HasProperty("_EmissionColor"))
        {
            ringMaterial.SetColor(
                "_EmissionColor",
                Color.red * 4f
            );
        }
    }

    private void DrawCircle()
    {
        line.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;

            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            line.SetPosition(i, new Vector3(x, 0f, z));
        }
    }
}