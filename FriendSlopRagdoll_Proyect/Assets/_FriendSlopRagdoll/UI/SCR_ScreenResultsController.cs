using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ResultsScreenController : MonoBehaviour
{
    [Header("Panel de resultados")]
    [SerializeField] private GameObject resultsPanel;

    [Header("Clasificación")]
    [SerializeField] private ResultsLeaderboardUI leaderboardUI;

    [Header("Botón continuar")]
    [SerializeField] private Button continueButton;

    private float previousTimeScale = 1f;
    private bool gamePaused = false;
    private bool resultsReady = false;

    private void Awake()
    {
        if (resultsPanel != null)
            resultsPanel.SetActive(false);
    }

    public void ShowResults()
    {
        if (resultsPanel == null)
        {
            Debug.LogError("[ResultsScreenController] Falta ResultsPanel.");
            return;
        }

        if (gamePaused)
            return;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        gamePaused = true;
        resultsReady = false;

        if (continueButton != null)
            continueButton.interactable = false;

        resultsPanel.SetActive(true);

        if (leaderboardUI != null)
            leaderboardUI.RefreshLeaderboard();

        StartCoroutine(VictoryPresentation());
    }

    private IEnumerator VictoryPresentation()
    {
        // Dejamos que todos vean la clasificación anterior.
        yield return new WaitForSecondsRealtime(1f);

        if (GameManager.Instance == null)
        {
            Debug.LogError("[ResultsScreenController] No se encontró GameManager.");
            EnableContinue();
            yield break;
        }

        int winnerId = GameManager.Instance.PendingWinnerId;

        if (winnerId >= 0)
        {
            // Sumamos la victoria pendiente.
            GameManager.Instance.ApplyPendingRoundVictory();

            int newWins = GameManager.Instance.GetScore(winnerId);

            // Reproducimos el +1 en la fila del ganador.
            // Reproducimos el +1 en la fila del ganador.
            if (leaderboardUI != null)
                leaderboardUI.PlayVictoryUpdate(winnerId, newWins); 

            // Esperamos a que se vea el +1.
            yield return new WaitForSecondsRealtime(1.5f);

            // Actualizamos la clasificación con movimiento.
            if (leaderboardUI != null)
                yield return leaderboardUI.AnimateLeaderboardUpdate();
        }
        else
        {
            Debug.LogWarning("[ResultsScreenController] No hay ningún ganador pendiente.");
        }

        EnableContinue();
    }

    private void EnableContinue()
    {
        resultsReady = true;

        if (continueButton != null)
            continueButton.interactable = true;
    }

    public void Continue()
    {
        if (!resultsReady)
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogError("[ResultsScreenController] No se encontró GameManager.");
            return;
        }

        if (gamePaused)
        {
            Time.timeScale = previousTimeScale;
            gamePaused = false;
        }

        resultsPanel.SetActive(false);
        GameManager.Instance.ContinueAfterResults();
    }

    private void OnDestroy()
    {
        if (gamePaused)
        {
            Time.timeScale = previousTimeScale;
            gamePaused = false;
        }
    }
}