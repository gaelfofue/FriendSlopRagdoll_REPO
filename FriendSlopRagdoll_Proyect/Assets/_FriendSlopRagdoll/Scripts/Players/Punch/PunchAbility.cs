using UnityEngine;

// Logica de golpe de UNA mano. Cada brazo tiene su propia instancia.
// Fases: Windup -> Strike -> Recovery -> Cooldown -> Idle.
// Durante el golpe, el brazo queda en punchLocked (no puede agarrar ni hacer overhead).
// La deteccion de impacto usa el BoxCollider del puno como trigger durante el Strike.
public class PunchAbility : MonoBehaviour
{
    [Header("References")]
    [SerializeField] ConfigurableJoint shoulderJoint;
    [SerializeField] ConfigurableJoint elbowJoint;
    [SerializeField] BoxCollider handCollider;
    [SerializeField] ArmController armController;
    [SerializeField] GrabCoordinator grabCoordinator;
    [SerializeField] PlayerInputRelay input;

    [Tooltip("FUTURO: si se asigna, el golpe tambien controlara la muneca. " +
             "Por ahora dejalo vacio.")]
    [SerializeField] ConfigurableJoint wristJoint;

    [Header("Arm Identity")]
    [SerializeField] bool isLeftArm;

    [Header("Fases (duraciones)")]
    [SerializeField] float windupDuration = 0.15f;
    [SerializeField] float strikeDuration = 0.1f;
    [SerializeField] float recoveryDuration = 0.25f;
    [SerializeField] float cooldownDuration = 0.6f;

    [Header("Angulos del golpe")]
    [Tooltip("Angulo del hombro durante el windup (recogido). " +
             "90 suele ser un punetazo recto.")]
    [SerializeField] float windupShoulderAngle = 90f;

    [Tooltip("Angulo del codo durante el windup (flexionado). " +
             "90 = codo doblado a 90 grados.")]
    [SerializeField] float windupElbowAngle = 90f;

    [Tooltip("Angulo del codo durante el strike (extendido). " +
             "0 = codo recto, puno hacia adelante.")]
    [SerializeField] float strikeElbowAngle = 0f;

    [Tooltip("Angulo del hombro durante el strike. " +
             "Suele ser el mismo que el windup para un punetazo recto.")]
    [SerializeField] float strikeShoulderAngle = 90f;

    [Header("Eje de rotacion del hombro y codo")]
    [Tooltip("Eje del hombro en el que se aplica el angulo. " +
             "Debe coincidir con el eje de flexion del brazo.")]
    [SerializeField] FlexAxis shoulderAxis = FlexAxis.X;

    [Tooltip("Eje del codo en el que se aplica el angulo.")]
    [SerializeField] FlexAxis elbowAxis = FlexAxis.X;

    [Header("Impacto")]
    [SerializeField] float punchForce = 15f;
    [SerializeField] float punchDamage = 1f;
    [SerializeField] float knockbackForce = 10f;
    [SerializeField] float stunDuration = 2f;
    [SerializeField] LayerMask hittableMask;

    [Header("Debug")]
    [SerializeField] bool debugLogs = true;

    [Header("Liberacion del codo durante el golpe")]
    [Tooltip("Si esta marcado, el codo se libera (Angular X/Y/Z Motion = Free) " +
         "durante el golpe, y se vuelve a bloquear (Limited con limite 0) al terminar. " +
         "Necesario si el ArmController tiene el codo bloqueado.")]
    [SerializeField] bool freeElbowDuringPunch = true;

    [Tooltip("Limite original del codo (para restaurarlo al terminar el golpe). " +
             "Suele ser 0 si el ArmController lo tenia bloqueado.")]
    [SerializeField] float originalElbowLimit = 0f;

    // Guardamos los Motion originales del codo para restaurarlos.
    JointProjectionMode originalElbowProjection;
    ConfigurableJointMotion originalElbowXMotion;
    ConfigurableJointMotion originalElbowYMotion;
    ConfigurableJointMotion originalElbowZMotion;
    SoftJointLimit originalElbowXLimit;
    SoftJointLimit originalElbowYLimit;
    SoftJointLimit originalElbowZLimit;

    public enum FlexAxis { X, Y, Z }

    public enum Phase { Idle, Windup, Strike, Recovery, Cooldown }
    Phase phase = Phase.Idle;
    float phaseTimer;

