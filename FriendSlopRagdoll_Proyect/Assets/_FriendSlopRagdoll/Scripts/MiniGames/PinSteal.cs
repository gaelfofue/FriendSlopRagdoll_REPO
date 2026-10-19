using UnityEngine;

public class PinSteal : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private PinHolder myHolder;
    private PinGameManager gameManager;

    void Start()
    {
        myHolder = GetComponent<PinHolder>();
        gameManager = FindObjectOfType<PinGameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        PinHolder otherHolder = other.GetComponent<PinHolder>();
        if (otherHolder == null) return;

        // Caso 1: yo no tengo, el otro sí -> le robo
        if (!myHolder.HasPin && otherHolder.HasPin)
        {
            gameManager.StealPin(otherHolder, myHolder);
        }
        // Caso 2: yo tengo, el otro no -> el otro me roba a mí
        else if (myHolder.HasPin && !otherHolder.HasPin)
        {
            gameManager.StealPin(myHolder, otherHolder);
        }
        // Si ambos tienen o ninguno tiene, no pasa nada
    }
}