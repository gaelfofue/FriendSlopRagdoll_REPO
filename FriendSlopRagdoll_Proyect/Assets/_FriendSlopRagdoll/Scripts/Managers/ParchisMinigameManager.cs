
using System.Collections;
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

    [Header("Desempate")]
    [SerializeField] private FallingPiece goldenPiecePrefab;
    [SerializeField] private Transform goldenPieceSpawnPoint;
    [SerializeField] private float goldenPieceSpawnHeight = 12f;

    private readonly HashSet<int> activePlayerIds =
        new HashSet<int>();

    private List<DeliveryZone> activeDeliveryZones =
        new List<DeliveryZone>();

    private bool gameEnded = false;
    private bool isSuddenDeath = false;
    private bool roundFinished = false;

    private FallingPiece goldenPiece;
    private int finishedWinnerId = -1;

    private void Start()
    {
        if (gameTimer == null || spawner == null)
        {
            Debug.LogError(
                "[ParchisMinigameManager] Faltan referencias."
            );
            return;
        }

        gameTimer.OnTimerFinished += OnTimerFinished;

        StartCoroutine(InitializeMinigame());
    }

    private IEnumerator InitializeMinigame()
    {
        // Damos tiempo al spawner y a PlayerIdentity
        // para crear e identificar a los jugadores.
        yield return null;
        yield return null;

        DetectActivePlayers();
        ConfigureDeliveryZones();

        if (activePlayerIds.Count == 0)
        {
            Debug.LogError(
                "[ParchisMinigameManager] No se han encontrado jugadores activos."
            );
            yield break;
        }

        Debug.Log(
            "[ParchisMinigameManager] Jugadores activos: " +
            string.Join(", ", activePlayerIds
                .OrderBy(id => id)
                .Select(id => (id + 1).ToString()))
        );

        if (activeDeliveryZones.Count == 0)
        {
            Debug.LogError(
                "[ParchisMinigameManager] Ningún jugador tiene una zona de entrega."
            );
            yield break;
        }

        spawner.StartSpawning();
    }

    private void DetectActivePlayers()
    {
        activePlayerIds.Clear();

        PlayerIdentity[] players =
            FindObjectsOfType<PlayerIdentity>();

        foreach (PlayerIdentity player in players)
        {
            if (player == null)
                continue;

            int id = player.PlayerIndex;

            if (id >= 0)
                activePlayerIds.Add(id);
        }
    }

    private void ConfigureDeliveryZones()
    {
        activeDeliveryZones.Clear();

        foreach (DeliveryZone zone in deliveryZones)
        {
            if (zone == null)
                continue;

            bool playerIsActive =
                activePlayerIds.Contains(zone.PlayerId);

            zone.gameObject.SetActive(playerIsActive);

            if (playerIsActive)
            {
                activeDeliveryZones.Add(zone);

                Debug.Log(
                    "[ParchisMinigameManager] Zona activada: jugador " +
                    (zone.PlayerId + 1)
                );
            }
            else
            {
                Debug.Log(
                    "[ParchisMinigameManager] Zona desactivada: jugador " +
                    (zone.PlayerId + 1) + " (no está jugando)"
                );
            }
        }
    }

    public void OnPieceDelivered(
        DeliveryZone zone,
        FallingPiece piece)
    {
        if (gameEnded || zone == null)
            return;

        if (!activePlayerIds.Contains(zone.PlayerId))
            return;

        if (isSuddenDeath && piece == goldenPiece)
        {
            EndGame(zone);
        }
    }

    private void OnTimerFinished()
    {
        if (gameEnded || isSuddenDeath)
            return;

        var sorted = activeDeliveryZones
            .Where(z => z != null && z.gameObject.activeInHierarchy)
            .OrderByDescending(z => z.TotalPoints)
            .ToList();

        if (sorted.Count == 0)
        {
            Debug.LogError(
                "[ParchisMinigameManager] No hay zonas activas."
            );
            return;
        }

        float topScore = sorted[0].TotalPoints;

        var topPlayers = sorted
            .Where(z => Mathf.Approximately(
                z.TotalPoints, topScore))
            .ToList();

        if (topPlayers.Count == 1)
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
        if (gameEnded || isSuddenDeath)
            return;

        isSuddenDeath = true;

        if (spawner != null)
            spawner.StopSpawning();

        foreach (var piece in FindObjectsOfType<FallingPiece>())
        {
            Destroy(piece.gameObject);
        }

        foreach (var zone in activeDeliveryZones)
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
        if (gameEnded || winnerZone == null)
            return;

        if (!activePlayerIds.Contains(winnerZone.PlayerId))
            return;

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
            {
                Debug.LogWarning(
                    "[ParchisMinigameManager] Falta WinnerSequence."
                );
            }

            if (winnerObject == null)
            {
                Debug.LogWarning(
                    "[ParchisMinigameManager] No se encontró el personaje " +
                    "del jugador " + (winnerId + 1) +
                    ". Se finalizará sin animación."
                );
            }

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

    private void OnDestroy()
    {
        if (gameTimer != null)
            gameTimer.OnTimerFinished -= OnTimerFinished;

        if (winnerSequence != null)
            winnerSequence.OnSequenceFinished -=
                HandleWinnerSequenceFinished;
    }
}
