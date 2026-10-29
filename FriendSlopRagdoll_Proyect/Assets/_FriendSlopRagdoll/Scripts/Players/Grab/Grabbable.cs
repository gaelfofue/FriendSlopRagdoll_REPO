using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour
{
    public Rigidbody Rigidbody { get; private set; }
    public bool IsHeld { get; private set; }

    [Header("Break Settings")]
    [SerializeField] float breakForceMultiplier = 1f;
    [SerializeField] bool isLimb = false;

    [Header("Ownership")]
    [Tooltip("Root del jugador al que pertenece este Grabbable. " +
             "Para brazos, el root del jugador. Para objetos, vacio o el objeto mismo.")]
    [SerializeField] Transform owner;

    public float BreakForceMultiplier => breakForceMultiplier;
    public bool IsLimb => isLimb;
    public Transform Owner => owner;
    public Transform CurrentHolder { get; private set; }

    void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        if (owner == null) owner = transform.root;
    }

    public void SetLimb(bool value) => isLimb = value;

    public void SetHeld(bool held, Transform holder = null)
    {
        IsHeld = held;
        CurrentHolder = held ? holder : null;
    }
}