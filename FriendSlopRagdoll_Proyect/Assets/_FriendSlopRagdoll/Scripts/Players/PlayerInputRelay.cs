using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputRelay : MonoBehaviour
{
    public event Action<Vector2> OnMove;
    public event Action OnJumpPressed;
    public event Action OnDivePressed;

    // Agarre individual (L1/R1) - por si acaso, no se usa por ahora
    public event Action OnGrabLeftPressed;
    public event Action OnGrabLeftReleased;
    public event Action OnGrabRightPressed;
    public event Action OnGrabRightReleased;

    // NUEVO: agarre global (B) - agarra ambos brazos a la vez
    public event Action OnGrabBothPressed;
    public event Action OnGrabBothReleased;

    public event Action OnPunchLeftPressed;
    public event Action OnPunchRightPressed;

    public event Action<float> OnFlexLeftArm;
    public event Action<float> OnFlexRightArm;

    public event Action OnOverheadPressed;
    public event Action OnOverheadReleased;

    public void OnMoveInput(InputAction.CallbackContext ctx) => OnMove?.Invoke(ctx.ReadValue<Vector2>());

    public void OnJumpInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnJumpPressed?.Invoke();
    }

    public void OnDiveInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnDivePressed?.Invoke();
    }

    public void OnGrabLeftInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnGrabLeftPressed?.Invoke();
        else if (ctx.canceled) OnGrabLeftReleased?.Invoke();
    }

    public void OnGrabRightInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnGrabRightPressed?.Invoke();
        else if (ctx.canceled) OnGrabRightReleased?.Invoke();
    }
    public void OnGrabBothInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnGrabBothPressed?.Invoke();
        else if (ctx.canceled) OnGrabBothReleased?.Invoke();
    }

    public void OnPunchLeftInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnPunchLeftPressed?.Invoke();
    }

    public void OnPunchRightInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnPunchRightPressed?.Invoke();
    }

    public void OnFlexLeftArmInput(InputAction.CallbackContext ctx) => OnFlexLeftArm?.Invoke(ctx.ReadValue<float>());
    public void OnFlexRightArmInput(InputAction.CallbackContext ctx) => OnFlexRightArm?.Invoke(ctx.ReadValue<float>());

    public void OnOverheadInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnOverheadPressed?.Invoke();
        else if (ctx.canceled) OnOverheadReleased?.Invoke();
    }
}