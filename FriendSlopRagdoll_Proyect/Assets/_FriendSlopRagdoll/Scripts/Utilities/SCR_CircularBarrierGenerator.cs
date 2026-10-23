using UnityEngine;

public class CircularBarrierGenerator : MonoBehaviour
{
    [Header("Configuración de la barrera")]
    [Min(0.1f)]
    public float radius = 3f;

    [Min(3)]
    public int segmentCount = 24;

    [Min(0.1f)]
    public float height = 1.5f;

    [Min(0.05f)]
    public float thickness = 0.2f;

    [Header("Generación")]
    public bool generateOnStart = false;

    [ContextMenu("Generar barrera circular")]
    public void GenerateBarrier()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        float segmentWidth =
            2f * radius * Mathf.Tan(Mathf.PI / segmentCount) * 0.9f;

        for (int i = 0; i < segmentCount; i++)
        {
            float angle = i * 360f / segmentCount;
            float radians = angle * Mathf.Deg2Rad;

            Vector3 localPosition = new Vector3
            (
                Mathf.Sin(radians) * radius,
                height / 2f,
                Mathf.Cos(radians) * radius
            );

            GameObject segment = new GameObject
            (

            $"BarrierSegment_{i + 1:00}"

            );

            segment.AddComponent<BoxCollider>();    
            segment.transform.SetParent(transform, false);
            segment.transform.localPosition = localPosition;
            segment.transform.localRotation =
                Quaternion.Euler(0f, angle, 0f);

            segment.transform.localScale = new Vector3
            (
                segmentWidth,
                height,
                thickness
            );
        }
    }
    private void Start()
    {
        if (generateOnStart)
            GenerateBarrier();
    }
}