
using System.Collections.Generic;
using UnityEngine;

public class PunchHitbox : MonoBehaviour
{
    [SerializeField] Collider hitbox;
    [SerializeField] NewPlayerController owner;

    [SerializeField] float punchImpulse = 8f;

    HashSet<NewPlayerController> hitPlayersThisPunch =
        new HashSet<NewPlayerController>();

    void Awake()
    {
        if (hitbox == null)
            hitbox = GetComponent<Collider>();

        if (owner == null)
            owner = GetComponentInParent<NewPlayerController>();

        hitbox.isTrigger = true;
        hitbox.enabled = false;
    }

    public void BeginPunch()
    {
        hitPlayersThisPunch.Clear();
        hitbox.enabled = true;
    }

    public void EndPunch()
    {
        hitbox.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (owner == null || !owner.isActiveRagdoll)
            return;

        NewPlayerController target =
            other.GetComponentInParent<NewPlayerController>();

        // Solo contamos impactos contra otros jugadores.
        if (target == null || target == owner)
            return;

        if (!target.isActiveRagdoll)
            return;

        // Varios colliders del mismo jugador no cuentan como varios golpes.
        if (!hitPlayersThisPunch.Add(target))
            return;

        Rigidbody hitBody = other.attachedRigidbody;

        target.RecievePunchHit(
            hitBody,
            transform.forward,
            punchImpulse
        );
    }
}