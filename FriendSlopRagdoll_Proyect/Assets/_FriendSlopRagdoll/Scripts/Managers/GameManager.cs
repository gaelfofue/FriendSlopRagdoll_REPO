
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class MinigameData
{
    public string sceneName;
    public string minigameName;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuración de partida")]
    [SerializeField] private int totalRounds = 3;
    [SerializeField]
    private List<MinigameData> minigamesPool =
        new List<MinigameData>();

    [Header("Escenas especiales")]
    [SerializeField] private string resultsSceneName = "Results";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Jugadores")]
    [SerializeField] private int totalPlayers = 4;

    [Header("Debug")]
    [SerializeField] private bool persistBetweenScenes = true;

    private List<MinigameData> shuffledOrder =
        new List<MinigameData>();

    // Copia persistente de los datos recibidos del lobby.
    private List<LobbyTransferData.PlayerData> playerRoster =
        new List<LobbyTransferData.PlayerData>();

    private int currentRoundIndex = 0;

    private Dictionary<int, int> playerScores =
        new Dictionary<int, int>();

    private Dictionary<int, int> playerArmsRipped =
        new Dictionary<int, int>();

    public event Action OnPlayerStatsChanged;

    public int CurrentRound => currentRoundIndex + 1;
    public int TotalRounds => totalRounds;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (persistBetweenScenes)
            DontDestroyOnLoad(gameObject);

        CaptureLobbyPlayers();

        InitializePlayerStats();
        ShuffleMinigames();
    }

    // Guardar una copia de los jugadores del lobby.
    public void CaptureLobbyPlayers()
    {
        playerRoster.Clear();

        foreach (LobbyTransferData.PlayerData data
                 in LobbyTransferData.Players)
        {
            LobbyTransferData.PlayerData copy =
                new LobbyTransferData.PlayerData();

            copy.playerIndex = data.playerIndex;
            copy.isKeyboard = data.isKeyboard;
            copy.gamepad = data.gamepad;
            copy.isReady = data.isReady;

            copy.characterId = data.characterId;
            copy.skinId = data.skinId;
            copy.accessoriesData = data.accessoriesData;

            playerRoster.Add(copy);
        }

        Debug.Log(
            "[GameManager] Jugadores guardados: " +
            playerRoster.Count
        );
    }

    // Restaurar los jugadores antes de cargar una ronda.
    private void RestoreLobbyPlayers()
    {
        LobbyTransferData.Clear();

        foreach (LobbyTransferData.PlayerData data in playerRoster)
        {
            LobbyTransferData.PlayerData copy =
                new LobbyTransferData.PlayerData();

            copy.playerIndex = data.playerIndex;
            copy.isKeyboard = data.isKeyboard;
            copy.gamepad = data.gamepad;
            copy.isReady = data.isReady;

            copy.characterId = data.characterId;
            copy.skinId = data.skinId;
            copy.accessoriesData = data.accessoriesData;

            LobbyTransferData.Players.Add(copy);
        }

        Debug.Log(
            "[GameManager] Jugadores restaurados: " +
            LobbyTransferData.Players.Count
        );
    }

    private void InitializePlayerStats()
    {
        playerScores.Clear();
        playerArmsRipped.Clear();

        for (int i = 0; i < totalPlayers; i++)
        {
            playerScores[i] = 0;
            playerArmsRipped[i] = 0;
        }

        OnPlayerStatsChanged?.Invoke();
    }

    public void StartNewGame(int rounds)
    {
        if (minigamesPool.Count == 0)
        {
            Debug.LogError(
                "GameManager: No hay minijuegos configurados."
            );
            return;
        }

        // Capturar los jugadores justo antes de empezar.
        CaptureLobbyPlayers();

        if (playerRoster.Count == 0)
        {
            Debug.LogError(
                "[GameManager] No hay jugadores recibidos del lobby."
            );
            return;
        }

        totalRounds = Mathf.Max(1, rounds);
        currentRoundIndex = 0;

        InitializePlayerStats();
        ShuffleMinigames();

        RestoreLobbyPlayers();

        SceneManager.LoadScene(
            shuffledOrder[currentRoundIndex].sceneName
        );
    }

    public void OnMinigameFinished(int winnerPlayerId)
    {
        if (!playerScores.ContainsKey(winnerPlayerId))
            playerScores[winnerPlayerId] = 0;

        playerScores[winnerPlayerId]++;

        currentRoundIndex++;

        Debug.Log(
            $"Jugador {winnerPlayerId + 1} gana. " +
            $"Victorias: {playerScores[winnerPlayerId]}"
        );

        OnPlayerStatsChanged?.Invoke();

        // Ya se han jugado todas las rondas.
        if (currentRoundIndex >= totalRounds)
        {
            Debug.Log("[GameManager] Partida terminada.");

            SceneManager.LoadScene(resultsSceneName);
            return;
        }

        int nextIndex = currentRoundIndex;

        if (shuffledOrder.Count == 1)
            nextIndex = 0;

        // Recuperar los datos antes de cargar la siguiente ronda.
        RestoreLobbyPlayers();

        SceneManager.LoadScene(
            shuffledOrder[nextIndex].sceneName
        );
    }

    public void RegisterArmRipped(int playerId)
    {
        if (!playerArmsRipped.ContainsKey(playerId))
            playerArmsRipped[playerId] = 0;

        playerArmsRipped[playerId]++;

        Debug.Log(
            $"Jugador {playerId + 1} ha arrancado un brazo. " +
            $"Total: {playerArmsRipped[playerId]}"
        );

        OnPlayerStatsChanged?.Invoke();
    }

    public int GetScore(int playerId)
    {
        return playerScores.TryGetValue(playerId, out int score)
            ? score : 0;
    }

    public int GetArmsRipped(int playerId)
    {
        return playerArmsRipped.TryGetValue(playerId, out int arms)
            ? arms : 0;
    }

    public Dictionary<int, int> GetAllScores()
    {
        return new Dictionary<int, int>(playerScores);
    }

    private void ShuffleMinigames()
    {
        shuffledOrder = new List<MinigameData>(minigamesPool);

        for (int i = shuffledOrder.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);

            (shuffledOrder[i], shuffledOrder[j]) =
                (shuffledOrder[j], shuffledOrder[i]);
        }
    }
}
