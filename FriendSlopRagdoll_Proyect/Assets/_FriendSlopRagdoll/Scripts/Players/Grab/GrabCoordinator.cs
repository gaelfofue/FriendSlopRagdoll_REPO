using UnityEngine;

public class GrabCoordinator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerInputRelay input;
    [SerializeField] HandGrabber leftHand;
    [SerializeField] HandGrabber rightHand;
    [SerializeField] SCR_ArmController leftArm;
    [SerializeField] SCR_ArmController rightArm;

    bool grabWanted;
    bool leftWasLocked;
    bool rightWasLocked;

    public bool GrabWanted => grabWanted;

    void OnEnable()
    {
        if (input != null)
            input.OnGrabBothPressed += HandleGrabPressed;
        // NO suscribimos OnGrabBothReleased
    }

    void OnDisable()
    {
        if (input != null)
            input.OnGrabBothPressed -= HandleGrabPressed;
    }

    void HandleGrabPressed()
    {
        grabWanted = !grabWanted; // toggle

        leftHand?.SetGrabWanted(grabWanted);
        rightHand?.SetGrabWanted(grabWanted);

        if (grabWanted)
        {
            // Empezar a agarrar: intentar en ambos brazos.
            TryGrabBoth();
        }
        else
        {
            // Soltar: liberar ambos brazos.
            leftHand?.Release();
            rightHand?.Release();
        }
    }

    void HandleGrabReleased()
    {
        grabWanted = false;

        leftHand?.SetGrabWanted(false);
        rightHand?.SetGrabWanted(false);

        leftHand?.Release();
        rightHand?.Release();
    }

    void TryGrabBoth()
    {
        if (leftHand != null && (leftArm == null || !leftArm.IsPunchLocked))
            leftHand.TryGrab();

        if (rightHand != null && (rightArm == null || !rightArm.IsPunchLocked))
            rightHand.TryGrab();
    }

    void Update()
    {
        // Reintentar el agarre en cada brazo cuando sale de punchLocked.
        if (leftArm != null && leftHand != null)
        {
            bool lockedNow = leftArm.IsPunchLocked;
            if (leftWasLocked && !lockedNow && grabWanted)
                leftHand.TryGrab();
            leftWasLocked = lockedNow;
        }

        if (rightArm != null && rightHand != null)
        {
            bool lockedNow = rightArm.IsPunchLocked;
            if (rightWasLocked && !lockedNow && grabWanted)
                rightHand.TryGrab();
            rightWasLocked = lockedNow;
        }
    }
}