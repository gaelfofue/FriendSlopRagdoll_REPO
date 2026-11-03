
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

    private float totalPoints = 0f;

    public int PlayerId => playerId;
    public int PiecesDelivered => pieces.Count;
    public float TotalPoints => totalPoints;

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
        if (piece == null) return;

        if (pieces.Add(piece))
        {
            totalPoints += piece.PointsValue;

            Debug.Log(
                $"Jugador {playerId + 1}: " +
                $"{PiecesDelivered} fichas | " +
                $"{TotalPoints:0.##} puntos"
            );

            if (manager != null)
                manager.OnPieceDelivered(this, piece);
        }
    }

    public void RemovePiece(FallingPiece piece)
    {
        if (piece != null && pieces.Remove(piece))
        {
            totalPoints -= piece.PointsValue;

            if (Mathf.Abs(totalPoints) < 0.001f)
                totalPoints = 0f;

            Debug.Log(
                $"Jugador {playerId + 1}: " +
                $"{PiecesDelivered} fichas | " +
                $"{TotalPoints:0.##} puntos"
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
        totalPoints = 0f;
    }
}
