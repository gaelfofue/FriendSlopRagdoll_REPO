using UnityEngine;

public class LegIK : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody legRigidbody;
    [SerializeField] Transform footTransform;

    [Header("IK Settings")]
    [SerializeField] float ikStrength = 30f;
    [SerializeField] float maxIKForce = 200f;
    [SerializeField] float dampingFactor = 3f;
    [SerializeField] bool waitForTarget = true;

    [Header("Suavizado")]
    [SerializeField] float deadzone = 0.05f;
    [SerializeField] float softZone = 0.15f;

    [Header("Workspace Limit (evita pelear contra el Joint)")]
    [SerializeField] Transform hipAnchor;
    [SerializeField] float legLength = 0.5f;
    [SerializeField] float maxConeAngle = 50f;
    [SerializeField] Vector3 restDirection = Vector3.down;

    [Header("Debug")]
    [SerializeField] bool debugLogs = false;

    Vector3 footTarget;
    bool hasTarget = false;
    bool isStepping = false;

    public Vector3 FootPosition => footTransform != null ? footTransform.position : legRigidbody.position;
    public bool HasTarget => hasTarget;
    public bool IsStepping => isStepping;

    // NUEVO: exponemos esto para que LegStepper pueda calcular el arco esferico real
    public Transform HipAnchor => hipAnchor;
    public float LegLength => legLength;
    public Vector3 RestDirection => restDirection.normalized;

    void Awake()
    {
        if (legRigidbody == null) legRigidbody = GetComponent<Rigidbody>();
        if (footTransform == null) footTransform = transform;
        footTarget = FootPosition;
    }

    public void SetFootTarget(Vector3 worldPos)
    {
        footTarget = ClampToReachableCone(worldPos);
        hasTarget = true;
    }

    public void SetStepping(bool stepping)
    {
        isStepping = stepping;
    }

    Vector3 ClampToReachableCone(Vector3 worldTarget)
    {
        if (hipAnchor == null) return worldTarget;

        Vector3 toTarget = worldTarget - hipAnchor.position;
        float dist = Mathf.Min(toTarget.magnitude, legLength);
        if (dist < 0.0001f) return hipAnchor.position + restDirection.normalized * 0.0001f;

        Vector3 dir = toTarget.normalized;
        Vector3 rest = restDirection.normalized;

        float angle = Vector3.Angle(rest, dir);
        if (angle > maxConeAngle)
            dir = Vector3.RotateTowards(rest, dir, maxConeAngle * Mathf.Deg2Rad, 0f);

        return hipAnchor.position + dir * dist;
    }

    void FixedUpdate()
    {
        if (legRigidbody == null) return;
        if (waitForTarget && !hasTarget) return;
        if (!isStepping) return;

        Vector3 footPos = FootPosition;
        Vector3 toTarget = footTarget - footPos;
        float dist = toTarget.magnitude;

        if (dist < deadzone)
        {
            if (debugLogs)
                Debug.Log($"[LegIK] {name}: en target (dist={dist:F2})");
            return;
        }

        Vector3 dir = toTarget / dist;

        float forceMag;
        if (dist < softZone)
            forceMag = Mathf.Lerp(0f, softZone * ikStrength, dist / softZone);
        else
            forceMag = dist * ikStrength;

        forceMag = Mathf.Min(forceMag, maxIKForce);

        Vector3 springForce = dir * forceMag;
        Vector3 dampingForce = -legRigidbody.linearVelocity * dampingFactor;
        Vector3 force = springForce + dampingForce;

        if (force.magnitude > maxIKForce)
            force = force.normalized * maxIKForce;

        legRigidbody.AddForceAtPosition(force, footPos, ForceMode.Acceleration);

        if (debugLogs)
            Debug.Log($"[LegIK] {name}: dist={dist:F2}, force={force.magnitude:F1}, dir={dir}, " +
                      $"footPos={footPos}, target={footTarget}");
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