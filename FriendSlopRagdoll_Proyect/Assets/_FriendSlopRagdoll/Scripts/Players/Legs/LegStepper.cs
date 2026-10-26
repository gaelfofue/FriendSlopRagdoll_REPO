using UnityEngine;

// Gestiona los pasos de las dos piernas. Sin rodilla: cada pierna es un palo
// rigido que se levanta entero y se planta entero.
// El pie se mueve hacia un "footTarget" que este script calcula en el suelo,
// delante del personaje, en la direccion de movimiento.
//
// FIX: el LegIK solo aplica fuerza cuando el pie esta en paso (IsStepping = true).
// Cuando el pie esta plantado, el LegIK no hace nada y no frena el CoreBall.
public class LegStepper : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody coreBall;
    [SerializeField] Transform torso;
    [SerializeField] LegIK leftLegIK;
    [SerializeField] LegIK rightLegIK;

    [Header("Step Settings")]
    [Tooltip("Distancia que debe avanzar el personaje para dar un paso.")]
    [SerializeField] float stepDistance = 0.35f;

    [Tooltip("Cuanto se levanta el pie al dar el paso.")]
    [SerializeField] float stepHeight = 0.15f;

    [Tooltip("Cuanto tarda el pie en llegar al nuevo destino.")]
    [SerializeField] float stepDuration = 0.2f;

    [Tooltip("Velocidad minima del CoreBall para empezar a dar pasos.")]
    [SerializeField] float minSpeedToStep = 0.3f;

    [Header("Foot Placement")]
    [Tooltip("Distancia lateral entre los pies (respecto al centro del personaje).")]
    [SerializeField] float footWidth = 0.15f;

    [Tooltip("Cuanto adelante del personaje se coloca el pie al pisar.")]
    [SerializeField] float footForwardOffset = 0.2f;

    [Tooltip("Capa del suelo para el raycast.")]
    [SerializeField] LayerMask groundMask = ~0;

    [Tooltip("Altura maxima del raycast desde el CoreBall hacia abajo.")]
    [SerializeField] float groundRayLength = 2f;

    [Header("Debug")]
    [SerializeField] bool debugLogs = false;

    // Estado interno
    float leftStepTimer = 0f;
    float rightStepTimer = 0f;
    Vector3 leftStepFrom, leftStepTo;
    Vector3 rightStepFrom, rightStepTo;
    bool leftStepping = false;
    bool rightStepping = false;

    void FixedUpdate()
    {
        if (coreBall == null || torso == null || leftLegIK == null || rightLegIK == null) return;

        Vector3 velocity = coreBall.linearVelocity;
        velocity.y = 0f;
        float speed = velocity.magnitude;

        if (speed < minSpeedToStep) return;

        Vector3 moveDir = velocity.normalized;

        // El frente del personaje es -Z local.
        Vector3 torsoForward = -torso.forward;
        Vector3 torsoRight = Vector3.Cross(Vector3.up, torsoForward).normalized;

        // Detectar suelo debajo del CoreBall.
        Vector3 groundPoint = coreBall.position;
        if (Physics.Raycast(coreBall.position + Vector3.up * 0.5f, Vector3.down,
                            out RaycastHit hit, groundRayLength, groundMask, QueryTriggerInteraction.Ignore))
        {
            groundPoint = hit.point;
        }

        // Si ningun pie esta pisando, decidir cual mueve.
        // El pie que este mas atras en la direccion de movimiento es el que toca mover.
        if (!leftStepping && !rightStepping)
        {
            float leftBehind = Vector3.Dot(leftLegIK.FootPosition - coreBall.position, moveDir);
            float rightBehind = Vector3.Dot(rightLegIK.FootPosition - coreBall.position, moveDir);

            // FIX: invertir la comparacion. El pie MAS ATRAS (valor mas negativo)
            // es el que hay que mover. Antes estaba al reves.
            if (leftBehind < rightBehind)
                StartStep(true, moveDir, torsoRight, groundPoint);
            else
                StartStep(false, moveDir, torsoRight, groundPoint);
        }

        // Avanzar los pasos en curso.
        if (leftStepping) TickStep(true);
        if (rightStepping) TickStep(false);

        if (debugLogs)
            Debug.Log($"[LegStepper] speed={speed:F2}, leftStepping={leftStepping}, rightStepping={rightStepping}");
    }

    void StartStep(bool isLeft, Vector3 moveDir, Vector3 torsoRight, Vector3 groundPoint)
    {
        Vector3 lateral = torsoRight * footWidth * (isLeft ? -1f : 1f);
        Vector3 forward = moveDir * footForwardOffset;

        Vector3 targetXZ = coreBall.position + lateral + forward;
        Vector3 target = targetXZ;
        if (Physics.Raycast(targetXZ + Vector3.up * 0.5f, Vector3.down,
                            out RaycastHit hit, groundRayLength, groundMask, QueryTriggerInteraction.Ignore))
        {
            target = hit.point;
        }

        target.y += stepHeight;

        if (isLeft)
        {
            leftStepping = true;
            leftStepTimer = 0f;
            leftStepFrom = leftLegIK.FootPosition;
            leftStepTo = target;
            leftLegIK.SetStepping(true);   // <-- NUEVO: activar IK solo durante el paso
        }
        else
        {
            rightStepping = true;
            rightStepTimer = 0f;
            rightStepFrom = rightLegIK.FootPosition;
            rightStepTo = target;
            rightLegIK.SetStepping(true);  // <-- NUEVO
        }

        if (debugLogs) Debug.Log($"[LegStepper] Step {(isLeft ? "L" : "R")} -> {target}");
    }

    void TickStep(bool isLeft)
    {
        if (isLeft)
        {
            leftStepTimer += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(leftStepTimer / stepDuration);

            Vector3 pos = Vector3.Lerp(leftStepFrom, leftStepTo, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * stepHeight;

            leftLegIK.SetFootTarget(pos);

            if (t >= 1f)
            {
                leftStepping = false;
                leftLegIK.SetStepping(false);  // <-- NUEVO: desactivar IK al plantar
                leftLegIK.SetFootTarget(leftStepTo - Vector3.up * stepHeight);
            }
        }
        else
        {
            rightStepTimer += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(rightStepTimer / stepDuration);

            Vector3 pos = Vector3.Lerp(rightStepFrom, rightStepTo, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * stepHeight;

            rightLegIK.SetFootTarget(pos);

            if (t >= 1f)
            {
                rightStepping = false;
                rightLegIK.SetStepping(false); // <-- NUEVO
                rightLegIK.SetFootTarget(rightStepTo - Vector3.up * stepHeight);
            }
        }
    }
}