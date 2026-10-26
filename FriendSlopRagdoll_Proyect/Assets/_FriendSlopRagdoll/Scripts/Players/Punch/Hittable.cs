using UnityEngine;

// Componente en el enemigo. Recibe golpes del SCR_PunchAbility y delega
// en el SCR_StunController del enemigo.
public class Hittable : MonoBehaviour
{
    [Header("References")]
    [SerializeField] StunController stunController;

    [Tooltip("Rigidbody del CoreBall del enemigo. Recibe el knockback.")]
    [SerializeField] Rigidbody coreBallRigidbody;

    public void ReceiveHit(Vector3 direction, float force, float damage, float knockbackForce, float stunDuration)
    {
        // Aplicar knockback al CoreBall.
        if (coreBallRigidbody != null)
        {
            Vector3 kb = direction.normalized * knockbackForce;
            kb.y = Mathf.Max(kb.y, knockbackForce * 0.3f); // un poco hacia arriba, para que salga volando
            coreBallRigidbody.AddForce(kb, ForceMode.Impulse);
        }

        // Activar el stun.
        if (stunController != null)
        {
            stunController.Stun(stunDuration);
        }

        Debug.Log($"[Hittable] Hit received. force={force}, damage={damage}, kb={knockbackForce}, stun={stunDuration}");
    }
}