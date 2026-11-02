
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParchisMinigameManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private PieceSpawner spawner;
    [SerializeField] private WinnerSequence winnerSequence;

    [SerializeField]
    private List<DeliveryZone> deliveryZones = new List<DeliveryZone>();

    [Header("Desempate")]
    [SerializeField] private FallingPiece goldenPiecePrefab;
    [SerializeField] private Transform goldenPieceSpawnPoint;
    [SerializeField] private float goldenPieceSpawnHeight = 12f;

    private bool gameEnded = false;
    private bool isSuddenDeath = false;
    private bool roundFinished = false;

    private FallingPiece goldenPiece;
    private int finishedWinnerId = -1;

    void Start()
    {
        if (gameTimer == null || spawner == null)
        {
            Debug.LogError(
                "[ParchisMinigameManager] Faltan referencias."
            );
            return;
        }

        gameTimer.OnTimerFinished += OnTimerFinished;

        spawner.StartSpawning();
    }

    public void OnPieceDelivered(DeliveryZone zone, FallingPiece piece)
    {
        if (gameEnded) return;

        if (isSuddenDeath && piece == goldenPiece)
        {
            EndGame(zone);
        }
    }

    private void OnTimerFinished()
    {
        if (gameEnded || isSuddenDeath) return;

        if (deliveryZones.Count == 0)
        {
            Debug.LogError(
                "[ParchisMinigameManager] No hay Delivery Zones."
            );
            return;
        }

        var sorted = deliveryZones
            .Where(z => z != null)
            .OrderByDescending(z => z.PiecesDelivered)
            .ToList();

        if (sorted.Count == 0)
        {
            Debug.LogError("No hay zonas válidas.");
            return;
        }

        int topScore = sorted[0].PiecesDelivered;

        var topPlayers = sorted
            .Where(z => z.PiecesDelivered == topScore)
            .ToList();

        if (topPlayers.Count == 1 && topScore > 0)
        {
            EndGame(topPlayers[0]);
        }
        else
        {
            StartSuddenDeath();
        }
    }

    private void StartSuddenDeath()
    {
        isSuddenDeath = true;

        if (spawner != null)
            spawner.StopSpawning();

        foreach (var piece in FindObjectsOfType<FallingPiece>())
        {
            Destroy(piece.gameObject);
        }

        foreach (var zone in deliveryZones)
        {
            if (zone != null)
                zone.ResetCount();
        }

        if (goldenPiecePrefab == null ||
            goldenPieceSpawnPoint == null)
        {
            Debug.LogError(
                "[ParchisMinigameManager] " +
                "Falta la ficha dorada o su punto de spawn."
            );
            return;
        }

        Vector3 spawnPos =
            goldenPieceSpawnPoint.position +
            Vector3.up * goldenPieceSpawnHeight;

        goldenPiece = Instantiate(
            goldenPiecePrefab,
            spawnPos,
            Quaternion.identity
        );

        Debug.Log("[ParchisMinigameManager] ¡MUERTE SÚBITA!");
    }

    private void EndGame(DeliveryZone winnerZone)
    {
        if (gameEnded || winnerZone == null) return;

        gameEnded = true;

        if (gameTimer != null)
            gameTimer.StopTimer();

        if (spawner != null)
            spawner.StopSpawning();

        int winnerId = winnerZone.PlayerId;

        finishedWinnerId = winnerId;

        Debug.Log(
            "[ParchisMinigameManager] ¡GANADOR: JUGADOR " +
            (winnerId + 1) + "!"
        );

        // Buscar automáticamente al personaje ganador.
        GameObject winnerObject = FindPlayerObject(winnerId);

        if (winnerSequence != null && winnerObject != null)
        {
            winnerSequence.OnSequenceFinished +=
                HandleWinnerSequenceFinished;

            winnerSequence.PlaySequence(
                winnerObject,
                winnerObject
            );
        }
        else
        {
            if (winnerSequence == null)
                Debug.LogWarning(
                    "[ParchisMinigameManager] Falta WinnerSequence."
                );

            if (winnerObject == null)
                Debug.LogWarning(
                    "[ParchisMinigameManager] No se encontró el personaje " +
                    "del jugador " + (winnerId + 1) +
                    ". Se finalizará sin animación."
                );

            FinishRound(winnerId);
        }
    }

    private GameObject FindPlayerObject(int playerId)
    {
        PlayerIdentity[] players =
            FindObjectsOfType<PlayerIdentity>();

        foreach (PlayerIdentity player in players)
        {
            if (player != null && player.PlayerIndex == playerId)
            {
                Debug.Log(
                    "[ParchisMinigameManager] Personaje encontrado: " +
                    player.name + " para el jugador " + (playerId + 1)
                );

                return player.gameObject;
            }
        }

        return null;
    }

    private void HandleWinnerSequenceFinished()
    {
        if (winnerSequence != null)
        {
            winnerSequence.OnSequenceFinished -=
                HandleWinnerSequenceFinished;
        }

        FinishRound(finishedWinnerId);
    }

    private void FinishRound(int winnerId)
    {
        if (roundFinished) return;

        roundFinished = true;

        Debug.Log(
            "[ParchisMinigameManager] Ronda terminada. Ganador: " +
            (winnerId + 1)
        );

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMinigameFinished(winnerId);
        }
        else
        {
            Debug.LogWarning(
                "[ParchisMinigameManager] No existe GameManager en la escena."
            );
        }
    }

    void OnDestroy()
    {
        if (gameTimer != null)
            gameTimer.OnTimerFinished -= OnTimerFinished;

        if (winnerSequence != null)
            winnerSequence.OnSequenceFinished -=
                HandleWinnerSequenceFinished;
    }
}
