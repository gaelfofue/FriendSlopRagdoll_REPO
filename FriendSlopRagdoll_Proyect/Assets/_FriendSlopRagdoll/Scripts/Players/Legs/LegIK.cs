using UnityEngine;

public class LegIK : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody legRigidbody;
    [SerializeField] Transform footTransform;

    [Header("IK Settings")]
    [SerializeField] float ikStrength = 30f;
    [SerializeField] float maxIKForce = 200f;
    [SerializeField] float maxIKSpeed = 5f;
    [SerializeField] bool waitForTarget = true;

    [Header("Suavizado")]
    [SerializeField] float deadzone = 0.05f;
    [SerializeField] float softZone = 0.15f;

    [Header("Debug")]
    [SerializeField] bool debugLogs = false;

    Vector3 footTarget;
    bool hasTarget = false;
    bool isStepping = false;   // <-- NUEVO

    public Vector3 FootPosition => footTransform != null ? footTransform.position : legRigidbody.position;
    public bool HasTarget => hasTarget;
    public bool IsStepping => isStepping;   // <-- NUEVO

    void Awake()
    {
        if (legRigidbody == null) legRigidbody = GetComponent<Rigidbody>();
        if (footTransform == null) footTransform = transform;
        footTarget = FootPosition;
    }

    public void SetFootTarget(Vector3 worldPos)
    {
        footTarget = worldPos;
        hasTarget = true;
    }

    // NUEVO: el LegStepper llama a esto al empezar/terminar un paso.
    // Solo cuando isStepping = true, el IK aplica fuerza.
    public void SetStepping(bool stepping)
    {
        isStepping = stepping;
    }

    void FixedUpdate()
    {
        if (legRigidbody == null) return;
        if (waitForTarget && !hasTarget) return;

        // FIX CLAVE: si no esta en paso, no hacer nada.
        // Antes el IK empujaba siempre, frenando el CoreBall.
        if (!isStepping) return;

        Vector3 footPos = FootPosition;
        Vector3 toTarget = footTarget - footPos;
        float dist = toTarget.magnitude;

        if (dist < deadzone) return;

        Vector3 dir = toTarget / dist;

        float speedTowards = Vector3.Dot(legRigidbody.linearVelocity, dir);
        if (speedTowards >= maxIKSpeed) return;

        float forceMag;
        if (dist < softZone)
            forceMag = Mathf.Lerp(0f, softZone * ikStrength, dist / softZone);
        else
            forceMag = dist * ikStrength;

        forceMag = Mathf.Min(forceMag, maxIKForce);

        Vector3 force = dir * forceMag;
        legRigidbody.AddForceAtPosition(force, footPos, ForceMode.Acceleration);

        if (debugLogs)
            Debug.Log($"[LegIK] {name}: dist={dist:F2}, force={forceMag:F1}");
    }

    void OnDrawGizmosSelected()
    {
        if (!hasTarget) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(footTarget, deadzone);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(FootPosition, footTarget);
    }
}