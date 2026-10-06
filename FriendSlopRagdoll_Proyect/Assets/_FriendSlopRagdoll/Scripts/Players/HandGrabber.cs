using UnityEngine;

// Logica de agarre de UNA mano. Cada mano tiene su propia instancia.
public class HandGrabber : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody handRigidBody;
    [SerializeField] PlayerInputRelay input;
    [SerializeField] bool isLeftHand;

    [Header("Grab Settings")]
    [SerializeField] float grabRadius = 0.25f;
    [SerializeField] LayerMask grabbableMask;

    FixedJoint grabJoint;
    Grabbable grabbedTarget;

    void OnEnable()
    {
        if (isLeftHand) { input.OnGrabLeftPressed += TryGrab; input.OnGrabLeftReleased += Release; }
        else { input.OnGrabRightPressed += TryGrab; input.OnGrabRightReleased += Release; }
    }

    void OnDisable()
    {
        if (isLeftHand) { input.OnGrabLeftPressed -= TryGrab; input.OnGrabLeftReleased -= Release; }
        else { input.OnGrabRightPressed -= TryGrab; input.OnGrabRightReleased -= Release; }
        Release();
    }

    void TryGrab()
    {
        if (grabJoint != null) return;

        Collider[] hits = Physics.OverlapSphere(handRigidBody.position, grabRadius, grabbableMask);
        Grabbable best = null;
        float bestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent(out Grabbable g)) continue;
            if (g.IsHeld || g.Rigidbody == handRigidBody) continue;

            float dist = Vector3.Distance(handRigidBody.position, g.Rigidbody.position);
            if (dist < bestDist) { bestDist = dist; best = g; }
        }

        if (best != null) Grab(best);
    }

    void Grab(Grabbable target)
    {
        grabbedTarget = target;
        grabbedTarget.SetHeld(true);

        grabJoint = handRigidBody.gameObject.AddComponent<FixedJoint>();
        grabJoint.connectedBody = target.Rigidbody;
    }

    void Release()
    {
        if (grabJoint != null) Destroy(grabJoint);
        grabJoint = null;

        grabbedTarget?.SetHeld(false);
        grabbedTarget = null;
    }
}
