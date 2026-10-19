using UnityEngine;

public class PinPickup : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private bool hasBeenPicked = false;
    private PinGameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<PinGameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenPicked) return;

        if (!other.CompareTag(playerTag)) return;

        PinHolder holder = other.GetComponent<PinHolder>();
        if (holder == null) return;

        hasBeenPicked = true;
        gameManager.OnPinPickedUp(holder);
    }
}