    bool punchRequested;
    bool hitAppliedThisStrike;

    // Para restaurar el collider despues del strike.
    bool originalIsTrigger;
    PhysicsMaterial originalMaterial;

    public bool IsPunching => phase != Phase.Idle && phase != Phase.Cooldown;
    public Phase CurrentPhase => phase;

    void Awake()
    {
        // Guardar el estado original del codo.
        if (elbowJoint != null)
        {
            originalElbowProjection = elbowJoint.projectionMode;
            originalElbowXMotion = elbowJoint.angularXMotion;
            originalElbowYMotion = elbowJoint.angularYMotion;
            originalElbowZMotion = elbowJoint.angularZMotion;
            originalElbowYLimit = elbowJoint.angularYLimit;
            originalElbowZLimit = elbowJoint.angularZLimit;
        }
    }
    void OnEnable()
    {
        if (input == null) return;
        if (isLeftArm) input.OnPunchLeftPressed += HandlePunchInput;
        else input.OnPunchRightPressed += HandlePunchInput;

        // Guardar el estado original del collider.
        if (handCollider != null)
        {
            originalIsTrigger = handCollider.isTrigger;
            originalMaterial = handCollider.sharedMaterial;
        }
    }

    void OnDisable()
    {
        if (input == null) return;
        if (isLeftArm) input.OnPunchLeftPressed -= HandlePunchInput;
        else input.OnPunchRightPressed -= HandlePunchInput;

        // Restaurar collider por si acaso.
        if (handCollider != null)
        {
            handCollider.isTrigger = originalIsTrigger;
            handCollider.sharedMaterial = originalMaterial;
        }
    }

    void HandlePunchInput()
    {
        punchRequested = true;
    }

    void FixedUpdate()
    {
        if (phase == Phase.Idle && punchRequested)
        {
            StartWindup();
        }

        punchRequested = false;

        switch (phase)
        {
            case Phase.Windup: TickWindup(); break;
            case Phase.Strike: TickStrike(); break;
            case Phase.Recovery: TickRecovery(); break;
            case Phase.Cooldown: TickCooldown(); break;
        }
    }

    void StartWindup()
    {
        phase = Phase.Windup;
        phaseTimer = 0f;
        hitAppliedThisStrike = false;

        armController?.SetPunchLocked(true);
        grabCoordinator?.CancelGrab();

        // Liberar el codo para poder doblarlo.
        if (freeElbowDuringPunch && elbowJoint != null)
            SetElbowMotion(ConfigurableJointMotion.Free);

        if (debugLogs) Debug.Log($"[Punch {(isLeftArm ? "L" : "R")}] Windup start");
    }

    void TickWindup()
    {
        phaseTimer += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(phaseTimer / windupDuration);

        // Interpolar hombro y codo desde su pose actual hasta la pose de windup.
        SetJointAngle(shoulderJoint, shoulderAxis, windupShoulderAngle);
        SetJointAngle(elbowJoint, elbowAxis, windupElbowAngle);

        if (phaseTimer >= windupDuration)
        {
            phase = Phase.Strike;
            phaseTimer = 0f;

            // Activar el collider del pu�o como trigger para detectar impacto.
            if (handCollider != null)
            {
                handCollider.isTrigger = true;
            }

            if (debugLogs) Debug.Log($"[Punch {(isLeftArm ? "L" : "R")}] Strike start");
        }
    }

    void TickStrike()
    {
        phaseTimer += Time.fixedDeltaTime;

        // Extender el codo bruscamente.
        SetJointAngle(elbowJoint, elbowAxis, strikeElbowAngle);
        SetJointAngle(shoulderJoint, shoulderAxis, strikeShoulderAngle);

        // Detectar impacto durante el strike (una vez por strike).
        if (!hitAppliedThisStrike)
            TryApplyHit();

        if (phaseTimer >= strikeDuration)
        {
            phase = Phase.Recovery;
            phaseTimer = 0f;

            // Restaurar el collider.
            if (handCollider != null)
            {
                handCollider.isTrigger = originalIsTrigger;
            }

            if (debugLogs) Debug.Log($"[Punch {(isLeftArm ? "L" : "R")}] Recovery start");
        }
    }

