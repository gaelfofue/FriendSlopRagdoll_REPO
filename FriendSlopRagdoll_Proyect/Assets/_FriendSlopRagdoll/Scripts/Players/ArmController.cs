using UnityEngine;

// FASE 1 - SOLO firmeza/flexion hacia adelante.
//
// CAMBIO CLAVE respecto a la version anterior:
// - Ya NO usamos SetTargetRotationLocal (el helper de Juckett) porque
//   depende de que el eje X local del hueso sea realmente "el eje de flexion",
//   lo cual NO se cumple en la mayoria de rigs importados.
// - En su lugar, usamos joint.targetRotation DIRECTAMENTE, y exponemos
//   los tres ejes (X, Y, Z) como campos separados para que puedas CALIBRAR
//   a ojo cual de ellos produce la flexion hacia adelante en TU rig.
// - Una vez calibrado, dejas solo el eje correcto con valor != 0 y los otros en 0.
//
// CONFIGURACION REQUERIDA en el ConfigurableJoint del hombro:
// - Rotation Drive Mode = X & YZ Drive (NO Slerp)
// - Angular X Motion / Y Motion / Z Motion = Limited o Free (NINGUNO Locked)
// - El drive del eje que uses (X o YZ) con Position Spring alto (300-800)
//   y Position Damper moderado (20-60). Maximum Force != 0.
// - El GameObject raiz del personaje DEBE tener SCR_RagdollSelfCollisionIgnorer.
[RequireComponent(typeof(ConfigurableJoint))]
public class SCR_ArmController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] ConfigurableJoint shoulderJoint;
    [SerializeField] PlayerInputRelay input;

    [Header("Arm Identity")]
    [SerializeField] bool isLeftArm;

    [Header("Calibracion de eje (AJUSTAR A OJO EN PLAY MODE)")]
    [Tooltip("Cual de los ejes del joint produce la flexion hacia adelante. " +
             "Empieza probando X. Si el brazo gira lateralmente, prueba Y o Z.")]
    [SerializeField] FlexAxis flexAxis = FlexAxis.X;

    [Tooltip("Angulo maximo de flexion (grados) en el eje elegido. " +
             "Prueba -90 si el brazo va hacia atras, +90 si va hacia adelante.")]
    [SerializeField] float maxFlexAngle = 90f;

    [Header("Smoothing")]
    [SerializeField] float flexSmoothSpeed = 8f;

    public enum FlexAxis { X, Y, Z }

    float rawFlexInput;
    float smoothedFlex;

    public float CurrentFlexAmount => smoothedFlex;

    void OnEnable()
    {
        if (isLeftArm)
            input.OnFlexLeftArm += HandleFlexInput;
        else
            input.OnFlexRightArm += HandleFlexInput;
    }

    void OnDisable()
    {
        if (isLeftArm)
            input.OnFlexLeftArm -= HandleFlexInput;
        else
            input.OnFlexRightArm -= HandleFlexInput;
    }

    void HandleFlexInput(float value)
    {
        rawFlexInput = Mathf.Clamp01(value);
    }

    void FixedUpdate()
    {
        smoothedFlex = Mathf.MoveTowards(smoothedFlex, rawFlexInput, flexSmoothSpeed * Time.fixedDeltaTime);

        // Construimos la rotacion objetivo SOLO en el eje elegido.
        // Los otros dos ejes quedan en 0 (sin cambio respecto a la pose de reposo).
        float angle = smoothedFlex * maxFlexAngle;
        Quaternion target = flexAxis switch
        {
            FlexAxis.X => Quaternion.Euler(angle, 0f, 0f),
            FlexAxis.Y => Quaternion.Euler(0f, angle, 0f),
            FlexAxis.Z => Quaternion.Euler(0f, 0f, angle),
            _ => Quaternion.identity
        };

        shoulderJoint.targetRotation = target;
    }

    // --- Diagnostico visual en el Editor ---
    void OnDrawGizmosSelected()
    {
        if (shoulderJoint == null) return;

        // Dibuja el eje del joint en el que estamos intentando girar.
        Vector3 worldAxis = shoulderJoint.transform.TransformDirection(flexAxis switch
        {
            FlexAxis.X => shoulderJoint.axis,
            FlexAxis.Y => Vector3.Cross(shoulderJoint.axis, shoulderJoint.secondaryAxis).normalized,
            FlexAxis.Z => shoulderJoint.secondaryAxis,
            _ => Vector3.right
        });

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(shoulderJoint.transform.position,
                        shoulderJoint.transform.position + worldAxis * 0.3f);
    }
}