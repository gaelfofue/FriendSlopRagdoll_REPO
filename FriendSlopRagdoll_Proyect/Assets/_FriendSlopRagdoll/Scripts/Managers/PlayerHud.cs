
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

    [Header("Slots")]
    [SerializeField]
    private PlayerSlot[] playerSlots =
        new PlayerSlot[4];

    [Header("Colores")]
    [SerializeField]
    private Color[] playerColors =
    {
        new Color(0.22f, 0.53f, 0.91f),
        new Color(0.90f, 0.30f, 0.27f),
        new Color(0.25f, 0.68f, 0.46f),
        new Color(0.95f, 0.45f, 0.65f)
    };

    [Header("Paneles")]
    [SerializeField] private RectTransform leftPanel;
    [SerializeField] private RectTransform rightPanel;

    [Header("Animacion")]
    [SerializeField] private float slideDuration = 0.45f;
    [SerializeField] private float visibleDuration = 5f;

    private Vector2 leftShownPosition;
    private Vector2 rightShownPosition;
    private Vector2 leftHiddenPosition;
    private Vector2 rightHiddenPosition;

    private Coroutine animationRoutine;
    private bool isVisible;

    private PlayerIdentity[] players = new PlayerIdentity[4];

    private IEnumerator Start()
    {
        if (leftPanel == null || rightPanel == null)
        {
            Debug.LogError("[PlayerHUD] Faltan los paneles.");
            enabled = false;
            yield break;
        }

        leftShownPosition = leftPanel.anchoredPosition;
        rightShownPosition = rightPanel.anchoredPosition;

        RectTransform leftParent = leftPanel.parent as RectTransform;
        RectTransform rightParent = rightPanel.parent as RectTransform;

        float leftWidth = leftParent != null ?
            leftParent.rect.width : Screen.width;

        float rightWidth = rightParent != null ?
            rightParent.rect.width : Screen.width;

        leftHiddenPosition = leftShownPosition -
            Vector2.right * (leftWidth + leftPanel.rect.width);

        rightHiddenPosition = rightShownPosition +
            Vector2.right * (rightWidth + rightPanel.rect.width);

        leftPanel.anchoredPosition = leftHiddenPosition;
        rightPanel.anchoredPosition = rightHiddenPosition;

        // Esperar a que el spawner cree los personajes
        // y PlayerIdentity termine de identificarlos.
        int expectedPlayers = LobbyTransferData.Players.Count;

        while (true)
        {
            PlayerIdentity[] found =
                FindObjectsOfType<PlayerIdentity>();

            int identifiedPlayers = 0;

            foreach (PlayerIdentity player in found)
            {
                if (player != null && player.PlayerIndex >= 0)
                    identifiedPlayers++;
            }

            if (identifiedPlayers >= expectedPlayers)
                break;

            yield return null;
        }

        RefreshPlayers();
        RefreshHUD();

        Debug.Log("[PlayerHUD] Conexión de jugadores completada.");
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerStatsChanged += RefreshHUD;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerStatsChanged -= RefreshHUD;

        UnsubscribeFromPlayers();

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }
    }

    private void RefreshPlayers()
    {
        UnsubscribeFromPlayers();

        players = new PlayerIdentity[4];

        PlayerIdentity[] foundPlayers =
            FindObjectsOfType<PlayerIdentity>();

        foreach (PlayerIdentity player in foundPlayers)
        {
            if (player == null) continue;

            int index = player.PlayerIndex;

            if (index < 0 || index >= players.Length)
                continue;

            players[index] = player;

            if (player.InputRelay != null)
                player.InputRelay.OnHUDPressed += ToggleHUD;
        }

        Debug.Log("[PlayerHUD] Jugadores conectados al HUD.");
    }

    private void UnsubscribeFromPlayers()
    {
        if (players == null) return;

        foreach (PlayerIdentity player in players)
        {
            if (player != null && player.InputRelay != null)
                player.InputRelay.OnHUDPressed -= ToggleHUD;
        }
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
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        isVisible = true;
        animationRoutine = StartCoroutine(ShowRoutine());
    }

    public void HideHUD()
    {
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
            PlayerSlot slot = playerSlots[i];

            if (slot == null) continue;

            if (slot.winsText != null)
                slot.winsText.text =
                    GameManager.Instance.GetScore(i).ToString();

            if (slot.armsText != null)
                slot.armsText.text =
                    GameManager.Instance.GetArmsRipped(i).ToString();

            if (slot.playerColor != null &&
                i < playerColors.Length)
            {
                slot.playerColor.color = playerColors[i];
            }
        }
    }
}
