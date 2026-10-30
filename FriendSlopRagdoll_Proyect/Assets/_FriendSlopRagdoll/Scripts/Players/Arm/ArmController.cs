using UnityEngine;
public class ArmController : MonoBehaviour
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

    [Tooltip("Angulo del overhead (brazos por encima de la cabeza). " +
             "Suele ser -180 o +180 en el mismo eje que la flexion.")]
    [SerializeField] float overheadAngle = 180f;

    [Header("Smoothing")]
    [SerializeField] float flexSmoothSpeed = 8f;

    [Header("Fisicas del brazo (reposo)")]
    [SerializeField] float activeSpring = 600f;
    [SerializeField] float activeDamper = 40f;
    [SerializeField] float idleSpring = 0f;
    [SerializeField] float idleDamper = 5f;
    [SerializeField] float idleThreshold = 0.05f;

    public enum FlexAxis { X, Y, Z }

    float rawFlexInput;
    float smoothedFlex;
    bool isIdle = true;
    bool overheadHeld;

    // FUTURO: cuando implementemos el punch, este flag lo pondra a true
    // el SCR_PunchAbility. Mientras este a true, el brazo no hace nada
    // (ni flex, ni overhead, ni idle). El overhead se cancela por brazo.
    bool punchLocked = false;

    public float CurrentFlexAmount => smoothedFlex;
    public bool IsOverhead => overheadHeld && !punchLocked;
    public bool IsPunchLocked => punchLocked;

    void OnEnable()
    {
        if (isLeftArm)
            input.OnFlexLeftArm += HandleFlexInput;
        else
            input.OnFlexRightArm += HandleFlexInput;

        input.OnOverheadPressed += HandleOverheadPressed;
        input.OnOverheadReleased += HandleOverheadReleased;
    }

    void OnDisable()
    {
        if (isLeftArm)
            input.OnFlexLeftArm -= HandleFlexInput;
        else
            input.OnFlexRightArm -= HandleFlexInput;

        input.OnOverheadPressed -= HandleOverheadPressed;
        input.OnOverheadReleased -= HandleOverheadReleased;
    }

    void HandleFlexInput(float value)
    {
        rawFlexInput = Mathf.Clamp01(value);
    }

    void HandleOverheadPressed() => overheadHeld = true;
    void HandleOverheadReleased() => overheadHeld = false;

    // API publica para que el futuro SCR_PunchAbility bloquee/desbloquee este brazo.
    public void SetPunchLocked(bool locked)
    {
        punchLocked = locked;
        if (locked)
        {
            // Si entra en punch, cancela el overhead de ESTE brazo.
            overheadHeld = false;
        }
    }

    void FixedUpdate()
    {
        // El smoothedFlex SIEMPRE se actualiza, incluso si estamos en overhead o punchLocked.
        // Asi, al soltar el overhead, el brazo vuelve DIRECTAMENTE a la pose del gatillo
        // (que ya esta actualizada), sin pasar por idle.
        smoothedFlex = Mathf.MoveTowards(smoothedFlex, rawFlexInput, flexSmoothSpeed * Time.fixedDeltaTime);

        // Si este brazo esta bloqueado por punch, no hace nada.
        if (punchLocked) return;

        // --- OVERHEAD (gana a todo lo demas) ---
        if (overheadHeld)
        {
            EnsureDriveActive();
            shoulderJoint.targetRotation = BuildTarget(overheadAngle);
            return;
        }

        // --- COMPORTAMIENTO NORMAL (idle vs flex) ---
        bool shouldBeIdle = smoothedFlex <= idleThreshold;
        if (shouldBeIdle != isIdle)
        {
            isIdle = shouldBeIdle;
            ApplyDriveSettings(isIdle);
        }

        if (isIdle) return;

        float angle = smoothedFlex * maxFlexAngle;
        shoulderJoint.targetRotation = BuildTarget(angle);
    }

    // Construye la rotacion objetivo en el eje configurado.
    Quaternion BuildTarget(float angle)
    {
        return flexAxis switch
        {
            FlexAxis.X => Quaternion.Euler(angle, 0f, 0f),
            FlexAxis.Y => Quaternion.Euler(0f, angle, 0f),
            FlexAxis.Z => Quaternion.Euler(0f, 0f, angle),
            _ => Quaternion.identity
        };
    }

    // Fuerza el drive activo (para overhead, que siempre quiere spring alto).
    void EnsureDriveActive()
    {
        if (!isIdle) return;
        isIdle = false;
        ApplyDriveSettings(false);
    }

    void ApplyDriveSettings(bool idle)
    {
        float spring = idle ? idleSpring : activeSpring;
        float damper = idle ? idleDamper : activeDamper;

        switch (flexAxis)
        {
            case FlexAxis.X:
                JointDrive xDrive = shoulderJoint.angularXDrive;
                xDrive.positionSpring = spring;
                xDrive.positionDamper = damper;
                shoulderJoint.angularXDrive = xDrive;
                break;

            case FlexAxis.Y:
            case FlexAxis.Z:
                JointDrive yzDrive = shoulderJoint.angularYZDrive;
                yzDrive.positionSpring = spring;
                yzDrive.positionDamper = damper;
                shoulderJoint.angularYZDrive = yzDrive;
                break;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (shoulderJoint == null) return;

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

    public void ForceRefresh()
    {
        // Re-evaluar el estado.
        bool shouldBeIdle = smoothedFlex <= idleThreshold;
        isIdle = shouldBeIdle;
        ApplyDriveSettings(isIdle);

        // Si no esta idle, escribir el targetRotation de flexion.
        if (!isIdle)
        {
            float angle = smoothedFlex * maxFlexAngle;
            shoulderJoint.targetRotation = BuildTarget(angle);
        }
    }
}