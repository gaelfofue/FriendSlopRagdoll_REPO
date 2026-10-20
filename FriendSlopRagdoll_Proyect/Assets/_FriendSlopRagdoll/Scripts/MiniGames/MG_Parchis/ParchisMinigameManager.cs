using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParchisMinigameManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private PieceSpawner spawner;
    [SerializeField] private WinnerSequence winnerSequence;
    [SerializeField] private List<DeliveryZone> deliveryZones = new List<DeliveryZone>();

    [Header("Desempate")]
    [SerializeField] private FallingPiece goldenPiecePrefab;
    [SerializeField] private Transform goldenPieceSpawnPoint;
    [SerializeField] private float goldenPieceSpawnHeight = 12f;

    private bool gameEnded = false;
    private bool isSuddenDeath = false;
    private bool goldenPieceSpawned = false;

    void Start()
    {
        if (gameTimer == null || spawner == null)
        {
            Debug.LogError("[ParchisMinigameManager] Faltan referencias.");
            return;
        }

        gameTimer.OnTimerFinished += OnTimerFinished;
        spawner.StartSpawning();
    }

    public void OnPieceDelivered(DeliveryZone zone, FallingPiece piece)
    {
        if (gameEnded) return;

        // En muerte súbita, la ficha dorada da la victoria inmediata
        if (isSuddenDeath && goldenPieceSpawned)
        {
            EndGame(zone);
        }
    }

    private void OnTimerFinished()
    {
        if (gameEnded) return;

        // Comprobar empate
        var sorted = deliveryZones.OrderByDescending(z => z.PiecesDelivered).ToList();
        int topScore = sorted[0].PiecesDelivered;
        var topPlayers = sorted.Where(z => z.PiecesDelivered == topScore).ToList();

        // Si hay un único ganador claro (y alguien metió al menos 1)
        if (topPlayers.Count == 1 && topScore > 0)
        {
            EndGame(topPlayers[0]);
        }
        else
        {
            // Empate o todos con 0 -> muerte súbita
            StartSuddenDeath();
        }
    }

    private void StartSuddenDeath()
    {
        isSuddenDeath = true;
        spawner.StopSpawning();

        // Opcional: borrar todas las fichas del tablero
        foreach (var piece in FindObjectsOfType<FallingPiece>())
            Destroy(piece.gameObject);

        foreach (var zone in deliveryZones)
            zone.ResetCount();

        // Spawnear la ficha dorada
        if (goldenPiecePrefab == null || goldenPieceSpawnPoint == null)
        {
            Debug.LogError("[ParchisMinigameManager] Falta el prefab o el punto de spawn de la ficha dorada.");
            return;
        }

        Vector3 spawnPos = goldenPieceSpawnPoint.position + Vector3.up * goldenPieceSpawnHeight;
        Instantiate(goldenPiecePrefab, spawnPos, Quaternion.identity);
        goldenPieceSpawned = true;

        Debug.Log("[ParchisMinigameManager] ¡Muerte súbita! La ficha dorada decide.");
    }

    private void EndGame(DeliveryZone winnerZone)
    {
        if (gameEnded) return;
        gameEnded = true;

        if (gameTimer != null) gameTimer.StopTimer();
        if (spawner != null) spawner.StopSpawning();

        if (winnerSequence != null)
            winnerSequence.PlaySequence(winnerZone.gameObject, winnerZone.gameObject);

        GameManager.Instance?.OnMinigameFinished(winnerZone.PlayerId);
    }
}