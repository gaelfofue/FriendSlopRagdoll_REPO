using Unity.VisualScripting;
using UnityEngine;

public class NewPlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody rb;
    [SerializeField] ConfigurableJoint mainJoint;
    [SerializeField] PlayerInputRelay inputRelay;
    [SerializeField] private MovementLock movementLock;
    [SerializeField] Animator anim;

    [Header("PunchDamage")]
    [SerializeField] int punchesToKnockout = 3;

    [Header("ControllerSettings")]
    [SerializeField] float maxSpeed = 3;

    [Header("States")]
    [SerializeField] bool isGrounded = false;
    public bool isActiveRagdoll = true;
    public bool isGrabingActive = false;
    public bool isPunching = false;

    // Store position String
    float startsSlerpPositionSpring = 0.0f;

    // Store punches received
    int punchesReceived = 0;

    // Timing
    float lastTimeBecameRagdoll = 0;

    // NewHandGrab
    NewHandGrab[] handGrabHandlers;

    // Recovery
    int revivePressCount = 0;
    const int revivePressesRequired = 15;
    const float reviveWaitTime = 3f;

    // Input
    Vector2 moveInputVector = Vector2.zero;
    bool isJumpButtonPressed = false;

    // Raycasts
    RaycastHit[] raycastHits = new RaycastHit[10];

    // Syncing of physics object
    SyncPhysicsObject[] syncPhysicsObjects;

    void OnEnable()
    {
        if (inputRelay == null) return;

        inputRelay.OnMove += HandleMoveInput;
        inputRelay.OnJumpPressed += HandleJumpInput;
        inputRelay.OnRevivePressed += HandleReviveInput;
        inputRelay.OnGrabBothPressed += HandleGrabPressed;
        inputRelay.OnGrabBothReleased += HandleGrabReleased;
    }

    void OnDisable()
    {
        if (inputRelay == null) return;

        inputRelay.OnMove -= HandleMoveInput;
        inputRelay.OnJumpPressed -= HandleJumpInput;
        inputRelay.OnRevivePressed -= HandleReviveInput;
        inputRelay.OnGrabBothPressed -= HandleGrabPressed;
        inputRelay.OnGrabBothReleased -= HandleGrabReleased;
    }

    void Awake()
    {
        syncPhysicsObjects = GetComponentsInChildren<SyncPhysicsObject>();

        handGrabHandlers = GetComponentsInChildren<NewHandGrab>();

        // Obtener el MovementLock del propio jugador.
        movementLock = GetComponent<MovementLock>();

        if (movementLock == null)
        {
            Debug.LogError("[NewPlayerController] No se encuentra MovementLock en " + gameObject.name);
        }
    }

    void Start()
    {
        startsSlerpPositionSpring = mainJoint.slerpDrive.positionSpring;
    }

    void FixedUpdate()
    {
        // Comprobar si el movimiento está bloqueado.
        if (movementLock != null && movementLock.IsLocked)
        {
            moveInputVector = Vector2.zero;
            isJumpButtonPressed = false;

            // Eliminar la inercia horizontal.
            Vector3 velocity = rb.linearVelocity;
            velocity.x = 0f;
            velocity.z = 0f;
            rb.linearVelocity = velocity;

            return;
        }

        // Assume that we are not grounded
        isGrounded = false;

        // Check if we ARE grounded
        int numberOfHits = Physics.SphereCastNonAlloc(
            rb.position,
            0.1f,
            transform.up * -1,
            raycastHits,
            0.5f
        );

        // Check for valid results
        for (int i = 0; i < numberOfHits; i++)
        {
            // Ignore self hits
            if (raycastHits[i].transform.root == transform)
                continue;

            isGrounded = true;
            break;
        }

        // Apply extra gravity
        if (!isGrounded)
            rb.AddForce(Vector2.down * 10);

        float inputMagnitude = moveInputVector.magnitude;

        Vector3 localVelocityVsForward = transform.forward * Vector3.Dot(transform.forward, rb.linearVelocity);

        float localForwardVelocity = localVelocityVsForward.magnitude;

        // Update both hand grabbers
        for (int i = 0; i < handGrabHandlers.Length; i++)
        {
            handGrabHandlers[i].UpdateState();
        }

        if (isActiveRagdoll)
        {
            if (inputMagnitude != 0)
            {
                Quaternion desiredDirection = Quaternion.LookRotation(new Vector3(moveInputVector.x, 0, moveInputVector.y * -1), transform.up);

                // Rotate target towards direction
                mainJoint.targetRotation = Quaternion.RotateTowards(mainJoint.targetRotation, desiredDirection, Time.fixedDeltaTime * 300
                );

                if (localForwardVelocity < maxSpeed)
                {
                    // Move the character in the direction it is facing
                    rb.AddForce(transform.forward * inputMagnitude * 30);
                }
            }

            if (isGrounded && isJumpButtonPressed)
            {
                rb.AddForce(Vector3.up * 20, ForceMode.Impulse);

                isJumpButtonPressed = false;
            }

            anim.SetFloat("movementSpeed", localForwardVelocity * 0.4f);

            // Update the joints rotation based on the animations
            for (int i = 0; i < syncPhysicsObjects.Length; i++)
            {
                syncPhysicsObjects[i].UpdateJointFromAnimation();
            }
        }
        else
        {
            // Check if the player has pressed enough buttons
            // and if the minimum ragdoll time has passed
            if (revivePressCount >= revivePressesRequired &&
                Time.time - lastTimeBecameRagdoll >= reviveWaitTime)
            {
                MakeActiveRagdoll();
            }
        }
    }

    public void OnPlayerBodyPartHit()
    {
        if (!isActiveRagdoll)
            return;

        MakeRagdoll();
    }

    void MakeRagdoll()
    {
        // Update main joint
        JointDrive jointDrive = mainJoint.slerpDrive;
        jointDrive.positionSpring = 0;
        mainJoint.slerpDrive = jointDrive;

        // Update the joints rotation
        for (int i = 0; i < syncPhysicsObjects.Length; i++)
        {
            syncPhysicsObjects[i].MakeRagdoll();
        }

        lastTimeBecameRagdoll = Time.time;

        revivePressCount = 0;

        isActiveRagdoll = false;
        isGrabingActive = false;
        isPunching = false;
    }

    void MakeActiveRagdoll()
    {
        JointDrive jointDrive = mainJoint.slerpDrive;
        jointDrive.positionSpring = startsSlerpPositionSpring;
        mainJoint.slerpDrive = jointDrive;

        // Update the joints rotation
        for (int i = 0; i < syncPhysicsObjects.Length; i++)
        {
            syncPhysicsObjects[i].MakeActiveRagdoll();
        }

        revivePressCount = 0;

        isActiveRagdoll = true;
        isGrabingActive = false;
        isPunching = false;
        punchesReceived = 0;
    }

    public void RecievePunchHit(Rigidbody hitbody, Vector3 direction, float impulse)
    {
        if (!isActiveRagdoll)
            return;

        if (hitbody != null)
        {
            hitbody.AddForce(direction.normalized * impulse, ForceMode.Impulse);
        }

        punchesReceived++;

        Debug.Log("Hits taken" + punchesReceived);

        if (punchesReceived >= punchesToKnockout)
        {
            punchesReceived = 0;
            MakeRagdoll();
        }
    }

    void HandleMoveInput(Vector2 input)
    {
        if (movementLock != null && movementLock.IsLocked)
        {
            moveInputVector = Vector2.zero;
            return;
        }

        moveInputVector = input;
    }

    void HandleJumpInput()
    {
        if (movementLock != null && movementLock.IsLocked)
            return;

        isJumpButtonPressed = true;
    }

    void HandleReviveInput()
    {
        if (movementLock != null && movementLock.IsLocked)
            return;

        if (isActiveRagdoll)
            return;

        revivePressCount++;

        Debug.Log("Recovery progress: " + revivePressCount + "/" + revivePressesRequired);
    }

    void HandleGrabPressed()
    {
        if (movementLock != null && movementLock.IsLocked)
            return;

        if (!isActiveRagdoll)
            return;

        isGrabingActive = true;
    }

    void HandleGrabReleased()
    {
        isGrabingActive = false;
    }
}