    void TickRecovery()
    {
        phaseTimer += Time.fixedDeltaTime;

        if (phaseTimer >= recoveryDuration)
        {
            // Restaurar el codo ANTES de soltar el punchLocked,
            // para que el ArmController lo encuentre en su estado original.
            if (freeElbowDuringPunch && elbowJoint != null)
                RestoreElbowMotion();

            // Soltar el punchLocked.
            armController?.SetPunchLocked(false);

            // CLAVE: forzar al ArmController a re-evaluar su estado.
            // Sin esto, el brazo se queda tieso en la pose del golpe.
            armController?.ForceRefresh();

            phase = Phase.Cooldown;
            phaseTimer = 0f;

            if (debugLogs) Debug.Log($"[Punch {(isLeftArm ? "L" : "R")}] Recovery end, cooldown start");
        }
    }

    void TickCooldown()
    {
        phaseTimer += Time.fixedDeltaTime;
        if (phaseTimer >= cooldownDuration)
        {
            phase = Phase.Idle;
            phaseTimer = 0f;
            if (debugLogs) Debug.Log($"[Punch {(isLeftArm ? "L" : "R")}] Cooldown end");
        }
    }

    void TryApplyHit()
    {
        if (handCollider == null) return;

        // OverlapBox alrededor del pu�o, en la direccion del golpe.
        Vector3 center = handCollider.bounds.center;
        Vector3 halfExtents = handCollider.bounds.extents;
        Collider[] hits = Physics.OverlapBox(center, halfExtents, handCollider.transform.rotation, hittableMask);

        foreach (var hit in hits)
        {
            // Ignorar el propio cuerpo.
            if (hit.transform.root == transform.root) continue;

            // Buscar SCR_Hittable en el collider o en sus padres.
            if (!hit.TryGetComponent(out Hittable hittable))
                hittable = hit.GetComponentInParent<Hittable>();

            if (hittable == null) continue;

            // Direccion del golpe: desde el hombro hacia adelante (o hacia el hit).
            Vector3 hitDir = (hit.transform.position - handCollider.transform.position).normalized;
            if (hitDir.sqrMagnitude < 0.01f) hitDir = transform.forward;

            hittable.ReceiveHit(hitDir, punchForce, punchDamage, knockbackForce, stunDuration);
            hitAppliedThisStrike = true;

            if (debugLogs) Debug.Log($"[Punch {(isLeftArm ? "L" : "R")}] Hit on {hit.name}");
            break;
        }
    }

    void SetJointAngle(ConfigurableJoint joint, FlexAxis axis, float angle)
    {
        if (joint == null) return;

        Quaternion target = axis switch
        {
            FlexAxis.X => Quaternion.Euler(angle, 0f, 0f),
            FlexAxis.Y => Quaternion.Euler(0f, angle, 0f),
            FlexAxis.Z => Quaternion.Euler(0f, 0f, angle),
            _ => Quaternion.identity
        };

        joint.targetRotation = target;

        // Asegurarnos de que el drive del joint esta activo (spring alto).
        EnsureDriveActive(joint, axis);
    }

    void EnsureDriveActive(ConfigurableJoint joint, FlexAxis axis)
    {
        if (axis == FlexAxis.X)
        {
            JointDrive d = joint.angularXDrive;
            if (d.positionSpring < 100f)
            {
                d.positionSpring = 800f;
                d.positionDamper = 40f;
                joint.angularXDrive = d;
            }
        }
        else
        {
            JointDrive d = joint.angularYZDrive;
            if (d.positionSpring < 100f)
            {
                d.positionSpring = 800f;
                d.positionDamper = 40f;
                joint.angularYZDrive = d;
            }
        }
    }

    void SetElbowMotion(ConfigurableJointMotion motion)
    {
        if (elbowJoint == null) return;

        elbowJoint.angularXMotion = motion;
        elbowJoint.angularYMotion = motion;
        elbowJoint.angularZMotion = motion;

        if (debugLogs) Debug.Log($"[Punch {(isLeftArm ? "L" : "R")}] Codo liberado: {motion}");
    }

    void RestoreElbowMotion()
    {
        if (elbowJoint == null) return;

        elbowJoint.angularXMotion = originalElbowXMotion;
        elbowJoint.angularYMotion = originalElbowYMotion;
        elbowJoint.angularZMotion = originalElbowZMotion;
        elbowJoint.angularYLimit = originalElbowYLimit;
        elbowJoint.angularZLimit = originalElbowZLimit;

        if (debugLogs) Debug.Log($"[Punch {(isLeftArm ? "L" : "R")}] Codo restaurado.");
    }
}