
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DeliveryZone : MonoBehaviour
{
    [SerializeField] private int playerId = 0;
    [SerializeField] private string pieceTag = "Piece";

    private HashSet<FallingPiece> pieces =
        new HashSet<FallingPiece>();

    private ParchisMinigameManager manager;

    public int PlayerId => playerId;
    public int PiecesDelivered => pieces.Count;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void Start()
    {
        manager = FindObjectOfType<ParchisMinigameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(pieceTag)) return;

        FallingPiece piece =
            other.GetComponentInParent<FallingPiece>();

        if (piece == null) return;

        piece.SetCurrentZone(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(pieceTag)) return;

        FallingPiece piece =
            other.GetComponentInParent<FallingPiece>();

        if (piece == null) return;

        if (piece.CurrentZone == this)
            piece.LeaveCurrentZone();
    }

    public void AddPiece(FallingPiece piece)
    {
        if (pieces.Add(piece))
        {
            Debug.Log(
                $"Jugador {playerId}: {PiecesDelivered} fichas"
            );

            if (manager != null)
                manager.OnPieceDelivered(this, piece);
        }
    }

    public void RemovePiece(FallingPiece piece)
    {
        if (pieces.Remove(piece))
        {
            Debug.Log(
                $"Jugador {playerId}: {PiecesDelivered} fichas"
            );
        }
    }

    public void ResetCount()
    {
        foreach (var piece in new List<FallingPiece>(pieces))
        {
            if (piece != null)
                piece.LeaveCurrentZone();
        }

        pieces.Clear();
    }
}
