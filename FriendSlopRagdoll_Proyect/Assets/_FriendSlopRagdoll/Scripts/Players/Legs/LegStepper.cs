using UnityEngine;

public class LegStepper : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody coreBall;
    [SerializeField] Transform torso;
    [SerializeField] LegIK leftLegIK;
    [SerializeField] LegIK rightLegIK;

    [Header("Step Settings")]
    [SerializeField] float stepDistance = 0.5f;
    [Tooltip("Ya NO es una altura literal en metros. Controla cuanto se inclina extra " +
             "la pierna a mitad de paso para simular el levantamiento del pie (como una rodilla virtual). " +
             "Prueba valores entre 0.1 y 0.25 con legLength=0.5.")]
    [SerializeField] float stepHeight = 0.15f;
    [SerializeField] float stepDuration = 0.25f;
    [SerializeField] float minSpeedToStep = 0.15f;

    [Header("Foot Placement")]
    [SerializeField] float footWidth = 0.25f;
    [SerializeField] float footForwardOffset = 0.35f;
    [SerializeField] LayerMask groundMask = ~0;
    [SerializeField] float groundRayLength = 3f;

    [Header("Debug")]
    [SerializeField] bool debugLogs = false;

    float leftStepTimer = 0f;
    float rightStepTimer = 0f;
    Vector3 leftStepFrom, rightStepFrom;
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

        if (!leftStepping && !rightStepping)
        {
            float leftBehind = Vector3.Dot(leftLegIK.FootPosition - coreBall.position, moveDir);
            float rightBehind = Vector3.Dot(rightLegIK.FootPosition - coreBall.position, moveDir);

            if (leftBehind < rightBehind)
                StartStep(true);
            else
                StartStep(false);
        }

        if (leftStepping) TickStep(true);
        if (rightStepping) TickStep(false);

        if (debugLogs)
            Debug.Log($"[LegStepper] speed={speed:F2}, leftStepping={leftStepping}, rightStepping={rightStepping}");
    }

    void StartStep(bool isLeft)
    {
        if (isLeft)
        {
            leftStepping = true;
            leftStepTimer = 0f;
            leftStepFrom = leftLegIK.FootPosition;
            leftLegIK.SetStepping(true);

            if (debugLogs)
                Debug.Log($"[LegStepper] Step L start. From={leftStepFrom}");
        }
        else
        {
            rightStepping = true;
            rightStepTimer = 0f;
            rightStepFrom = rightLegIK.FootPosition;
            rightLegIK.SetStepping(true);

            if (debugLogs)
                Debug.Log($"[LegStepper] Step R start. From={rightStepFrom}");
        }
    }

    void TickStep(bool isLeft)
    {
        if (isLeft)
        {
            leftStepTimer += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(leftStepTimer / stepDuration);

            Vector3 currentTarget = CalculateStepTarget(true, coreBall.position);
            Vector3 pos = ComputeSwingPosition(leftLegIK, leftStepFrom, currentTarget, t);

            leftLegIK.SetFootTarget(pos);

            if (t >= 1f)
            {
                leftStepping = false;
                leftLegIK.SetStepping(false);
            }
        }
        else
        {
            rightStepTimer += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(rightStepTimer / stepDuration);

            Vector3 currentTarget = CalculateStepTarget(false, coreBall.position);
            Vector3 pos = ComputeSwingPosition(rightLegIK, rightStepFrom, currentTarget, t);

            rightLegIK.SetFootTarget(pos);

            if (t >= 1f)
            {
                rightStepping = false;
                rightLegIK.SetStepping(false);
            }
        }
    }

    // --- NUEVO: calculo del arco real de pendulo (sustituye al Lerp + altura en Y) ---
    //
    // Como la pierna es un segmento rigido sin rodilla, el pie SOLO puede estar
    // sobre la superficie de una esfera de radio "legLength" centrada en el hipAnchor.
    // Por eso, en vez de interpolar POSICIONES en linea recta, interpolamos DIRECCIONES
    // (con Slerp, que da el arco mas corto sobre la esfera = el swing real de un pendulo),
    // y añadimos una inclinacion extra temporal (con un seno) para simular el efecto
    // de "levantar el pie", ya que geometricamente mas inclinacion = mas altura.
    Vector3 ComputeSwingPosition(LegIK legIK, Vector3 stepFrom, Vector3 currentTarget, float t)
    {
        Transform hip = legIK.HipAnchor;

        // Fallback por si a alguna pierna no se le asigno el hipAnchor todavia
        if (hip == null)
        {
            Vector3 posFallback = Vector3.Lerp(stepFrom, currentTarget, t);
            posFallback.y += Mathf.Sin(t * Mathf.PI) * stepHeight;
            return posFallback;
        }

        Vector3 hipPos = hip.position;
        float legLen = Mathf.Max(legIK.LegLength, 0.01f);
        Vector3 rest = legIK.RestDirection; // normalmente Vector3.down

        Vector3 dirFrom = (stepFrom - hipPos).normalized;
        Vector3 dirTo = (currentTarget - hipPos).normalized;

        // Direccion horizontal de respaldo por si dirFrom/dirTo coinciden con "rest"
        // (caso borde: pierna colgando exactamente recta)
        Vector3 fallbackH = Vector3.ProjectOnPlane(-torso.forward, rest);
        fallbackH = fallbackH.sqrMagnitude > 0.0001f ? fallbackH.normalized : Vector3.forward;

        Vector3 hFrom = GetHorizontalComponent(dirFrom, rest, fallbackH);
        Vector3 hTo = GetHorizontalComponent(dirTo, rest, fallbackH);

        // Interpolamos el "rumbo" horizontal del swing
        Vector3 hInterp = Vector3.Slerp(hFrom, hTo, t);
        hInterp = hInterp.sqrMagnitude > 0.0001f ? hInterp.normalized : fallbackH;

        // Interpolamos el angulo de inclinacion base...
        float thetaFrom = ComputeTilt(dirFrom, rest);
        float thetaTo = ComputeTilt(dirTo, rest);
        float thetaBase = Mathf.Lerp(thetaFrom, thetaTo, t);

        // ...y le sumamos una inclinacion EXTRA temporal (la "U" del paso).
        // Mas angulo respecto al reposo = pie mas alto (geometria del pendulo).
        float liftAngleRad = Mathf.Asin(Mathf.Clamp01(stepHeight / legLen));
        float theta = thetaBase + Mathf.Sin(t * Mathf.PI) * liftAngleRad;
        theta = Mathf.Clamp(theta, 0f, 89f * Mathf.Deg2Rad);

        // Reconstruimos la direccion final combinando inclinacion + rumbo horizontal
        Vector3 dir = rest * Mathf.Cos(theta) + hInterp * Mathf.Sin(theta);

        // SIEMPRE a distancia legLength exacta: es fisicamente lo unico que la pierna puede hacer
        return hipPos + dir * legLen;
    }

    float ComputeTilt(Vector3 dir, Vector3 rest)
    {
        float d = Mathf.Clamp(Vector3.Dot(dir.normalized, rest.normalized), -1f, 1f);
        return Mathf.Acos(d);
    }

    Vector3 GetHorizontalComponent(Vector3 dir, Vector3 rest, Vector3 fallback)
    {
        Vector3 perp = dir - Vector3.Dot(dir, rest) * rest;
        if (perp.sqrMagnitude < 0.0001f) return fallback;
        return perp.normalized;
    }

    Vector3 CalculateStepTarget(bool isLeft, Vector3 coreBallPos)
    {
        Vector3 velocity = coreBall.linearVelocity;
        velocity.y = 0f;
        Vector3 moveDir = velocity.magnitude > 0.01f ? velocity.normalized : -torso.forward;

        Vector3 torsoForward = -torso.forward;
        Vector3 torsoRight = Vector3.Cross(Vector3.up, torsoForward).normalized;

        Vector3 lateral = torsoRight * footWidth * (isLeft ? -1f : 1f);
        Vector3 forward = moveDir * footForwardOffset;

        Vector3 targetXZ = coreBallPos + lateral + forward;
        Vector3 target = targetXZ;
        if (Physics.Raycast(targetXZ + Vector3.up * 0.5f, Vector3.down,
                            out RaycastHit hit, groundRayLength, groundMask, QueryTriggerInteraction.Ignore))
        {
            target = hit.point;
        }

        return target;
    }
}