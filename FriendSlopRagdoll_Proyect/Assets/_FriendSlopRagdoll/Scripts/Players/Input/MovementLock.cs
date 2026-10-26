using UnityEngine;

// "Semaforo" compartido de cualquier sistema 
// Que el movimiento normal se bloquee sin conocer al player controller

public class MovementLock : MonoBehaviour
{
    int lockCount;

    public bool IsLocked => lockCount > 0;
    public void Lock() => lockCount++;
    public void Unlock() => lockCount = Mathf.Max(0, lockCount - 1);
}
