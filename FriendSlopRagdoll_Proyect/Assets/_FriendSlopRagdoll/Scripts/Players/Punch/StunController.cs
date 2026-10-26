using System.Collections.Generic;
using UnityEngine;

// Gestiona el noqueo (stun) del personaje.
// - Pone todos los ConfigurableJoint a spring 0 (ragdoll pasivo).
// - Desactiva los scripts de control (arm, hand, punch).
// - Deja activo el SCR_PlayerController (el CoreBall sigue recibiendo input).
// - El jugador puede "despertar" antes espameando cualquier boton.
public class StunController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerInputRelay input;
    [SerializeField] SCR_PlayerController playerController;
    [SerializeField] List<ArmController> armControllers = new List<ArmController>();
    [SerializeField] List<HandGrabber> handGrabbers = new List<HandGrabber>();
    [SerializeField] List<PunchAbility> punchAbilities = new List<PunchAbility>();
    [SerializeField] GrabCoordinator grabCoordinator;

    [Header("Stun Settings")]
    [Tooltip("Duracion base del stun en segundos.")]
    [SerializeField] float baseStunDuration = 2f;

    [Tooltip("Cuanto reduce cada pulsacion de boton durante el stun. " +
             "0.05 = 50ms por pulsacion. A 2s, con 20 pulsaciones despiertas.")]
    [SerializeField] float spamRecoveryBonus = 0.05f;

    [Tooltip("Multiplicador de velocidad del CoreBall durante el stun. " +
             "1 = sin cambio. 0.5 = mitad de velocidad.")]
    [SerializeField] float speedMultiplierDuringStun = 1f;

    [Header("Debug")]
    [SerializeField] bool debugLogs = true;

    bool isStunned;
    float remainingStun;
    float originalMaxSpeed;
    float originalMoveForce;

    // Guardar los drives originales de cada joint para restaurarlos.
    struct JointSnapshot
    {
        public ConfigurableJoint joint;
        public JointDrive angularX;
        public JointDrive angularYZ;
    }
    List<JointSnapshot> jointSnapshots = new List<JointSnapshot>();

    public bool IsStunned => isStunned;

    void Awake()
    {
        // Cachear todos los ConfigurableJoint del ragdoll.
        var joints = GetComponentsInChildren<ConfigurableJoint>(true);
        foreach (var j in joints)
        {
            jointSnapshots.Add(new JointSnapshot
            {
                joint = j,
                angularX = j.angularXDrive,
                angularYZ = j.angularYZDrive
            });
        }

        // Cachear valores originales del PlayerController (si esta asignado).
        if (playerController != null)
        {
            // No tenemos acceso directo a maxSpeed/moveForce desde fuera,
            // asi que usamos reflexion... o mejor, exponemos propiedades publicas.
            // Por ahora, solo cambiamos el speedMultiplier via un metodo publico.
        }
    }

    void OnEnable()
    {
        if (input == null) return;

        // Suscribirse a TODOS los botones para detectar spam.
        input.OnJumpPressed += OnAnyButton;
        input.OnPunchLeftPressed += OnAnyButton;
        input.OnPunchRightPressed += OnAnyButton;
        input.OnGrabBothPressed += OnAnyButton;
        input.OnOverheadPressed += OnAnyButton;
        input.OnDivePressed += OnAnyButton;
    }

    void OnDisable()
    {
        if (input == null) return;

        input.OnJumpPressed -= OnAnyButton;
        input.OnPunchLeftPressed -= OnAnyButton;
        input.OnPunchRightPressed -= OnAnyButton;
        input.OnGrabBothPressed -= OnAnyButton;
        input.OnOverheadPressed -= OnAnyButton;
        input.OnDivePressed -= OnAnyButton;
    }

    void OnAnyButton()
    {
        if (!isStunned) return;
        remainingStun -= spamRecoveryBonus;
        if (remainingStun < 0f) remainingStun = 0f;
    }

    public void Stun(float duration)
    {
        if (isStunned) return; // ya estaba stuneado

        isStunned = true;
        remainingStun = duration > 0f ? duration : baseStunDuration;

        // Poner todos los joints en spring 0 (ragdoll pasivo).
        foreach (var snap in jointSnapshots)
        {
            JointDrive x = snap.angularX;
            x.positionSpring = 0f;
            x.positionDamper = 5f;
            snap.joint.angularXDrive = x;

            JointDrive yz = snap.angularYZ;
            yz.positionSpring = 0f;
            yz.positionDamper = 5f;
            snap.joint.angularYZDrive = yz;
        }

        // Desactivar scripts de control de brazos.
        foreach (var arm in armControllers) if (arm != null) arm.enabled = false;
        foreach (var hand in handGrabbers) if (hand != null) hand.enabled = false;
        foreach (var punch in punchAbilities) if (punch != null) punch.enabled = false;

        // Cancelar el agarre.
        grabCoordinator?.CancelGrab();

        // Aplicar el speedMultiplier al PlayerController (si esta asignado).
        if (playerController != null)
            playerController.SetSpeedMultiplier(speedMultiplierDuringStun);

        if (debugLogs) Debug.Log($"[Stun] Stunned for {remainingStun}s");
    }

    void FixedUpdate()
    {
        if (!isStunned) return;

        remainingStun -= Time.fixedDeltaTime;
        if (remainingStun <= 0f)
        {
            EndStun();
        }
    }

    void EndStun()
    {
        isStunned = false;
        remainingStun = 0f;

        // Restaurar los drives originales.
        foreach (var snap in jointSnapshots)
        {
            if (snap.joint == null) continue;
            snap.joint.angularXDrive = snap.angularX;
            snap.joint.angularYZDrive = snap.angularYZ;
        }

        // Reactivar scripts.
        foreach (var arm in armControllers) if (arm != null) arm.enabled = true;
        foreach (var hand in handGrabbers) if (hand != null) hand.enabled = true;
        foreach (var punch in punchAbilities) if (punch != null) punch.enabled = true;

        // Restaurar speedMultiplier.
        if (playerController != null)
            playerController.SetSpeedMultiplier(1f);

        if (debugLogs) Debug.Log("[Stun] Recovered");
    }
}