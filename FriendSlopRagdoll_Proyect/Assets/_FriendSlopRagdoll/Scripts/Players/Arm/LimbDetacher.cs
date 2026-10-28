using System;
using UnityEngine;

// Vive en el UpperArm. Escucha la fuerza del FixedJoint del agarre
// (el que conecta la mano de otro jugador con este brazo).
// Si la fuerza supera detachForce durante detachDuration segundos,
// el brazo se desprende del Torso.
//
// Al desprenderse:
// - Destruye el ConfigurableJoint del hombro.
// - Desactiva ArmController, PunchAbility, HandGrabber.
// - Convierte el Grabbable del brazo en objeto normal (IsLimb = false).
// - El Rigidbody del brazo pasa a ser independiente.
public class LimbDetacher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] ConfigurableJoint shoulderJoint;   // ConfigurableJoint del hombro (UpperArm -> Torso)
    [SerializeField] Rigidbody limbRigidbody;            // Rigidbody del UpperArm
    [SerializeField] Grabbable limbGrabbable;            // Grabbable del UpperArm (IsLimb = true)
    [SerializeField] ArmController armController;        // Control de flexion/overhead
    [SerializeField] PunchAbility punchAbility;          // Golpe
    [SerializeField] HandGrabber handGrabber;            // Agarre de la mano

    [Header("Detach Settings")]
    [Tooltip("Fuerza del FixedJoint del agarre necesaria para empezar a contar.")]
    [SerializeField] float detachForce = 800f;

    [Tooltip("Tiempo que la fuerza debe superar el umbral para desprender.")]
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

        // Solo contar fuerza si el brazo esta siendo agarrado por OTRO jugador.
        // Si IsHeld = false, el brazo no esta agarrado, no hay desmembramiento.
        if (!limbGrabbable.IsHeld) { overThresholdTimer = 0f; return; }

        // La fuerza del FixedJoint del agarre NO se puede leer directamente
        // desde el ConfigurableJoint del hombro. Lo que hacemos es leer
        // la fuerza que el ConfigurableJoint del hombro esta haciendo para
        // mantener el brazo unido al Torso. Si el agarre tira fuerte, el
        // hombro hace fuerza para no soltarse, y esa fuerza es la que medimos.
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
            Debug.Log($"[LimbDetacher {name}] force={force:F0}, timer={overThresholdTimer:F2}");
    }

    void Detach()
    {
        if (IsDetached) return;
        IsDetached = true;

        if (debugLogs) Debug.Log($"[LimbDetacher {name}] DETACHED!");

        // 1. Destruir el ConfigurableJoint del hombro (el brazo se separa del Torso).
        if (shoulderJoint != null)
        {
            Destroy(shoulderJoint);
            shoulderJoint = null;
        }

        // 2. Desactivar scripts de control del brazo.
        if (armController != null) armController.enabled = false;
        if (punchAbility != null) punchAbility.enabled = false;
        if (handGrabber != null) handGrabber.enabled = false;

        // 3. Convertir el Grabbable en objeto normal (ya no es un brazo "de jugador").
        if (limbGrabbable != null)
        {
            limbGrabbable.SetLimb(false);
            // Si estaba siendo agarrado por otro jugador, forzar la suelta
            // para que el FixedJoint no quede huerfano.
            limbGrabbable.SetHeld(false);
        }

        // 4. Notificar a los sistemas interesados (AbilityUnlocker, etc).
        OnDetached?.Invoke(this);
    }
}