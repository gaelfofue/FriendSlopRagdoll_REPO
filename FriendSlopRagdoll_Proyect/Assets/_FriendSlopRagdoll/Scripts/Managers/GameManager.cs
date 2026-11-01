
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

    private int currentRoundIndex = 0;

    // Estadísticas: PlayerId -> cantidad
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

        InitializePlayerStats();
        ShuffleMinigames();
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

    // Comenzar una partida nueva
    public void StartNewGame(int rounds)
    {
        if (minigamesPool.Count == 0)
        {
            Debug.LogError("GameManager: No hay minijuegos configurados.");
            return;
        }

        totalRounds = Mathf.Max(1, rounds);
        currentRoundIndex = 0;

        InitializePlayerStats();
        ShuffleMinigames();

        SceneManager.LoadScene(
            shuffledOrder[currentRoundIndex].sceneName
        );
    }

    // Registrar ganador de un minijuego
    public void OnMinigameFinished(int winnerPlayerId)
    {
        if (!playerScores.ContainsKey(winnerPlayerId))
            playerScores[winnerPlayerId] = 0;

        playerScores[winnerPlayerId]++;

        currentRoundIndex++;

        Debug.Log(
            $"Jugador {winnerPlayerId} gana. " +
            $"Victorias: {playerScores[winnerPlayerId]}"
        );

        OnPlayerStatsChanged?.Invoke();

        if (currentRoundIndex >= totalRounds)
        {
            RestartCurrentMinigame();
            return;
        }

        // Si solo hay un minijuego disponible, volver a jugarlo.
        if (shuffledOrder.Count == 1)
        {
            SceneManager.LoadScene(shuffledOrder[0].sceneName);
        }
        else
        {
            SceneManager.LoadScene(
                shuffledOrder[currentRoundIndex].sceneName
            );
        }
    }

    // Reiniciar la partida al terminar todas las rondas
    private void RestartCurrentMinigame()
    {
        string currentScene =
            SceneManager.GetActiveScene().name;

        Debug.Log("Partida terminada. Reiniciando minijuego.");

        currentRoundIndex = 0;

        InitializePlayerStats();

        SceneManager.LoadScene(currentScene);
    }

    // Registrar un brazo arrancado
    public void RegisterArmRipped(int playerId)
    {
        if (!playerArmsRipped.ContainsKey(playerId))
            playerArmsRipped[playerId] = 0;

        playerArmsRipped[playerId]++;

        Debug.Log(
            $"Jugador {playerId} ha perdido un brazo. " +
            $"Total: {playerArmsRipped[playerId]}"
        );

        OnPlayerStatsChanged?.Invoke();
    }

    // Consultar estadísticas
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
