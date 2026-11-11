using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreChangePopup : MonoBehaviour
{
    [Header("Animación")]
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private float riseDistance = 30f;

    [Header("Colores")]
    [SerializeField] private Color addColor = Color.green;
    [SerializeField] private Color removeColor = Color.red;

    private TMP_Text popupText;
    private RectTransform rectTransform;
    private Vector2 startPosition;
    private Coroutine currentAnimation;

    private void Awake()
    {
        popupText = GetComponent<TMP_Text>();
        rectTransform = GetComponent<RectTransform>();

        startPosition = rectTransform.anchoredPosition;

        popupText.text = "";
        popupText.enabled = false;
    }

    public void ShowChange(float amount)
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(AnimateChange(amount));
    }

    private IEnumerator AnimateChange(float amount)
    {
        popupText.enabled = true;

        popupText.text = amount > 0
            ? "+" + amount.ToString("0.##")
            : amount.ToString("0.##");

        popupText.color = amount > 0 ? addColor : removeColor;

        rectTransform.anchoredPosition = startPosition;

        Color originalColor = popupText.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            rectTransform.anchoredPosition =
                startPosition + Vector2.up * riseDistance * progress;

            Color fadedColor = originalColor;
            fadedColor.a = 1f - progress;

            popupText.color = fadedColor;

            yield return null;
        }

        popupText.text = "";
        popupText.enabled = false;
        rectTransform.anchoredPosition = startPosition;
        popupText.color = originalColor;

        currentAnimation = null;
    }
}