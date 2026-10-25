using UnityEngine;

public class HandGrabber : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody handRigidbody;
    [SerializeField] PlayerInputRelay input;
    [SerializeField] bool isLeftHand;
    [SerializeField] ArmController armController;
    [SerializeField] Transform aimOrigin;

    [Header("Grab Detection")]
    [SerializeField] float grabRadius = 0.4f;
    [SerializeField] LayerMask grabbableMask;

    [Header("Assist (vaga)")]
    [SerializeField] float assistMaxDistance = 1.5f;
    [SerializeField] float assistAcceleration = 4f;
    [SerializeField] float assistMaxSpeed = 2f;
    [SerializeField] float assistMinFlex = 0.3f;

    [Header("Break")]
    [SerializeField] float baseBreakForce = 1500f;

    [Header("Debug")]
    [SerializeField] bool debugLogs = true;

    FixedJoint grabJoint;
    Grabbable grabbedTarget;
    bool grabWanted;
    Rigidbody ownRootBody;

    public bool IsGrabbing => grabJoint != null;
    public Grabbable GrabbedTarget => grabbedTarget;

    void Awake()
    {
        ownRootBody = GetComponentInParent<Rigidbody>();
        if (ownRootBody == null && handRigidbody != null)
            ownRootBody = handRigidbody;
    }

    void Update()
    {
        // Asistencia: solo si queremos agarrar y no tenemos nada agarrado.
        if (!grabWanted || IsGrabbing) return;

        bool armReady = armController == null
            || armController.IsOverhead
            || armController.CurrentFlexAmount >= assistMinFlex;

        if (!armReady) return;

        Grabbable target = FindBestTarget();
        if (target != null)
            AssistTowards(target);
    }

    void FixedUpdate()
    {
        // CLAVE: mientras queramos agarrar y no tengamos nada,
        // reintentamos TryGrab() cada FixedUpdate.
        // Asi, cuando la asistencia acerque la mano, lo detectamos.
        if (!grabWanted || IsGrabbing) return;
        if (armController != null && armController.IsPunchLocked) return;

        if (debugLogs)
            Debug.Log($"[HandGrabber {(isLeftHand ? "L" : "R")}] Buscando target... hand={handRigidbody.position}, radius={grabRadius}");

        Grabbable target = FindBestTarget();
        if (target != null)
        {
            if (debugLogs)
                Debug.Log($"[HandGrabber {(isLeftHand ? "L" : "R")}] Target encontrado: {target.name}. Agarrando.");
            Grab(target);
        }
    }

    void OnDestroy()
    {
        if (grabJoint != null) Destroy(grabJoint);
    }

    public void TryGrab()
    {
        if (IsGrabbing) return;
        if (armController != null && armController.IsPunchLocked) return;

        Grabbable target = FindBestTarget();
        if (target == null) return;

        Grab(target);
    }

    public void Release()
    {
        grabWanted = false;

        if (grabJoint != null)
        {
            Destroy(grabJoint);
            grabJoint = null;
        }

        if (grabbedTarget != null)
        {
            grabbedTarget.SetHeld(false);
            grabbedTarget = null;
        }
    }

    public void ForceRelease()
    {
        if (grabJoint != null)
        {
            Destroy(grabJoint);
            grabJoint = null;
        }

        if (grabbedTarget != null)
        {
            grabbedTarget.SetHeld(false);
            grabbedTarget = null;
        }
    }

    public void SetGrabWanted(bool wanted)
    {
        grabWanted = wanted;
        if (debugLogs)
            Debug.Log($"[HandGrabber {(isLeftHand ? "L" : "R")}] SetGrabWanted({wanted})");
    }

    Grabbable FindBestTarget()
    {
        if (handRigidbody == null) return null;

        Vector3 aimDir = GetAimDirection();
        Collider[] hits = Physics.OverlapSphere(handRigidbody.position, grabRadius, grabbableMask);

        if (debugLogs && hits.Length > 0)
            Debug.Log($"[HandGrabber {(isLeftHand ? "L" : "R")}] OverlapSphere devolvio {hits.Length} colliders.");

        Grabbable best = null;
        float bestScore = float.MinValue;

        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent(out Grabbable g))
            {
                if (debugLogs)
                    Debug.Log($"[HandGrabber {(isLeftHand ? "L" : "R")}] Collider {hit.name} NO tiene SCR_Grabbable. Descartado.");
                continue;
            }

            if (g.IsHeld) { if (debugLogs) Debug.Log($"[HandGrabber {(isLeftHand ? "L" : "R")}] {g.name} ya esta IsHeld. Descartado."); continue; }
            if (g.Rigidbody == handRigidbody) { if (debugLogs) Debug.Log($"[HandGrabber {(isLeftHand ? "L" : "R")}] {g.name} es la propia mano. Descartado."); continue; }
            if (IsOwnBody(g)) { if (debugLogs) Debug.Log($"[HandGrabber {(isLeftHand ? "L" : "R")}] {g.name} es del propio cuerpo. Descartado."); continue; }

            Vector3 toTarget = g.Rigidbody.position - handRigidbody.position;
            float dist = toTarget.magnitude;

            // NOTA: NO filtramos por dist > grabRadius. El OverlapSphere ya ha filtrado
            // por radio usando la posicion del COLLIDER. El Rigidbody del Grabbable
            // puede estar mas lejos (si el Grabbable esta en un hijo, o el Rigidbody
            // esta en el centro del objeto y el collider en la superficie).
            // Usamos dist solo para el score.

            if (aimOrigin != null)
            {
                Vector3 toTargetFromAim = (g.Rigidbody.position - aimOrigin.position).normalized;
                float alignment = Vector3.Dot(aimDir, toTargetFromAim);
                if (alignment < 0.3f)
                {
                    if (debugLogs) Debug.Log($"[HandGrabber {(isLeftHand ? "L" : "R")}] {g.name} no esta alineado (alignment={alignment:F2}). Descartado.");
                    continue;
                }
            }

            float score = -dist + Vector3.Dot(aimDir, toTarget.normalized) * 0.5f;

            if (debugLogs)
                Debug.Log($"[HandGrabber {(isLeftHand ? "L" : "R")}] {g.name} candidato: dist={dist:F2}, score={score:F2}");

            if (score > bestScore) { bestScore = score; best = g; }
        }

        if (debugLogs)
            Debug.Log($"[HandGrabber {(isLeftHand ? "L" : "R")}] Best target: {(best != null ? best.name : "NULL")}");

        return best;
    }

    void AssistTowards(Grabbable target)
    {
        Vector3 dir = (target.Rigidbody.position - handRigidbody.position).normalized;
        float dist = Vector3.Distance(handRigidbody.position, target.Rigidbody.position);
        if (dist > assistMaxDistance) return;

        float speedTowards = Vector3.Dot(handRigidbody.linearVelocity, dir);
        if (speedTowards >= assistMaxSpeed) return;

        handRigidbody.AddForce(dir * assistAcceleration, ForceMode.Acceleration);
    }

    void Grab(Grabbable target)
    {
        grabbedTarget = target;
        grabbedTarget.SetHeld(true);

        grabJoint = handRigidbody.gameObject.AddComponent<FixedJoint>();
        grabJoint.connectedBody = target.Rigidbody;

        if (target.IsLimb)
            grabJoint.breakForce = float.PositiveInfinity;
        else
            grabJoint.breakForce = baseBreakForce * target.BreakForceMultiplier;

        if (debugLogs)
            Debug.Log($"[HandGrabber {(isLeftHand ? "L" : "R")}] Grab OK: {target.name}. breakForce={grabJoint.breakForce}");
    }

    void OnJointBreak(float breakForce)
    {
        if (debugLogs)
            Debug.Log($"[HandGrabber {(isLeftHand ? "L" : "R")}] Joint roto por fuerza {breakForce}.");
        grabJoint = null;
        if (grabbedTarget != null) { grabbedTarget.SetHeld(false); grabbedTarget = null; }
    }

    Vector3 GetAimDirection()
    {
        if (aimOrigin != null)
            return (handRigidbody.position - aimOrigin.position).normalized;
        return handRigidbody.transform.forward;
    }

    bool IsOwnBody(Grabbable g)
    {
        if (ownRootBody == null) return false;
        return g.Rigidbody == ownRootBody || g.transform.IsChildOf(ownRootBody.transform);
    }

    void OnDrawGizmosSelected()
    {
        if (handRigidbody == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(handRigidbody.position, grabRadius);
        if (aimOrigin != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(aimOrigin.position, aimOrigin.position + GetAimDirection() * 1f);
        }
    }
}