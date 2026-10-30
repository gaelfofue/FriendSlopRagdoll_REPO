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
    [SerializeField] private List<MinigameData> minigamesPool = new List<MinigameData>();

    [Header("Escenas especiales")]
    [SerializeField] private string resultsSceneName = "Results";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Debug")]
    [SerializeField] private bool persistBetweenScenes = true;

    private List<MinigameData> shuffledOrder = new List<MinigameData>();
    private int currentRoundIndex = 0;

    // Sistema de puntos: jugadorId -> puntos
    private Dictionary<int, int> playerScores = new Dictionary<int, int>();

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

        ShuffleMinigames();
    }

    // Llamado desde el menú principal cuando el jugador elige rondas
    public void StartNewGame(int rounds)
    {
        totalRounds = rounds;
        currentRoundIndex = 0;
        playerScores.Clear();

        ShuffleMinigames();
        SceneManager.LoadScene(shuffledOrder[currentRoundIndex].sceneName);
    }

    // Llamado por el PinGameManager al terminar el minijuego
    public void OnMinigameFinished(int winnerPlayerId)
    {
        if (!playerScores.ContainsKey(winnerPlayerId))
            playerScores[winnerPlayerId] = 0;

        playerScores[winnerPlayerId]++;
        Debug.Log($"Jugador {winnerPlayerId} gana. Total: {playerScores[winnerPlayerId]}");

        currentRoundIndex++;

        if (currentRoundIndex >= totalRounds)
            SceneManager.LoadScene(resultsSceneName);
        else
            SceneManager.LoadScene(shuffledOrder[currentRoundIndex].sceneName);
    }

    private void ShuffleMinigames()
    {
        shuffledOrder = new List<MinigameData>(minigamesPool);
        for (int i = shuffledOrder.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffledOrder[i], shuffledOrder[j]) = (shuffledOrder[j], shuffledOrder[i]);
        }
    }

    // Consulta pública para el HUD de resultados
    public int GetScore(int playerId)
    {
        return playerScores.TryGetValue(playerId, out int score) ? score : 0;
    }

    public Dictionary<int, int> GetAllScores() => new Dictionary<int, int>(playerScores);
}