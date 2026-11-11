
using UnityEngine;

public class PlayerPunchController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator anim;
    [SerializeField] PlayerInputRelay inputRelay;
    [SerializeField] NewPlayerController player;

    [Header("Hands")]
    [SerializeField] NewHandGrab leftHand;
    [SerializeField] NewHandGrab rightHand;

    [Header("Punch Hitboxes")]
    [SerializeField] PunchHitbox leftHitbox;
    [SerializeField] PunchHitbox rightHitbox;

    [Header("Animator Triggers")]
    [SerializeField] string leftPunchTrigger = "PunchLeft";
    [SerializeField] string rightPunchTrigger = "PunchRight";

    bool isPunchingLeft;
    bool isPunchingRight;

    void Awake()
    {
        if (anim == null)
            anim = GetComponent<Animator>();

        if (player == null)
            player = GetComponentInParent<NewPlayerController>();

        if (inputRelay == null)
            inputRelay = player.GetComponent<PlayerInputRelay>();
    }

    void OnEnable()
    {
        inputRelay.OnPunchLeftPressed += PunchLeft;
        inputRelay.OnPunchRightPressed += PunchRight;
    }

    void OnDisable()
    {
        inputRelay.OnPunchLeftPressed -= PunchLeft;
        inputRelay.OnPunchRightPressed -= PunchRight;
    }

    void PunchLeft()
    {
        StartPunch(true);
    }

    void PunchRight()
    {
        StartPunch(false);
    }

    void StartPunch(bool left)
    {
        if (!player.isActiveRagdoll)
            return;

        // Por ahora no permitimos encadenar ni solapar puñetazos.
        if (isPunchingLeft || isPunchingRight)
            return;

        // Cancelar el agarre de ambas manos.
        player.isGrabingActive = false;

        leftHand.CancelGrabForPunch();
        rightHand.CancelGrabForPunch();

        player.isPunching = true;

        if (left)
        {
            isPunchingLeft = true;
            leftHand.SetPunching(true);
            anim.SetTrigger(leftPunchTrigger);
        }
        else
        {
            isPunchingRight = true;
            rightHand.SetPunching(true);
            anim.SetTrigger(rightPunchTrigger);
        }
    }

    // Animation Event: comienzo de la ventana de impacto izquierda.
    public void LeftPunchImpactStart()
    {
        if (isPunchingLeft)
            leftHitbox.BeginPunch();
    }

    // Animation Event: final de la ventana de impacto izquierda.
    public void LeftPunchImpactEnd()
    {
        leftHitbox.EndPunch();
    }

    // Animation Event: final de la animación izquierda.
    public void LeftPunchFinished()
    {
        leftHitbox.EndPunch();

        isPunchingLeft = false;
        leftHand.SetPunching(false);

        player.isPunching = false;
    }

    // Animation Event: comienzo de la ventana de impacto derecha.
    public void RightPunchImpactStart()
    {
        if (isPunchingRight)
            rightHitbox.BeginPunch();
    }

    // Animation Event: final de la ventana de impacto derecha.
    public void RightPunchImpactEnd()
    {
        rightHitbox.EndPunch();
    }

    // Animation Event: final de la animación derecha.
    public void RightPunchFinished()
    {
        rightHitbox.EndPunch();

        isPunchingRight = false;
        rightHand.SetPunching(false);

        player.isPunching = false;
    }
}