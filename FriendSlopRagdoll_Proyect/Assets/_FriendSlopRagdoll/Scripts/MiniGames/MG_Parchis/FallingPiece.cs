
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FallingPiece : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float destroyY = -20f;

    private Rigidbody rb;
    private DeliveryZone currentZone;

    public DeliveryZone CurrentZone => currentZone;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (transform.position.y < destroyY)
        {
            LeaveCurrentZone();
            Destroy(gameObject);
        }
    }

    public void SetCurrentZone(DeliveryZone newZone)
    {
        if (currentZone == newZone) return;

        if (currentZone != null)
            currentZone.RemovePiece(this);

        currentZone = newZone;

        if (currentZone != null)
            currentZone.AddPiece(this);
    }

    public void LeaveCurrentZone()
    {
        SetCurrentZone(null);
    }

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

    void OnDestroy()
    {
        if (currentZone != null)
            currentZone.RemovePiece(this);
    }
}
