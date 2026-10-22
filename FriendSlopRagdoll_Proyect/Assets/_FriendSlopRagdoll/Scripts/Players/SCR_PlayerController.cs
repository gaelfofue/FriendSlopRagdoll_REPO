using UnityEngine;

public class SCR_PlayerController : MonoBehaviour
{
    public enum ModelForwardAxis
    {
        NegZ = 0,   // (0,0,-1) estandar de Unity
        PosZ = 1,   // (0,0, 1) de espaldas
        NegX = 2,   // (-1,0,0)  <-- el de este modelo
        PosX = 3    // ( 1,0,0)
    }

    #region GENERAL VARIABLES
    [Header("References")]
    [SerializeField] Rigidbody rb;
    [SerializeField] ConfigurableJoint mainJoint;

    [Tooltip("Rigidbody que gira (Torso). Si lo dejas vacio se coge del mainJoint.")]
    [SerializeField] Rigidbody torsoRigidbody;

    [SerializeField] PlayerInputRelay input;
    [SerializeField] MovementLock movementLock;

    [Header("MIRAR HACIA DONDE SE MUEVE")]
    [Tooltip("Hacia donde apunta la CARA del modelo, en espacio LOCAL del Rigidbody.\n" +
             "Este modelo tiene la cara mirando a -X, por eso esta en NegX (-1,0,0).\n" +
             "Si algun dia cambias el mesh, mira el HUD: 'MIRA' tiene que coincidir con 'DEBERIA'.")]
    [SerializeField] ModelForwardAxis modelForwardAxis = ModelForwardAxis.NegX;

    [Tooltip("Ajuste fino en grados por si queda un desfase pequeno (positivo = gira la referencia a la derecha).")]
    [SerializeField] float modelYawOffsetDeg = 0f;

    [Tooltip("Grados por segundo de giro por cada grado de error. 8 = a 30 grados de error gira a 240 grados/s.")]
    [SerializeField] float yawGain = 8f;

    [Tooltip("Velocidad de giro maxima en grados/segundo.")]
    [SerializeField] float maxYawSpeed = 720f;

    [Tooltip("Aceleracion del giro (grados/seg^2). Mas alto = arranca y frena mas seco.")]
    [SerializeField] float yawAcceleration = 2000f;

    [Tooltip("Error en grados por debajo del cual se considera ya orientado (evita temblores).")]
    [SerializeField] float yawDeadzone = 1.5f;

    [Tooltip("Anula los muelles del drive rotacional del ConfigurableJoint (slerpDrive / angularXDrive / " +
             "angularYZDrive). NO toca angularXMotion/angularYZMotion, asi que NO se cae al suelo. " +
             "Activalo solo si al soltar las teclas notas que algo lo devuelve a su direccion de reposo.")]
    [SerializeField] bool neutralizeJointRotationDrive = false;

    [Header("Controller Settings (SIN TOCAR)")]
    [SerializeField] float maxSpeed = 3f;
    [SerializeField] float moveForce = 30f;
    [SerializeField] float jumpForce = 20f;
    [SerializeField] float extraGravity = 10f;

    [Header("Ground Check")]
    [SerializeField] float groundCheckRadius = 0.1f;
    [SerializeField] float groundCheckDistance = 0.5f;
    [SerializeField] bool isGrounded;

    [Header("DEBUG")]
    [SerializeField] bool debugFacing = true;
    [SerializeField] float debugLogInterval = 0.5f;
    [Tooltip("Gizmos: VERDE = deberia mirar, AZUL = mira de verdad, AMARILLO = forward sin aplanar.")]
    [SerializeField] bool debugGizmos = true;

    [Header("Raycasts")]
    RaycastHit[] raycastHits = new RaycastHit[10];
    #endregion

    Vector2 moveInput;
    bool jumpRequested;
    Rigidbody turnBody;

    Vector3 dbgDesired = Vector3.zero;
    Vector3 dbgFlatForward = Vector3.zero;
    Vector3 dbgRawForward = Vector3.zero;
    float dbgError;
    float dbgYawDegPerSec;
    float debugNextLogTime;

    void Awake()
    {
        if (torsoRigidbody == null && mainJoint != null)
            torsoRigidbody = mainJoint.GetComponent<Rigidbody>();

        ResolveTurnBody();

        if (neutralizeJointRotationDrive && mainJoint != null)
        {
            JointDrive zero = mainJoint.slerpDrive;
            zero.positionSpring = 0f; zero.maximumForce = 0f; mainJoint.slerpDrive = zero;

            JointDrive zeroX = mainJoint.angularXDrive;
            zeroX.positionSpring = 0f; zeroX.maximumForce = 0f; mainJoint.angularXDrive = zeroX;

            JointDrive zeroYZ = mainJoint.angularYZDrive;
            zeroYZ.positionSpring = 0f; zeroYZ.maximumForce = 0f; mainJoint.angularYZDrive = zeroYZ;

            Debug.Log("[SCR_PlayerController] Muelles del drive rotacional anulados. Motion X/Z intactos.", this);
        }
    }

