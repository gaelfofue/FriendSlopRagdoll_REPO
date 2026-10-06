using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Unico punto de entrada del Input system. Asi esto funciona por separado de las fisicas
// Si esto se rompe, solo se rompe el input y nada mas

public class PlayerInputRelay : MonoBehaviour
{
    public event Action<Vector2> OnMove;
    public event Action OnJumpPressed;
    public event Action OnDivePressed;

    public event Action OnGrabLeftPressed;
    public event Action OnGrabLeftReleased;
    public event Action OnGrabRightPressed;
    public event Action OnGrabRightReleased;

    public event Action OnPunchLeftPressed;
    public event Action OnPunchRightPressed;

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
        else if (ctx.performed) OnGrabRightReleased?.Invoke();
    }

    public void OnPunchLeftInput (InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnPunchLeftPressed?.Invoke();
    }
    public void OnPunchRightInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnPunchRightPressed?.Invoke();
    }
}
