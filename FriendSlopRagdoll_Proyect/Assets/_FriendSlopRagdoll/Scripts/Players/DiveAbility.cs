using UnityEngine;

// Cabezaso/plancha agresiva. Solo activo (via AbilityUnlocker) sin brazos

public class DiveAbility : MonoBehaviour
{
    [SerializeField] Rigidbody coreRigidbody;
    [SerializeField] PlayerInputRelay input;
    [SerializeField] MovementLock movementLock;

    [SerializeField] float diveForwardForce = 40f;
    [SerializeField] float diveUpwardForce = 6f;
    [SerializeField] float diveDuration = 0.6f;
    [SerializeField] float diveCooldown = 1.5f;

    float cooldownTimer;

    void OnEnable() { input.OnDivePressed += TryDive; cooldownTimer = 0f; }
    void OnDisable() => input.OnDivePressed -= TryDive;

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    void TryDive()
    {
        if (cooldownTimer > 0f) return;

        Vector3 dir = coreRigidbody.transform.forward * diveForwardForce + Vector3.up * diveUpwardForce;
        coreRigidbody.AddForce(dir, ForceMode.Impulse);

        movementLock.Lock();
        cooldownTimer = diveCooldown;
        Invoke(nameof(EndDive), diveDuration);
    }

    void EndDive () => movementLock.Unlock();
}
