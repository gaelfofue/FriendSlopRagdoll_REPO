
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
    private List<DeliveryZone> deliveryZones =
        new List<DeliveryZone>();

    [Header("Jugadores")]
    [Tooltip("Ordenar los jugadores según su PlayerId: 0, 1, 2, 3")]
    [SerializeField]
    private List<GameObject> playerObjects =
        new List<GameObject>();

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

    // Cuando una ficha entra en una Delivery Zone
    public void OnPieceDelivered(
        DeliveryZone zone,
        FallingPiece piece)
    {
        if (gameEnded) return;

        // En muerte súbita solo gana quien entrega
        // la ficha dorada.
        if (isSuddenDeath && piece == goldenPiece)
        {
            EndGame(zone);
        }
    }

    // Cuando termina el temporizador
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

        // Ganador único
        if (topPlayers.Count == 1 && topScore > 0)
        {
            EndGame(topPlayers[0]);
        }
        else
        {
            // Empate o nadie consiguió fichas
            StartSuddenDeath();
        }
    }

    // Iniciar muerte súbita
    private void StartSuddenDeath()
    {
        isSuddenDeath = true;

        if (spawner != null)
            spawner.StopSpawning();

        // Eliminar fichas normales
        foreach (var piece in FindObjectsOfType<FallingPiece>())
        {
            Destroy(piece.gameObject);
        }

        // Reiniciar puntuaciones
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

        Debug.Log(
            "[ParchisMinigameManager] ¡MUERTE SÚBITA!"
        );
    }

    // Terminar partida y comenzar secuencia de victoria
    private void EndGame(DeliveryZone winnerZone)
    {
        if (gameEnded || winnerZone == null) return;

        gameEnded = true;

        if (gameTimer != null)
            gameTimer.StopTimer();

        if (spawner != null)
            spawner.StopSpawning();

        int winnerId = winnerZone.PlayerId;

        // Guardar el ganador para cuando termine
        // la secuencia de cámara.
        finishedWinnerId = winnerId;

        Debug.Log(
            $"[ParchisMinigameManager] " +
            $"¡GANADOR: JUGADOR {winnerId}!"
        );

        // Buscar al personaje ganador
        GameObject winnerObject = null;

        if (winnerId >= 0 &&
            winnerId < playerObjects.Count)
        {
            winnerObject = playerObjects[winnerId];
        }

        // Reproducir cámara lenta y zoom al ganador
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
            Debug.LogWarning(
                "[ParchisMinigameManager] " +
                "Falta WinnerSequence o el personaje ganador. " +
                "Finalizando sin animación."
            );

            FinishRound(winnerId);
        }
    }

    // Se ejecuta cuando termina la WinnerSequence
    private void HandleWinnerSequenceFinished()
    {
        if (winnerSequence != null)
        {
            winnerSequence.OnSequenceFinished -=
                HandleWinnerSequenceFinished;
        }

        FinishRound(finishedWinnerId);
    }

    // Comunicar el ganador al GameManager
    private void FinishRound(int winnerId)
    {
        // Evitar finalizar dos veces
        if (roundFinished) return;

        roundFinished = true;

        Debug.Log(
            $"[ParchisMinigameManager] " +
            $"Ronda terminada. Ganador: {winnerId}"
        );

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMinigameFinished(winnerId);
        }
        else
        {
            Debug.LogWarning(
                "[ParchisMinigameManager] " +
                "No existe GameManager en la escena. " +
                "Ganador: Jugador " + winnerId
            );
        }
    }

    void OnDestroy()
    {
        // Desuscribir eventos para evitar errores
        if (gameTimer != null)
        {
            gameTimer.OnTimerFinished -= OnTimerFinished;
        }

        if (winnerSequence != null)
        {
            winnerSequence.OnSequenceFinished -=
                HandleWinnerSequenceFinished;
        }
    }
}
