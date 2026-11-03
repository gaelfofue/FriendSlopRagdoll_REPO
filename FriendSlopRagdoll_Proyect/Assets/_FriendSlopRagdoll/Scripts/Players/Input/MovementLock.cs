using UnityEngine;

// Sistema compartido para bloquear el movimiento
// de un jugador desde diferentes componentes.

public class MovementLock : MonoBehaviour
{
    private int lockCount = 0;

    // Indica si existe algún bloqueo activo.
    public bool IsLocked => lockCount > 0;

    // Añade un bloqueo.
    public void Lock()
    {
        lockCount++;
    }

    // Retira un bloqueo sin permitir valores negativos.
    public void Unlock()
    {
        lockCount = Mathf.Max(0, lockCount - 1);
    }
}