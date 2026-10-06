using UnityEngine;

// Marca cualquier parte del cuerpo/objeto como agarrable.
[RequireComponent (typeof(Rigidbody))]

public class Grabbable : MonoBehaviour
{
    public Rigidbody Rigidbody {  get; private set; }
    public bool IsHeld { get; private set; }

    void Awake() => Rigidbody = GetComponent<Rigidbody>();
    public void SetHeld(bool held) => IsHeld = held;
}
