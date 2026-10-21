using UnityEngine;

public class SCR_PlayerController : MonoBehaviour
{
    #region GENERAL VARIABLES
    [Header("References")]
    [SerializeField] Rigidbody rb;
    [SerializeField] ConfigurableJoint mainJoint;

    [Tooltip("Rigidbody del Torso. Es el que gira con el torque. " +
             "Si lo dejas vacío, se busca en el hijo del mainJoint automaticamente.")]
    [SerializeField] Rigidbody torsoRigidbody;

    [SerializeField] PlayerInputRelay input;
    [SerializeField] MovementLock movementLock;

    [Header("Rotation")]
    [Tooltip("Cuanto torque aplicar por grado de diferencia entre el Torso y la direccion deseada. " +
             "Empieza con 5 y ajusta a ojo.")]
    [SerializeField] float rotationTorqueFactor = 5f;

    [Tooltip("Si el Torso gira hacia el lado contrario, pon esto en -1.")]
    [SerializeField] float rotationTorqueSign = 1f;

    [Header("Controller Settings")]
    [SerializeField] float maxSpeed = 3f;
    [SerializeField] float moveForce = 30f;
    [SerializeField] float jumpForce = 20f;
    [SerializeField] float extraGravity = 10f;

    [Header("Ground Check")]
    [SerializeField] float groundCheckRadius = 0.1f;
    [SerializeField] float groundCheckDistance = 0.5f;
    [SerializeField] bool isGrounded;

    [Header("Raycasts")]
    RaycastHit[] raycastHits = new RaycastHit[10];
    #endregion

    Vector2 moveInput;
    bool jumpRequested;

    void Awake()
    {
        // Auto-asignar torsoRigidbody si no se ha puesto en el Inspector.
        // El Torso es el hijo del mainJoint (connectedBody es el CoreBall, y el Torso es el transform del joint).
        if (torsoRigidbody == null && mainJoint != null)
            torsoRigidbody = mainJoint.GetComponent<Rigidbody>();
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

        if (movementLock != null && movementLock.IsLocked) return;

        Move();
        Jump();
    }

    void GroundCheck()
    {
        isGrounded = false;
        int numberOfHits = Physics.SphereCastNonAlloc(rb.position, groundCheckRadius, transform.up * -1, raycastHits, groundCheckDistance);
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
        if (inputMagnitude == 0) return;

        Vector3 inputDirWorld = new Vector3(-moveInput.x, 0f, moveInput.y * -1f).normalized;

        // --- ROTACION POR TORQUE DIRECTO ---
        if (torsoRigidbody != null)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(inputDirWorld, Vector3.up);
            Quaternion deltaRotation = desiredRotation * Quaternion.Inverse(torsoRigidbody.rotation);

            deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f) angle -= 360f;

            // Nos quedamos solo con la componente Y (yaw)
            float yawAngle = angle * (axis.y >= 0f ? 1f : -1f);

            // Aplicamos torque en Y mundial (con signo ajustable por si va al reves)
            Vector3 torque = Vector3.up * (yawAngle * rotationTorqueFactor * rotationTorqueSign);
            torsoRigidbody.AddTorque(torque, ForceMode.Acceleration);
        }

        // --- FUERZA ---
        float speedTowardsInput = Vector3.Dot(rb.linearVelocity, inputDirWorld);
        if (speedTowardsInput < maxSpeed)
            rb.AddForce(inputDirWorld * inputMagnitude * moveForce);
    }

    void Jump()
    {
        if (!isGrounded || !jumpRequested) return;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        jumpRequested = false;
    }
}