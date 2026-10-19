using UnityEngine;

public class WinnerPickUp : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string playerTag = "Player";

    private bool hasTriggered = false;
    private WinnerSequence winnerSequence;

    void Start()
    {
        // Buscamos el manager de la secuencia en la escena
        winnerSequence = FindObjectOfType<WinnerSequence>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Evitamos que se dispare múltiples veces si el jugador tiene varios colliders
        if (hasTriggered) return;

        if (other.CompareTag(playerTag))
        {
            hasTriggered = true;

            // Desactivamos el modelo visual o el collider para que no se pueda volver a tocar
            GetComponent<Collider>().enabled = false;

            // Opcional: Ocultar el objeto visualmente o desactivar el script de flotación
            // gameObject.SetActive(false); 

            if (winnerSequence != null)
            {
                // Le pasamos a quién ha ganado (el jugador) y este pickup para el zoom
                winnerSequence.StartWinnerSequence(other.gameObject, this.gameObject);
            }
            else
            {
                Debug.LogError("No se encontró el script WinnerSequence en la escena.");
            }
        }
    }
}