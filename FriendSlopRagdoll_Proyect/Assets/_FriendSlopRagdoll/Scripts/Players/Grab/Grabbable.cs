using UnityEngine;

// Marca cualquier parte del cuerpo/objeto como agarrable.
[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour
{
    public Rigidbody Rigidbody { get; private set; }
    public bool IsHeld { get; private set; }

    [Header("Break Settings")]
    [Tooltip("Multiplicador del breakForce del HandGrabber. " +
             "1 = normal, <1 = mas facil de romper, >1 = mas dificil.")]
    [SerializeField] float breakForceMultiplier = 1f;

    [Tooltip("Si esta marcado, este Grabbable es un BRAZO de jugador. " +
             "Los agarres brazo-a-brazo NO se rompen por fuerza " +
             "(el desmembramiento se gestiona aparte).")]
    [SerializeField] bool isLimb = false;

    public void SetLimb(bool value) => isLimb = value;

    public float BreakForceMultiplier => breakForceMultiplier;
    public bool IsLimb => isLimb;

    void Awake() => Rigidbody = GetComponent<Rigidbody>();
    public void SetHeld(bool held) => IsHeld = held;
}