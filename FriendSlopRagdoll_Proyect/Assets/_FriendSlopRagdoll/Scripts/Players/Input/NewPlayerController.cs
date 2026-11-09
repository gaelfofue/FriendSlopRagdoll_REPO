
using Unity.VisualScripting;
using UnityEngine;

public class NewPlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody rb;
    [SerializeField] ConfigurableJoint mainJoint;
    [SerializeField] PlayerInputRelay inputRelay;
    [SerializeField] Animator anim;

    [Header("ControllerSettings")]
    [SerializeField] float maxSpeed = 3;

    [Header("States")]
    [SerializeField] bool isGrounded = false;
    public bool isActiveRagdoll = true;

    // Store position String
    float startsSlerpPositionSpring = 0.0f;

    // Timing
    float lastTimeBecameRagdoll = 0;

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
    }

    void OnDisable()
    {
        if (inputRelay == null) return;

        inputRelay.OnMove -= HandleMoveInput;
        inputRelay.OnJumpPressed -= HandleJumpInput;
        inputRelay.OnRevivePressed -= HandleReviveInput;
    }

    void Awake()
    {
        syncPhysicsObjects = GetComponentsInChildren<SyncPhysicsObject>();
    }

    void Start()
    {
        startsSlerpPositionSpring = mainJoint.slerpDrive.positionSpring;
    }

    void FixedUpdate()
    {
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
    }

    void HandleMoveInput(Vector2 input)
    {
        moveInputVector = input;
    }

    void HandleJumpInput()
    {
        isJumpButtonPressed = true;
    }

    void HandleReviveInput()
    {
        if (isActiveRagdoll)
            return;

        revivePressCount++;

        Debug.Log("Recovery progress: " + revivePressCount + "/" + revivePressesRequired);
    }
}
