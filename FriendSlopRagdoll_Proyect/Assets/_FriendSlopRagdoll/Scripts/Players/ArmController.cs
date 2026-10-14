using UnityEngine;

// Recibe un valor analogico (0 = idle colgando, 1 = flexion maxima al frente)
// y mueve el targetRotation del ConfigurableJoint del hombro en consecuencia.
//
// CONFIGURACION REQUERIDA en el ConfigurableJoint del hueso del brazo (hombro):
// - Angular X Motion = Limited o Free (NUNCA Locked, si esta Locked el brazo no se movera nunca)
// - Angular Y Motion / Angular Z Motion = Limited (limita el rango para que no se abra hacia los lados)
// - Angular X Drive -> Position Spring: prueba un valor alto, 300-600
// - Angular X Drive -> Position Damper: prueba un valor moderado, 20-50
//
// Este script NO sabe nada de agarre, overhead ni seleccion de brazo.
// Resuelve EXCLUSIVAMENTE el continuo idle -> flexion frontal.

[RequireComponent(typeof(ConfigurableJoint))]
public class ArmController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] ConfigurableJoint shoulderJoint;
    [SerializeField] PlayerInputRelay input;

    [Header("Arm Identity")]
    [Tooltip("Marca true si este componente controla el brazo IZQUIERDO")]
    [SerializeField] bool isLeftArm;

    [Header("Poses (Euler local del hueso, ajustar a ojo en Play Mode)")]
    [Tooltip("Rotacion local cuando el brazo cuelga relajado (idle)")]
    [SerializeField] Vector3 idlePoseEuler = Vector3.zero;

    [Tooltip("Rotacion local cuando el brazo esta 100% flexionado hacia el frente")]
    [SerializeField] Vector3 forwardPoseEuler = new Vector3(-90f, 0f, 0f);

    [Header("Smoothing")]
    [Tooltip("Velocidad con la que el brazo sigue al valor de input. Mayor = responde mas rapido")]
    [SerializeField] float flexSmoothSpeed = 8f;

    // Valor crudo que llega del input (0-1), sin procesar
    float rawFlexInput;

    // Valor ya suavizado, es el que realmente se usa para mover el joint
    float smoothedFlex;

    // Rotacion local inicial del hueso, necesaria para los calculos de espacio del joint
    Quaternion startLocalRotation;

    // Publico de solo lectura: otros sistemas futuros (seleccion, agarre) podran
    // consultar cuanto esta flexionado este brazo ahora mismo, sin modificarlo.
    public float CurrentFlexAmount => smoothedFlex;

    void Awake()
    {
        startLocalRotation = shoulderJoint.transform.localRotation;
    }

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
        // Clamp de seguridad. El Input System deberia darnos 0-1 pero mejor curarnos en salud.
        rawFlexInput = Mathf.Clamp01(value);
    }

    void FixedUpdate()
    {
        // Suavizamos para que el brazo no salte instantaneamente al valor crudo del input,
        // esto ayuda a que se sienta "fisico" y no robotico.
        smoothedFlex = Mathf.MoveTowards(smoothedFlex, rawFlexInput, flexSmoothSpeed * Time.fixedDeltaTime);

        Vector3 blendedEuler = Vector3.Lerp(idlePoseEuler, forwardPoseEuler, smoothedFlex);
        shoulderJoint.SetTargetRotationLocal(Quaternion.Euler(blendedEuler), startLocalRotation);
    }

#if UNITY_EDITOR
    // Ayuda visual en el Inspector para debug, no afecta al build
    void OnDrawGizmosSelected()
    {
        if (shoulderJoint == null) return;
        UnityEditor.Handles.Label(shoulderJoint.transform.position + Vector3.up * 0.2f,
            $"Flex: {smoothedFlex:F2} (raw: {rawFlexInput:F2})");
    }
#endif
}