using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ResultsLeaderboardUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform rowsContainer;
    [SerializeField] private LeaderboardPlayerRowUI rowPrefab;

    [Header("Animación de clasificación")]
    [SerializeField] private float positionAnimationDuration = 0.8f;

    private GameManager gameManager;

    private void OnEnable()
    {
        gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            Debug.LogError("[ResultsLeaderboardUI] No se encontró GameManager.");
            return;
        }

        RefreshLeaderboard();
    }

    public void RefreshLeaderboard()
    {
        if (gameManager == null || rowsContainer == null || rowPrefab == null)
            return;

        foreach (Transform child in rowsContainer)
            Destroy(child.gameObject);

        List<int> playerIds = gameManager.GetPlayerIds();

        var sortedPlayers = playerIds
            .Select(id => new
            {
                Id = id,
                Name = gameManager.GetPlayerName(id),
                Wins = gameManager.GetScore(id)
            })
            .OrderByDescending(player => player.Wins)
            .ThenBy(player => player.Id)
            .ToList();

        PlayerIdentity[] players =
            FindObjectsByType<PlayerIdentity>(FindObjectsSortMode.None);

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            var player = sortedPlayers[i];

            LeaderboardPlayerRowUI row =
                Instantiate(rowPrefab, rowsContainer);

            row.Setup(
                player.Id,
                i + 1,
                player.Name,
                player.Wins
            );

            foreach (PlayerIdentity identity in players)
            {
                if (identity.PlayerIndex == player.Id)
                {
                    PlayerColor playerColor =
                        identity.GetComponent<PlayerColor>();

                    if (playerColor != null)
                    {
                        row.SetPlayerColor(
                            playerColor.GetPlayerColor(player.Id)
                        );
                    }

                    break;
                }
            }
        }
    }

    public void PlayVictoryUpdate(int playerId, int newWins)
    {
        foreach (Transform child in rowsContainer)
        {
            LeaderboardPlayerRowUI row =
                child.GetComponent<LeaderboardPlayerRowUI>();

            if (row != null && row.PlayerId == playerId)
            {
                row.PlayVictoryUpdate(newWins);
                return;
            }
        }

        Debug.LogWarning(
            "[ResultsLeaderboardUI] No se encontró la fila del ganador."
        );
    }

    public IEnumerator AnimateLeaderboardUpdate()
    {
        if (gameManager == null || rowsContainer == null)
            yield break;

        GridLayoutGroup grid =
            rowsContainer.GetComponent<GridLayoutGroup>();

        if (grid == null)
        {
            Debug.LogError(
                "[ResultsLeaderboardUI] Falta GridLayoutGroup."
            );
            yield break;
        }

        RectTransform containerRect =
            rowsContainer as RectTransform;

        if (containerRect == null)
            yield break;

        // Guardamos el orden actual.
        List<LeaderboardPlayerRowUI> originalRows =
            rowsContainer
                .Cast<Transform>()
                .Select(t => t.GetComponent<LeaderboardPlayerRowUI>())
                .Where(row => row != null)
                .ToList();

        // Obtenemos la nueva clasificación.
        List<int> playerIds = gameManager.GetPlayerIds();

        var sortedIds = playerIds
            .OrderByDescending(id => gameManager.GetScore(id))
            .ThenBy(id => id)
            .ToList();

        Dictionary<int, LeaderboardPlayerRowUI> rowsById =
            originalRows.ToDictionary(row => row.PlayerId);

        List<LeaderboardPlayerRowUI> sortedRows =
            new List<LeaderboardPlayerRowUI>();

        foreach (int id in sortedIds)
        {
            if (rowsById.TryGetValue(id, out LeaderboardPlayerRowUI row))
                sortedRows.Add(row);
        }

        if (sortedRows.Count != originalRows.Count)
        {
            Debug.LogError(
                "[ResultsLeaderboardUI] No coinciden las filas de jugadores."
            );
            yield break;
        }

        // Guardamos las posiciones originales.
        Dictionary<LeaderboardPlayerRowUI, int> oldPositions =
            new Dictionary<LeaderboardPlayerRowUI, int>();

        for (int i = 0; i < originalRows.Count; i++)
            oldPositions[originalRows[i]] = i;

        // Calculamos las posiciones de destino usando el Grid.
        for (int i = 0; i < sortedRows.Count; i++)
            sortedRows[i].transform.SetSiblingIndex(i);

        LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);

        Dictionary<LeaderboardPlayerRowUI, Vector2> targetPositions =
            new Dictionary<LeaderboardPlayerRowUI, Vector2>();

        foreach (LeaderboardPlayerRowUI row in sortedRows)
        {
            RectTransform rect =
                row.GetComponent<RectTransform>();

            targetPositions[row] = rect.anchoredPosition;
        }

        // Restauramos el orden original antes de mover las filas.
        for (int i = 0; i < originalRows.Count; i++)
            originalRows[i].transform.SetSiblingIndex(i);

        LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);

        // Desactivamos temporalmente el Grid para permitir el movimiento.
        grid.enabled = false;

        // Lanzamos todas las animaciones en el mismo momento.
        for (int i = 0; i < sortedRows.Count; i++)
        {
            LeaderboardPlayerRowUI row = sortedRows[i];

            int oldPosition = oldPositions[row];
            int newPosition = i;

            if (oldPosition != newPosition)
            {
                bool movedUp = newPosition < oldPosition;

                StartCoroutine(
                    row.AnimateToPosition(
                        targetPositions[row],
                        movedUp,
                        positionAnimationDuration
                    )
                );
            }
        }

        // Esperamos a que todos los movimientos terminen.
        yield return new WaitForSecondsRealtime(
            positionAnimationDuration
        );

        // Aplicamos el nuevo orden definitivo.
        for (int i = 0; i < sortedRows.Count; i++)
            sortedRows[i].transform.SetSiblingIndex(i);

        // Reactivamos el Grid.
        grid.enabled = true;

        LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);

        // Actualizamos los números y ocultamos las flechas.
        for (int i = 0; i < sortedRows.Count; i++)
        {
            sortedRows[i].UpdatePosition(i + 1);
            sortedRows[i].HidePositionChangeArrows();
        }

        Debug.Log("[ResultsLeaderboardUI] Clasificación actualizada.");
    }
}