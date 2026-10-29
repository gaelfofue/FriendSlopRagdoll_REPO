using TMPro;
using UnityEngine;

public class ScreenSettingsController : MonoBehaviour
{
    [Header("Controles de pantalla")]
    [SerializeField] private TMP_Dropdown displayModeDropdown;

    private void Start()
    {
        if (displayModeDropdown == null)
        {
            Debug.LogWarning(
                "ScreenSettingsController: falta asignar el desplegable."
            );
            return;
        }

        displayModeDropdown.onValueChanged.AddListener(
            ApplyDisplayMode
        );

        LoadCurrentDisplayMode();
    }

    private void OnDestroy()
    {
        if (displayModeDropdown != null)
        {
            displayModeDropdown.onValueChanged.RemoveListener(
                ApplyDisplayMode
            );
        }
    }

    private void LoadCurrentDisplayMode()
    {
        int currentIndex;

        if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
        {
            currentIndex = 0;
        }
        else if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
        {
            currentIndex = 1;
        }
        else
        {
            currentIndex = 2;
        }

        displayModeDropdown.SetValueWithoutNotify(currentIndex);
        displayModeDropdown.RefreshShownValue();
    }

    private void ApplyDisplayMode(int index)
    {
        Debug.Log("Modo de pantalla seleccionado: " + index);

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

        Screen.SetResolution(
            Screen.width,
            Screen.height,
            mode
        );

        Debug.Log("Modo solicitado: " + mode);
    }

}

