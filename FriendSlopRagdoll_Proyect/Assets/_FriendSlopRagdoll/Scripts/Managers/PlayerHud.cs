
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [System.Serializable]
    public class PlayerSlot
    {
        public Image playerColor;
        public TMP_Text winsText;
        public TMP_Text armsText;
    }

    [Header("Inputs de los jugadores")]
    [SerializeField]
    private PlayerInputRelay[] playerInputs =
        new PlayerInputRelay[4];

    [Header("Slots de jugadores")]
    [SerializeField]
    private PlayerSlot[] playerSlots =
        new PlayerSlot[4];

    [Header("Colores")]
    [SerializeField]
    private Color[] playerColors =
    {
        new Color(0.22f, 0.53f, 0.91f),
        new Color(0.90f, 0.30f, 0.27f),
        new Color(0.95f, 0.76f, 0.20f),
        new Color(0.25f, 0.68f, 0.46f)
    };

    [Header("Paneles")]
    [SerializeField] private RectTransform leftPanel;
    [SerializeField] private RectTransform rightPanel;

    [Header("Animación")]
    [SerializeField] private float slideDuration = 0.45f;
    [SerializeField] private float visibleDuration = 5f;

    private Vector2 leftShownPosition;
    private Vector2 rightShownPosition;

    private Vector2 leftHiddenPosition;
    private Vector2 rightHiddenPosition;

    private Coroutine animationRoutine;
    private bool isVisible;

    private bool inputsSubscribed;

    private void Start()
    {
        if (leftPanel == null || rightPanel == null)
        {
            Debug.LogError("[PlayerHUD] Faltan los paneles.");
            return;
        }

        leftShownPosition = leftPanel.anchoredPosition;
        rightShownPosition = rightPanel.anchoredPosition;

        RectTransform leftParent =
            leftPanel.parent as RectTransform;

        RectTransform rightParent =
            rightPanel.parent as RectTransform;

        float leftWidth = leftParent != null
            ? leftParent.rect.width
            : Screen.width;

        float rightWidth = rightParent != null
            ? rightParent.rect.width
            : Screen.width;

        leftHiddenPosition = leftShownPosition -
            Vector2.right * (leftWidth + leftPanel.rect.width);

        rightHiddenPosition = rightShownPosition +
            Vector2.right * (rightWidth + rightPanel.rect.width);

        leftPanel.anchoredPosition = leftHiddenPosition;
        rightPanel.anchoredPosition = rightHiddenPosition;

        SubscribeInputs();
        RefreshHUD();
    }

    private void OnEnable()
    {
        SubscribeInputs();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerStatsChanged += RefreshHUD;
        }
    }

    private void OnDisable()
    {
        UnsubscribeInputs();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerStatsChanged -= RefreshHUD;
        }

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }
    }

    private void SubscribeInputs()
    {
        if (inputsSubscribed || playerInputs == null)
            return;

        foreach (PlayerInputRelay playerInput in playerInputs)
        {
            if (playerInput != null)
                playerInput.OnHUDPressed += ToggleHUD;
        }

        inputsSubscribed = true;
    }

    private void UnsubscribeInputs()
    {
        if (!inputsSubscribed || playerInputs == null)
            return;

        foreach (PlayerInputRelay playerInput in playerInputs)
        {
            if (playerInput != null)
                playerInput.OnHUDPressed -= ToggleHUD;
        }

        inputsSubscribed = false;
    }

    public void ToggleHUD()
    {
        if (isVisible)
            HideHUD();
        else
            ShowHUD();
    }

    public void ShowHUD()
    {
        if (leftPanel == null || rightPanel == null)
            return;

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        isVisible = true;
        animationRoutine = StartCoroutine(ShowRoutine());
    }

    public void HideHUD()
    {
        if (leftPanel == null || rightPanel == null)
            return;

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        isVisible = false;
        animationRoutine = StartCoroutine(HideRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        yield return AnimatePanels(
            leftPanel.anchoredPosition,
            leftShownPosition,
            rightPanel.anchoredPosition,
            rightShownPosition
        );

        if (isVisible)
        {
            yield return new WaitForSecondsRealtime(visibleDuration);

            if (isVisible)
                HideHUD();
        }

        animationRoutine = null;
    }

    private IEnumerator HideRoutine()
    {
        yield return AnimatePanels(
            leftPanel.anchoredPosition,
            leftHiddenPosition,
            rightPanel.anchoredPosition,
            rightHiddenPosition
        );

        animationRoutine = null;
    }

    private IEnumerator AnimatePanels(
        Vector2 leftStart,
        Vector2 leftEnd,
        Vector2 rightStart,
        Vector2 rightEnd)
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, slideDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            leftPanel.anchoredPosition =
                Vector2.Lerp(leftStart, leftEnd, t);

            rightPanel.anchoredPosition =
                Vector2.Lerp(rightStart, rightEnd, t);

            yield return null;
        }

        leftPanel.anchoredPosition = leftEnd;
        rightPanel.anchoredPosition = rightEnd;
    }

    public void RefreshHUD()
    {
        if (GameManager.Instance == null)
            return;

        for (int i = 0; i < playerSlots.Length; i++)
        {
            if (playerSlots[i] == null)
                continue;

            int wins = GameManager.Instance.GetScore(i);
            int arms = GameManager.Instance.GetArmsRipped(i);

            if (playerSlots[i].winsText != null)
                playerSlots[i].winsText.text = wins.ToString();

            if (playerSlots[i].armsText != null)
                playerSlots[i].armsText.text = arms.ToString();

            if (playerSlots[i].playerColor != null &&
                i < playerColors.Length)
            {
                playerSlots[i].playerColor.color =
                    playerColors[i];
            }
        }
    }
}
