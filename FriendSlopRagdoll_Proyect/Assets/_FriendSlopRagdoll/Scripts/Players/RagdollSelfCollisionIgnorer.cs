using UnityEngine;

// Ignora todas las colisiones entre los colliders del propio ragdoll.
// Sin esto, los huesos chocan entre si (brazo contra torso, brazo contra brazo, etc)
// y eso rompe cualquier intento de controlar el ragdoll con joints,
// generando trabazos, temblores o "calambres" cuando un hueso intenta moverse
// hacia una pose que lo acerca a otra parte del cuerpo.
//
// DEBE ir en el objeto RAIZ del personaje (el que es padre de todos los huesos).
// DEBE ejecutarse ANTES de que cualquier otro sistema empiece a mover el ragdoll,
// por eso usamos Awake() en vez de Start().
public class RagdollSelfCollisionIgnorer : MonoBehaviour
{
    void Awake()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        int ignoredPairs = 0;

        for (int i = 0; i < colliders.Length; i++)
        {
            for (int j = i + 1; j < colliders.Length; j++)
            {
                Physics.IgnoreCollision(colliders[i], colliders[j], true);
                ignoredPairs++;
            }
        }

        Debug.Log($"[RagdollSelfCollisionIgnorer] Pares de colliders ignorados: {ignoredPairs}");
    }
}