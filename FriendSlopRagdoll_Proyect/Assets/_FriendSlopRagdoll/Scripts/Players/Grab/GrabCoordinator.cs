using UnityEngine;

public class GrabCoordinator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerInputRelay input;
    [SerializeField] HandGrabber leftHand;
    [SerializeField] HandGrabber rightHand;
    [SerializeField] ArmController leftArm;
    [SerializeField] ArmController rightArm;

    bool grabWanted;
    bool leftWasLocked;
    bool rightWasLocked;

    public bool GrabWanted => grabWanted;

    void OnEnable()
    {
        if (input != null)
            input.OnGrabBothPressed += HandleGrabToggle;
    }

    void OnDisable()
    {
        if (input != null)
            input.OnGrabBothPressed -= HandleGrabToggle;
    }

    void HandleGrabToggle()
    {
        grabWanted = !grabWanted;

        leftHand?.SetGrabWanted(grabWanted);
        rightHand?.SetGrabWanted(grabWanted);

        if (grabWanted)
        {
            TryGrabBoth();
        }
        else
        {
            leftHand?.Release();
            rightHand?.Release();
        }
    }

    // NUEVO: lo llamara el SCR_PunchAbility cuando el jugador pulse L1 o R1.
    // Cancela el agarre en AMBOS brazos (opcion b).
    public void CancelGrab()
    {
        if (!grabWanted) return; // si ya estaba OFF, no hay nada que cancelar

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