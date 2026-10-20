using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DeliveryZone : MonoBehaviour
{
    [SerializeField] private int playerId = 0;
    [SerializeField] private string pieceTag = "Piece"; // Tag de las fichas

    private int piecesDelivered = 0;
    private ParchisMinigameManager manager;

    public int PlayerId => playerId;
    public int PiecesDelivered => piecesDelivered;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        manager = FindObjectOfType<ParchisMinigameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(pieceTag)) return;

        FallingPiece piece = other.GetComponent<FallingPiece>();
        if (piece == null || piece.IsDelivered) return;

        piece.MarkAsDelivered();
        piecesDelivered++;

        if (manager != null)
            manager.OnPieceDelivered(this, piece);
    }

    public void ResetCount()
    {
        piecesDelivered = 0;
    }

    // Para la ronda de desempate: solo cuenta si es la ficha dorada
    public void ForceDeliver(FallingPiece piece)
    {
        piece.MarkAsDelivered();
        piecesDelivered++;
    }
}