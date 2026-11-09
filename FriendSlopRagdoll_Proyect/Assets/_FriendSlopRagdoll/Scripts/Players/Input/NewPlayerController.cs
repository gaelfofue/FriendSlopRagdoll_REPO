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
    [SerializeField] bool isActiveRagdoll = true;

    //Store position String
    float startsSlerpPositionSpring = 0.0f;

    // Input
    Vector2 moveInputVector = Vector2.zero;
    bool isJumpButtonPressed = false;

    // Raycasts
    RaycastHit[] raycastHits = new RaycastHit[10];

    //Syncing of physics object
    SyncPhysicsObject[] syncPhysicsObjects;

    void Awake()
    {
        syncPhysicsObjects = GetComponentsInChildren<SyncPhysicsObject>();
    }

    void Start()
    {
        startsSlerpPositionSpring = mainJoint.slerpDrive.positionSpring;
    }

    void OnEnable()
    {
        if (inputRelay == null) return;

        inputRelay.OnMove += HandleMoveInput;
        inputRelay.OnJumpPressed += HandleJumpInput;
    }

    void OnDisable()
    {
        if (inputRelay == null) return;

        inputRelay.OnMove -= HandleMoveInput;
        inputRelay.OnJumpPressed -= HandleJumpInput;
    }

    void HandleMoveInput(Vector2 input)
    {
        moveInputVector = input;
    }

    void HandleJumpInput()
    {
        isJumpButtonPressed = true;
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

        Vector3 localVelocityVsForward = transform.forward * Vector3.Dot(transform.forward, rb.velocity);

        float localForwardVelocity = localVelocityVsForward.magnitude;

        if (inputMagnitude != 0)
        {
            Quaternion desiredDirection = Quaternion.LookRotation(
                new Vector3(moveInputVector.x, 0, moveInputVector.y * -1),
                transform.up
            );

            // Rotate target towards direction
            mainJoint.targetRotation = Quaternion.RotateTowards(
                mainJoint.targetRotation,
                desiredDirection,
                Time.fixedDeltaTime * 300
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

        //Update the joints rotation based on the animations
        for (int i = 0; i < syncPhysicsObjects.Length; i++)
        {
            syncPhysicsObjects[i].UpdateJointFromAnimation();
        }
    }

    void MakeRagdoll()
    {
        isActiveRagdoll = false;

        JointDrive jointDrive = mainJoint.slerpDrive;
        jointDrive.positionSpring = 0;
        mainJoint.slerpDrive = jointDrive;
    }

    void MakeActiveMakeRagdoll()
    {
        isActiveRagdoll = true;

        JointDrive jointDrive = mainJoint.slerpDrive;
        jointDrive.positionSpring = startsSlerpPositionSpring;
        mainJoint.slerpDrive = jointDrive;
    }
}