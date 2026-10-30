using UnityEngine;

public class SettingsMenuController : MonoBehaviour
{
    [Header("Panels de configuración")]
    [SerializeField] private GameObject screenSettingsPanel;
    [SerializeField] private GameObject controlsSettingsPanel;
    [SerializeField] private GameObject audioSettingsPanel;
    [SerializeField] private GameObject gameplaySettingsPanel;

    private GameObject currentPanel;

    private void Start()
    {
        ShowScreenSettings();
    }

    public void ShowScreenSettings()
    {
        ShowPanel(screenSettingsPanel);
    }

    public void ShowControlsSettings()
    {
        ShowPanel(controlsSettingsPanel);
    }

    public void ShowAudioSettings()
    {
        ShowPanel(audioSettingsPanel);
    }

    public void ShowGameplaySettings()
    {
        ShowPanel(gameplaySettingsPanel);
    }

    private void ShowPanel(GameObject panelToShow)
    {
        if (panelToShow == null)
        {
            Debug.LogWarning(
                "SettingsMenuController: falta asignar un panel."
            );
            return;
        }

        if (screenSettingsPanel != null)
            screenSettingsPanel.SetActive(false);

        if (controlsSettingsPanel != null)
            controlsSettingsPanel.SetActive(false);

        if (audioSettingsPanel != null)
            audioSettingsPanel.SetActive(false);

        if (gameplaySettingsPanel != null)
            gameplaySettingsPanel.SetActive(false);

        panelToShow.SetActive(true);
        currentPanel = panelToShow;
    }
}

