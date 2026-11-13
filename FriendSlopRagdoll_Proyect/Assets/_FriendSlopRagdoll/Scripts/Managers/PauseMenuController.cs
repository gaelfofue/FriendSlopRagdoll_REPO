
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject pausePanel;

    [Header("Escenas")]
    [SerializeField] private string lobbySceneName = "Lobby";

    private bool isPaused;

    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisible;

    private readonly HashSet<PlayerInputRelay> subscribedRelays =
        new HashSet<PlayerInputRelay>();

    private void Awake()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    private void OnEnable()
    {
        RefreshRelaySubscriptions();
    }

    private void Update()
    {
        RefreshRelaySubscriptions();
    }

    private void OnDisable()
    {
        UnsubscribeAllRelays();

        if (isPaused)
            ForceResume();
    }

    private void OnDestroy()
    {
        if (isPaused)
            ForceResume();
    }

    private void RefreshRelaySubscriptions()
    {
        PlayerInputRelay[] relays =
            FindObjectsByType<PlayerInputRelay>(FindObjectsSortMode.None);

        HashSet<PlayerInputRelay> currentRelays =
            new HashSet<PlayerInputRelay>(relays);

        foreach (PlayerInputRelay relay in currentRelays)
        {
            if (relay != null && subscribedRelays.Add(relay))
                relay.OnHUDPressed += TogglePause;
        }

        subscribedRelays.RemoveWhere(relay =>
        {
            if (relay == null || !currentRelays.Contains(relay))
            {
                if (relay != null)
                    relay.OnHUDPressed -= TogglePause;

                return true;
            }

            return false;
        });
    }

    private void UnsubscribeAllRelays()
    {
        foreach (PlayerInputRelay relay in subscribedRelays)
        {
            if (relay != null)
                relay.OnHUDPressed -= TogglePause;
        }

        subscribedRelays.Clear();
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (isPaused)
            return;

        isPaused = true;

        previousCursorVisible = Cursor.visible;
        previousCursorLockState = Cursor.lockState;

        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("Juego pausado.");
    }

    // Método que conectaremos al botón Continuar.
    public void ContinueGame()
    {
        ResumeGame();
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        isPaused = false;

        // Restablecer la velocidad normal del juego.
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Cursor.visible = previousCursorVisible;
        Cursor.lockState = previousCursorLockState;

        Debug.Log("Juego reanudado.");
    }

    private void ForceResume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Cursor.visible = previousCursorVisible;
        Cursor.lockState = previousCursorLockState;
    }

    public void ReturnToLobby()
    {
        Time.timeScale = 1f;
        isPaused = false;

        SceneManager.LoadScene(lobbySceneName);
    }
}
