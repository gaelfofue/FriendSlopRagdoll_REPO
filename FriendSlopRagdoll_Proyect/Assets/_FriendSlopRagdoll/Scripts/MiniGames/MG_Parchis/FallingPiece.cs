using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FallingPiece : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float destroyY = -20f; // Si cae por debajo de esto, se destruye
    [SerializeField] private float minImpactVelocity = 1f; // Velocidad mínima para "estabilizarse"

    private Rigidbody rb;
    private bool isDelivered = false;

    public bool IsDelivered => isDelivered;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (transform.position.y < destroyY)
            Destroy(gameObject);
    }

    // Llamado por la DeliveryZone cuando la ficha entra en una base
    public void MarkAsDelivered()
    {
        isDelivered = true;
        rb.isKinematic = true; // Se queda quieta en la base
        // Opcional: desactivar collider para que no estorbe
        GetComponent<Collider>().enabled = false;
    }

    // Opcional: por si quieres poder cogerla en el aire
    public void OnPickedUp(Transform holdPoint)
    {
        rb.isKinematic = true;
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
    }

    public void OnDropped()
    {
        transform.SetParent(null);
        rb.isKinematic = false;
    }
}