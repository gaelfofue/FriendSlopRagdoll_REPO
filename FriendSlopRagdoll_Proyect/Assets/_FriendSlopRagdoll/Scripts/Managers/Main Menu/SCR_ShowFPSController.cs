using UnityEngine;
using UnityEngine.UI;

public class ShowFPSController : MonoBehaviour
{
    private const string ShowFPSKey = "Settings_ShowFPS";
    private const bool DefaultShowFPS = false;

    [Header("Casilla Mostrar FPS")]
    [SerializeField] private Toggle showFPSToggle;

    private bool pendingShowFPS;

    private void Start()
    {
        if (showFPSToggle == null)
        {
            Debug.LogWarning(
                "ShowFPSController: falta asignar la casilla.",
                this
            );
            return;
        }

        pendingShowFPS = PlayerPrefs.GetInt(
            ShowFPSKey,
            DefaultShowFPS ? 1 : 0
        ) == 1;

        showFPSToggle.SetIsOnWithoutNotify(pendingShowFPS);

        showFPSToggle.onValueChanged.AddListener(OnShowFPSChanged);
    }

    private void OnDestroy()
    {
        if (showFPSToggle != null)
        {
            showFPSToggle.onValueChanged.RemoveListener(OnShowFPSChanged);
        }
    }

    private void OnShowFPSChanged(bool value)
    {
        pendingShowFPS = value;
    }

    public void ApplyChanges()
    {
        FPSManager manager = FindFirstObjectByType<FPSManager>();

        if (manager == null)
        {
            Debug.LogWarning("ShowFPSController: no se encuentra FPSManager.");
            return;
        }

        manager.SetShowFPS(pendingShowFPS);

        Debug.Log("Mostrar FPS: " + pendingShowFPS);
    }

    public void ResetChanges()
    {
        pendingShowFPS = DefaultShowFPS;

        showFPSToggle.SetIsOnWithoutNotify(pendingShowFPS);

        ApplyChanges();

        Debug.Log("Mostrar FPS restablecido.");
    }
}