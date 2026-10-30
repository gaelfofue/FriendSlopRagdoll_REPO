using System;
using UnityEngine;

public class LimbDetacher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] ConfigurableJoint shoulderJoint;
    [SerializeField] Rigidbody limbRigidbody;
    [SerializeField] Grabbable limbGrabbable;
    [SerializeField] ArmController armController;
    [SerializeField] PunchAbility punchAbility;
    [SerializeField] HandGrabber handGrabber;

    [Header("Detach Settings")]
    [SerializeField] float detachForce = 800f;
    [SerializeField] float detachDuration = 0.4f;

    [Header("Debug")]
    [SerializeField] bool debugLogs = true;

    public bool IsDetached { get; private set; }
    public event Action<LimbDetacher> OnDetached;

    float overThresholdTimer;

    void FixedUpdate()
    {
        if (IsDetached) return;
        if (shoulderJoint == null || limbGrabbable == null) return;

        // 1. Solo contar si el brazo esta siendo agarrado.
        if (!limbGrabbable.IsHeld) { overThresholdTimer = 0f; return; }

        // 2. Solo contar si el que agarra NO es el propio jugador.
        //    Si el jugador se agarra a si mismo (o el brazo es del mismo root),
        //    no contamos fuerza.
        Transform holder = limbGrabbable.CurrentHolder;
        Transform owner = limbGrabbable.Owner;

        if (holder == null || owner == null) { overThresholdTimer = 0f; return; }

        if (holder == owner)
        {
            // El dueño esta agarrando su propio brazo. No cuenta.
            overThresholdTimer = 0f;
            return;
        }

        // 3. Medir la fuerza del joint del hombro.
        float force = shoulderJoint.currentForce.magnitude;

        if (force >= detachForce)
        {
            overThresholdTimer += Time.fixedDeltaTime;
            if (overThresholdTimer >= detachDuration)
                Detach();
        }
        else
        {
            overThresholdTimer = 0f;
        }

        if (debugLogs && force > detachForce * 0.5f)
            Debug.Log($"[LimbDetacher {name}] force={force:F0}, timer={overThresholdTimer:F2}, holder={holder.name}, owner={owner.name}");
    }

    void Detach()
    {
        if (IsDetached) return;
        IsDetached = true;

        if (debugLogs) Debug.Log($"[LimbDetacher {name}] DETACHED!");

        if (armController != null)
        {
            DestroyImmediate(armController);
            armController = null;
        }

        if (punchAbility != null) punchAbility.enabled = false;
        if (handGrabber != null) handGrabber.enabled = false;

        if (shoulderJoint != null)
        {
            Destroy(shoulderJoint);
            shoulderJoint = null;
        }

        if (limbGrabbable != null)
        {
            limbGrabbable.SetLimb(false);
            limbGrabbable.SetHeld(false);
        }

        OnDetached?.Invoke(this);
    }
}