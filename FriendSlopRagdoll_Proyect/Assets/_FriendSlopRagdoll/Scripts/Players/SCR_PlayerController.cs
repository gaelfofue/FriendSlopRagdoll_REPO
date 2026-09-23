using UnityEngine;
using UnityEngine.InputSystem;

public class SCR_PlayerController : MonoBehaviour
{
    #region GENERAL VARIABLES
    [Header("References")]
    [SerializeField] Rigidbody rb; //Ref al Rigidbody del personaje
    [SerializeField] ConfigurableJoint mainJoint; //Ref al joint que controla la rotacion del personaje

    [Header("Controller Settings")]
    [SerializeField] float maxSpeed = 3f; //Velocidad maxima horizontal del personaje
    [SerializeField] float moveForce = 30f; //Fuerza aplicada al moverse
    [SerializeField] float jumpForce = 20f; //Fuerza del salto
    [SerializeField] float extraGravity = 10f; //Gravedad extra aplicada cuando no esta grounded (Menos floaty)
    [SerializeField] float rotationSpeed = 300f; //Velocidad de rotacion del joint hacia la direccion deseada

    [Header("Ground Check")]
    [SerializeField] float groundCheckRadius = 0.1f; //Radio del SphereCast de deteccion de suelo
    [SerializeField] float groundCheckDistance = 0.5f; //Distancia del SphereCast de deteccion de suelo
    [SerializeField] bool isGrounded; //Determina si el personaje esta tocando el suelo

    [Header("Raycasts")]
    RaycastHit[] raycastHits = new RaycastHit[10]; //Buffer de resultados del SphereCastNonAlloc
    #endregion

    // Variables de input
    Vector2 moveInput;
    bool isJumpButtonPressed;

    void FixedUpdate()
    {
        GroundCheck();
        ApplyExtraGravity();
        Move();
        Jump();
    }

    void GroundCheck()
    {
        //Asumimos que no estamos grounded
        isGrounded = false;

        //Comprobamos si estamos grounded
        int numberOfHits = Physics.SphereCastNonAlloc(rb.position, groundCheckRadius, transform.up * -1, raycastHits, groundCheckDistance);

        //Revisamos resultados validos
        for (int i = 0; i < numberOfHits; i++)
        {
            //Ignoramos los hits contra nosotros mismos
            if (raycastHits[i].transform.root == transform)
                continue;

            isGrounded = true;
            break;
        }
    }

    void ApplyExtraGravity()
    {
        //Aplicamos gravedad extra al personaje para hacerlo menos floaty
        if (!isGrounded)
            rb.AddForce(Vector3.down * extraGravity);
    }

    void Move()
    {
        float inputMagnitude = moveInput.magnitude;

        if (inputMagnitude == 0) return;

        //Direccion deseada segun el input
        Quaternion desiredDirection = Quaternion.LookRotation(new Vector3(moveInput.x, 0, moveInput.y * -1), transform.up);

        //Rotamos el target del joint hacia la direccion deseada
        mainJoint.targetRotation = Quaternion.RotateTowards(mainJoint.targetRotation, desiredDirection, Time.fixedDeltaTime * rotationSpeed);

        //Calculamos la velocidad local hacia delante
        Vector3 localVelocityVsFoward = transform.forward * Vector3.Dot(transform.forward, rb.linearVelocity);
        float localFowardVelocity = localVelocityVsFoward.magnitude;

        //Solo movemos si no hemos superado la velocidad maxima
        if (localFowardVelocity < maxSpeed)
        {
            rb.AddForce(transform.forward * inputMagnitude * moveForce);
        }
    }

    void Jump()
    {
        if (!isGrounded || !isJumpButtonPressed) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isJumpButtonPressed = false;
    }

    #region INPUT METHODS
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        Debug.Log("OnMove: " + moveInput);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed) isJumpButtonPressed = true;
        Debug.Log("OnJump: " + context.phase);
    }
    #endregion
}