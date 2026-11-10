using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardPlayerRowUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TMP_Text positionText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text winsText;
    [SerializeField] private Image playerColorCircle;
    [SerializeField] private TMP_Text victoryChangeText;
    [SerializeField] private GameObject positionUpArrow;
    [SerializeField] private GameObject positionDownArrow;

    private Coroutine victoryAnimation;
    private int playerId;

    public int PlayerId => playerId;

    public void Setup(int playerId, int position, string playerName, int wins)
    {
        this.playerId = playerId;
        positionText.text = position + ".º";
        playerNameText.text = playerName;
        winsText.text = wins + " victorias";

        if (victoryChangeText != null)
            victoryChangeText.gameObject.SetActive(false);

        if (positionUpArrow != null)
            positionUpArrow.SetActive(false);

        if (positionDownArrow != null)
            positionDownArrow.SetActive(false);
    }

    public void SetPlayerColor(Color color)
    {
        if (playerColorCircle != null)
            playerColorCircle.color = color;
    }

    public void PlayVictoryUpdate(int newWins)
    {
        if (victoryChangeText == null)
        {
            Debug.LogWarning("[LeaderboardPlayerRowUI] Falta VictoryChangeText.");
            return;
        }

        if (victoryAnimation != null)
            StopCoroutine(victoryAnimation);

        victoryAnimation = StartCoroutine(VictoryAnimation(newWins));
    }

    private IEnumerator VictoryAnimation(int newWins)
    {
        victoryChangeText.gameObject.SetActive(true);
        victoryChangeText.text = "+1";

        RectTransform rect = victoryChangeText.rectTransform;
        Vector3 originalScale = Vector3.one;

        rect.localScale = originalScale * 0.5f;

        Color originalColor = victoryChangeText.color;
        Color animatedColor = originalColor;
        animatedColor.a = 1f;
        victoryChangeText.color = animatedColor;

        float duration = 0.65f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            rect.localScale = Vector3.Lerp(
                originalScale * 0.5f,
                originalScale,
                Mathf.SmoothStep(0f, 1f, progress)
            );

            yield return null;
        }

        rect.localScale = originalScale;

        // Actualizamos el contador cuando termina la animación.
        winsText.text = newWins + " victorias";

        yield return new WaitForSecondsRealtime(0.4f);

        float fadeDuration = 0.3f;
        elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / fadeDuration);

            Color fadedColor = originalColor;
            fadedColor.a = 1f - progress;
            victoryChangeText.color = fadedColor;

            yield return null;
        }

        victoryChangeText.gameObject.SetActive(false);
        victoryChangeText.color = originalColor;
        rect.localScale = originalScale;

        victoryAnimation = null;
    }

    public void UpdatePosition(int newPosition)
    {
        positionText.text = newPosition + ".º";
    }

    public void ShowPositionChange(bool movedUp)
    {
        if (positionUpArrow != null)
            positionUpArrow.SetActive(movedUp);

        if (positionDownArrow != null)
            positionDownArrow.SetActive(!movedUp);
    }


    public IEnumerator AnimateToPosition(
        Vector2 targetPosition,
        bool movedUp,
        float duration
    )
    {
        RectTransform rect = GetComponent<RectTransform>();

        if (rect == null)
            yield break;

        ShowPositionChange(movedUp);

        Vector2 startPosition = rect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            rect.anchoredPosition = Vector2.Lerp(
                startPosition,
                targetPosition,
                smoothProgress
            );

            yield return null;
        }

        rect.anchoredPosition = targetPosition;
    }

    public void HidePositionChangeArrows()
    {
        if (positionUpArrow != null)
            positionUpArrow.SetActive(false);

        if (positionDownArrow != null)
            positionDownArrow.SetActive(false);
    }
}