using UnityEngine;

public class PinPickup : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private bool hasBeenPicked = false;
    private PinGameManager gameManager;

    private PinGameManager GetManager()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<PinGameManager>();
        return gameManager;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenPicked) return;
        if (!other.CompareTag(playerTag)) return;

        PinHolder holder = other.GetComponent<PinHolder>();
        if (holder == null)
        {
            Debug.LogWarning($"[PinPickup] {other.name} tiene tag Player pero no PinHolder.");
            return;
        }

        PinGameManager mgr = GetManager();
        if (mgr == null)
        {
            Debug.LogError("[PinPickup] No hay PinGameManager en la escena.");
            return;
        }

        hasBeenPicked = true;
        mgr.OnPinPickedUp(holder);
    }
}