using UnityEngine;

public class PlayerDustEffect : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private ParticleSystem dustParticles;

    [Header("Configuración")]
    [SerializeField] private float minimumSpeed = 2f;
    [SerializeField] private float emissionInterval = 0.12f;

    private float emissionTimer;

    private void Update()
    {
        if (playerRigidbody == null || dustParticles == null)
            return;

        Vector3 velocity = playerRigidbody.linearVelocity;
        velocity.y = 0f;

        float horizontalSpeed = velocity.magnitude;

        if (horizontalSpeed >= minimumSpeed)
        {
            emissionTimer += Time.deltaTime;

            if (emissionTimer >= emissionInterval)
            {
                dustParticles.Emit(3);
                emissionTimer = 0f;
            }
        }
        else
        {
            emissionTimer = 0f;
        }
    }
}


