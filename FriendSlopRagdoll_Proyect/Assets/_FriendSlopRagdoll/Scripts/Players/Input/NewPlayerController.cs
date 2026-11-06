using UnityEngine;

public class NewPlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody rb;
    [SerializeField] ConfigurableJoint mainJoint;
    [SerializeField] PlayerInputRelay inputRelay;

    [Header("ControllerSettings")]
    [SerializeField] float maxSpeed = 3;
    [SerializeField] bool isGrounded = false;

    // Input
    Vector2 moveInputVector = Vector2.zero;
    bool isJumpButtonPressed = false;

    // Raycasts
    RaycastHit[] raycastHits = new RaycastHit[10];

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

            Vector3 localVelocityVsForward = transform.forward *
                Vector3.Dot(transform.forward, rb.velocity);

            float localForwardVelocity = localVelocityVsForward.magnitude;

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
    }
}