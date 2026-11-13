
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerInputRelay inputRelay;
    [SerializeField] private GameObject pausePanel;

    [Header("Escenas")]
    [SerializeField] private string lobbySceneName = "Lobby";

    private bool isPaused;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (inputRelay == null)
            inputRelay = FindObjectOfType<PlayerInputRelay>();
    }

    private void OnEnable()
    {
        if (inputRelay != null)
            inputRelay.OnHUDPressed += TogglePause;
    }

    private void OnDisable()
    {
        if (inputRelay != null)
            inputRelay.OnHUDPressed -= TogglePause;
    }

    private void Update()
    {
        // Permite mantener sincronizado el estado si se cambia
        // la pausa desde otro sistema.
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
        previousTimeScale = Time.timeScale;

        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        isPaused = false;

        Time.timeScale = previousTimeScale;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ReturnToLobby()
    {
        Time.timeScale = 1f;
        isPaused = false;

        SceneManager.LoadScene(lobbySceneName);
    }
}