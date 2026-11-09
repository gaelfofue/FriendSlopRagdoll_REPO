using UnityEngine;

public class DiceImpact : MonoBehaviour
{
    [Header("Configuración del empujón")]
    [SerializeField] private float impactRadius = 3f;
    [SerializeField] private float pushForce = 12f;
    [SerializeField] private float upwardForce = 4f;

    private bool hasImpacted = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted)
            return;

        hasImpacted = true;

        PushNearbyPlayers();
    }

    private void PushNearbyPlayers()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(
            transform.position,
            impactRadius
        );

        foreach (Collider nearbyObject in nearbyObjects)
        {
            PlayerIdentity identity =
                nearbyObject.GetComponentInParent<PlayerIdentity>();

            if (identity == null)
                continue;

            Rigidbody playerRb =
                identity.GetComponent<Rigidbody>();

            if (playerRb == null)
                continue;

            Vector3 direction =
                playerRb.position - transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                direction = transform.forward;

            direction.Normalize();

            Vector3 force =
                direction * pushForce +
                Vector3.up * upwardForce;

            playerRb.AddForce(force, ForceMode.Impulse);
        }
    }
}