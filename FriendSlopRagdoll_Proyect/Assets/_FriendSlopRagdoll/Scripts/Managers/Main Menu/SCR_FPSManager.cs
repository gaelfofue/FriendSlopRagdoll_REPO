using TMPro;
using UnityEngine;

public class FPSManager : MonoBehaviour
{
    private static FPSManager instance;

    private const string ShowFPSKey = "Settings_ShowFPS";

    [Header("Contador de FPS")]
    [SerializeField] private TMP_Text fpsCounterText;

    [Header("Configuración")]
    [SerializeField] private float updateInterval = 0.5f;

    private float elapsedTime;
    private int frameCount;
    private float fps;

    private bool showFPS;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);

        showFPS = PlayerPrefs.GetInt(ShowFPSKey, 0) == 1;

        UpdateCounterVisibility();
    }

    private void Update()
    {
        if (!showFPS)
            return;

        frameCount++;
        elapsedTime += Time.unscaledDeltaTime;

        if (elapsedTime >= updateInterval)
        {
            fps = frameCount / elapsedTime;

            fpsCounterText.text = "FPS: " + Mathf.RoundToInt(fps);

            frameCount = 0;
            elapsedTime = 0f;
        }
    }

    private void UpdateCounterVisibility()
    {
        if (fpsCounterText != null)
        {
            fpsCounterText.gameObject.SetActive(showFPS);
        }
    }

    public void SetShowFPS(bool value)
    {
        showFPS = value;

        PlayerPrefs.SetInt(ShowFPSKey, showFPS ? 1 : 0);
        PlayerPrefs.Save();

        elapsedTime = 0f;
        frameCount = 0;

        UpdateCounterVisibility();
    }
}