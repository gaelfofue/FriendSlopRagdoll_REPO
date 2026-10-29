using TMPro;
using UnityEngine;

public class ScreenSettingsController : MonoBehaviour
{
    private const string ScreenModeKey = "Settings_ScreenMode";
    private const int DefaultScreenMode = 1;

    [Header("Controles de pantalla")]
    [SerializeField] private TMP_Dropdown displayModeDropdown;

    private int pendingScreenMode;
    private int appliedScreenMode;

    private void Start()
    {
        if (displayModeDropdown == null)
        {
            Debug.LogWarning(
                "ScreenSettingsController: falta asignar el desplegable.",
                this
            );
            return;
        }

        displayModeDropdown.onValueChanged.AddListener(
            OnScreenModeChanged
        );

        appliedScreenMode = PlayerPrefs.GetInt(
            ScreenModeKey,
            DefaultScreenMode
        );

        pendingScreenMode = appliedScreenMode;

        UpdateDropdown(pendingScreenMode);
        ApplyScreenMode(appliedScreenMode);
    }

    private void OnDestroy()
    {
        if (displayModeDropdown != null)
        {
            displayModeDropdown.onValueChanged.RemoveListener(
                OnScreenModeChanged
            );
        }
    }

    private void OnScreenModeChanged(int index)
    {
        pendingScreenMode = index;
    }

    public void ApplyChanges()
    {
        appliedScreenMode = pendingScreenMode;

        PlayerPrefs.SetInt(ScreenModeKey, appliedScreenMode);
        PlayerPrefs.Save();

        ApplyScreenMode(appliedScreenMode);

        Debug.Log("Modo de pantalla guardado: " + appliedScreenMode);
    }

    public void ResetChanges()
    {
        pendingScreenMode = DefaultScreenMode;
        UpdateDropdown(pendingScreenMode);

        ApplyChanges();

        Debug.Log("Ajustes de pantalla restablecidos.");
    }

    private void UpdateDropdown(int index)
    {
        displayModeDropdown.SetValueWithoutNotify(index);
        displayModeDropdown.RefreshShownValue();
    }

    private void ApplyScreenMode(int index)
    {
        FullScreenMode mode;

        switch (index)
        {
            case 0:
                mode = FullScreenMode.ExclusiveFullScreen;
                break;

            case 1:
                mode = FullScreenMode.FullScreenWindow;
                break;

            case 2:
                mode = FullScreenMode.Windowed;
                break;

            default:
                return;
        }

        Screen.SetResolution(Screen.width, Screen.height, mode);
    }
}