    void ResolveTurnBody()
    {
        turnBody = torsoRigidbody != null ? torsoRigidbody : rb;

        if (mainJoint != null && mainJoint.angularYMotion == ConfigurableJointMotion.Locked && rb != null)
            turnBody = rb;

        if (turnBody == null)
            Debug.LogError("[SCR_PlayerController] No hay Rigidbody para girar.", this);
    }

    void OnEnable()
    {
        if (input == null) return;
        input.OnMove += HandleMove;
        input.OnJumpPressed += HandleJump;
    }

    void OnDisable()
    {
        if (input == null) return;
        input.OnMove -= HandleMove;
        input.OnJumpPressed -= HandleJump;
    }

    void HandleMove(Vector2 v) => moveInput = v;
    void HandleJump() => jumpRequested = true;

    void FixedUpdate()
    {
        GroundCheck();
        ApplyExtraGravity();

        if (movementLock != null && movementLock.IsLocked)
        {
            FaceDirection(Vector3.zero, false);
            return;
        }

        Move();
        Jump();
    }

    void GroundCheck()
    {
        isGrounded = false;
        int numberOfHits = Physics.SphereCastNonAlloc(rb.position, groundCheckRadius, Vector3.down,
                                                      raycastHits, groundCheckDistance);
        for (int i = 0; i < numberOfHits; i++)
        {
            if (raycastHits[i].transform.root == transform) continue;
            isGrounded = true;
            break;
        }
    }

    void ApplyExtraGravity()
    {
        if (!isGrounded)
            rb.AddForce(Vector3.down * extraGravity);
    }

    void Move()
    {
        float inputMagnitude = moveInput.magnitude;
        Vector3 inputDirWorld = new Vector3(-moveInput.x, 0f, moveInput.y * -1f);
        bool hasInput = inputMagnitude > 0.001f;

        if (hasInput)
        {
            inputDirWorld = inputDirWorld.normalized;

            float speedTowardsInput = Vector3.Dot(rb.linearVelocity, inputDirWorld);
            if (speedTowardsInput < maxSpeed)
                rb.AddForce(inputDirWorld * inputMagnitude * moveForce);
        }

        FaceDirection(inputDirWorld, hasInput);
    }

    /// <summary>Forward del modelo en LOCAL, segun el eje elegido + ajuste fino.</summary>
    Vector3 GetModelForwardLocal()
    {
        Vector3 axis = modelForwardAxis switch
        {
            // OJO: en Unity Vector3.forward = (0,0,+1) y Vector3.back = (0,0,-1).
            // Uso vectores literales para que no haya duda.
            ModelForwardAxis.NegZ => new Vector3(0f, 0f, -1f), // estandar
            ModelForwardAxis.PosZ => new Vector3(0f, 0f, 1f),  // de espaldas
            ModelForwardAxis.PosX => new Vector3(1f, 0f, 0f),
            _ => new Vector3(-1f, 0f, 0f)                      // <-- este modelo
        };

        if (Mathf.Abs(modelYawOffsetDeg) > 0.001f)
            axis = Quaternion.Euler(0f, modelYawOffsetDeg, 0f) * axis;

        return axis;
    }

    void FaceDirection(Vector3 dir, bool hasInput)
    {
        if (turnBody == null || turnBody.isKinematic) return;

        // 1) Forward del modelo, aplanado en horizontal (inmune al tilt del joint)
        dbgRawForward = turnBody.rotation * GetModelForwardLocal();

        Vector3 flatForward = Vector3.ProjectOnPlane(dbgRawForward, Vector3.up);
        if (flatForward.sqrMagnitude < 0.0001f)
            flatForward = Vector3.ProjectOnPlane(turnBody.transform.forward, Vector3.up);
        if (flatForward.sqrMagnitude < 0.0001f) return; // torso tumbado: no forzamos nada
        flatForward.Normalize();
        dbgFlatForward = flatForward;

        // 2) Direccion deseada (cero si no hay input)
        dbgDesired = hasInput ? dir.normalized : Vector3.zero;

        // 3) Error de yaw: el unico angulo que cuenta
        dbgError = hasInput ? Vector3.SignedAngle(flatForward, dbgDesired, Vector3.up) : 0f;

        // 4) Control de velocidad angular en Y (nunca se queda a medias contra un muelle)
        dbgYawDegPerSec = Vector3.Dot(turnBody.angularVelocity, Vector3.up) * Mathf.Rad2Deg;

        float targetYaw = Mathf.Abs(dbgError) > yawDeadzone
            ? Mathf.Clamp(dbgError * yawGain, -maxYawSpeed, maxYawSpeed)
            : 0f;

        float newYaw = Mathf.MoveTowards(dbgYawDegPerSec, targetYaw, yawAcceleration * Time.fixedDeltaTime);

        if (!Mathf.Approximately(newYaw, dbgYawDegPerSec))
        {
            Vector3 av = turnBody.angularVelocity;
            av += Vector3.up * ((newYaw - dbgYawDegPerSec) * Mathf.Deg2Rad);
            turnBody.angularVelocity = av;
        }
    }

void Jump()
    {
        if (!isGrounded || !jumpRequested) return;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        jumpRequested = false;
    }
}



