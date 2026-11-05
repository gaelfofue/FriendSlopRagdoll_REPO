
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuración de partida")]
    [SerializeField] private int totalRounds = 3;

    [Header("Escenas")]
    [SerializeField] private string minigameSceneName = "SCN_ParchisTest";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Jugadores")]
    [SerializeField] private int totalPlayers = 4;

    // Copia persistente de los jugadores recibidos del lobby.
    private List<LobbyTransferData.PlayerData> playerRoster =
        new List<LobbyTransferData.PlayerData>();

    // Número de rondas ya completadas.
    private int currentRoundIndex = 0;

    private bool awaitingNextRound = false;
    private bool roundFinished = false;

    private Dictionary<int, int> playerScores =
        new Dictionary<int, int>();

    private Dictionary<int, int> playerArmsRipped =
        new Dictionary<int, int>();

    public event Action OnPlayerStatsChanged;

    public int CurrentRound => currentRoundIndex + 1;
    public int TotalRounds => totalRounds;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        InitializePlayerStats();

        Debug.Log("[GameManager] Gestor de partida iniciado.");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    // Se ejecuta al cargar una escena.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != minigameSceneName)
            return;

        // Si estamos entrando para jugar la siguiente ronda,
        // conservamos las victorias y el progreso.
        if (awaitingNextRound)
        {
            awaitingNextRound = false;
            roundFinished = false;

            Debug.Log(
                "[GameManager] Comenzando ronda " +
                (currentRoundIndex + 1) + "/" + totalRounds
            );

            return;
        }

        // Si entramos en Parchís sin estar esperando otra ronda,
        // comienza una partida nueva.
        currentRoundIndex = 0;
        roundFinished = false;

        InitializePlayerStats();

        CaptureLobbyPlayers();

        Debug.Log(
            "[GameManager] Nueva partida. Rondas: " + totalRounds
        );
    }

    // Copiar los jugadores guardados desde el lobby.
    public void CaptureLobbyPlayers()
    {
        if (LobbyTransferData.Players.Count == 0)
        {
            Debug.LogWarning(
                "[GameManager] No hay jugadores disponibles para guardar."
            );
            return;
        }

        playerRoster.Clear();

        foreach (LobbyTransferData.PlayerData data in LobbyTransferData.Players)
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
            "[GameManager] Jugadores guardados: " + playerRoster.Count
        );
    }

    // Restaurar los jugadores antes de cargar otra ronda.
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

    // Permite iniciar una partida desde otro script si se necesita.
    public void StartNewGame(int rounds)
    {
        totalRounds = Mathf.Max(1, rounds);

        currentRoundIndex = 0;
        awaitingNextRound = false;
        roundFinished = false;

        InitializePlayerStats();
        CaptureLobbyPlayers();

        if (playerRoster.Count == 0)
        {
            Debug.LogError(
                "[GameManager] No hay jugadores recibidos del lobby."
            );
            return;
        }

        RestoreLobbyPlayers();

        SceneManager.LoadScene(minigameSceneName);
    }

    // Se llama cuando termina una ronda y ya se conoce al ganador.
    public void OnMinigameFinished(int winnerPlayerId)
    {
        // Evita que se contabilice dos veces la misma ronda.
        if (roundFinished)
            return;

        roundFinished = true;

        if (!playerScores.ContainsKey(winnerPlayerId))
            playerScores[winnerPlayerId] = 0;

        playerScores[winnerPlayerId]++;

        currentRoundIndex++;

        Debug.Log(
            "Jugador " + (winnerPlayerId + 1) +
            " gana. Victorias: " + playerScores[winnerPlayerId]
        );

        OnPlayerStatsChanged?.Invoke();

        // ¿Se han completado todas las rondas?
        if (currentRoundIndex >= totalRounds)
        {
            Debug.Log("[GameManager] ¡Partida terminada!");

            SceneManager.LoadScene(mainMenuSceneName);
            return;
        }

        // Preparar los jugadores para la siguiente ronda.
        if (playerRoster.Count == 0)
        {
            Debug.LogError(
                "[GameManager] No hay jugadores guardados para la siguiente ronda."
            );
            return;
        }

        RestoreLobbyPlayers();

        awaitingNextRound = true;

        Debug.Log(
            "[GameManager] Cargando ronda " +
            (currentRoundIndex + 1) + "/" + totalRounds
        );

        SceneManager.LoadScene(minigameSceneName);
    }

    public void RegisterArmRipped(int playerId)
    {
        if (!playerArmsRipped.ContainsKey(playerId))
            playerArmsRipped[playerId] = 0;

        playerArmsRipped[playerId]++;

        Debug.Log(
            "Jugador " + (playerId + 1) +
            " ha arrancado un brazo. Total: " +
            playerArmsRipped[playerId]
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
}
