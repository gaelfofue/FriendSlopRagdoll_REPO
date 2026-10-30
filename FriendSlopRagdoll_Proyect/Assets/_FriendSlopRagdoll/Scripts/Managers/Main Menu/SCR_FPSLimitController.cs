using TMPro;
using UnityEngine;

public class FPSLimitController : MonoBehaviour
{
    private const string FPSLimitKey = "Settings_FPSLimit";
    private const int DefaultFPSLimit = 1;

    [Header("Desplegable de límite de FPS")]
    [SerializeField] private TMP_Dropdown fpsLimitDropdown;

    private int pendingFPSLimit;
    private int appliedFPSLimit;

    private readonly int[] fpsValues = { 30, 60, 120, -1 };

    private void Start()
    {
        if (fpsLimitDropdown == null)
        {
            Debug.LogWarning(
                "FPSLimitController: falta asignar el desplegable.",
                this
            );
            return;
        }

        fpsLimitDropdown.onValueChanged.AddListener(OnFPSLimitChanged);

        appliedFPSLimit = Mathf.Clamp(
            PlayerPrefs.GetInt(FPSLimitKey, DefaultFPSLimit),
            0,
            3
        );

        pendingFPSLimit = appliedFPSLimit;

        UpdateDropdown(pendingFPSLimit);
        ApplyFPSLimit(appliedFPSLimit);
    }

    private void OnDestroy()
    {
        if (fpsLimitDropdown != null)
        {
            fpsLimitDropdown.onValueChanged.RemoveListener(OnFPSLimitChanged);
        }
    }

    private void OnFPSLimitChanged(int index)
    {
        pendingFPSLimit = index;
    }

    public void ApplyChanges()
    {
        appliedFPSLimit = pendingFPSLimit;

        PlayerPrefs.SetInt(FPSLimitKey, appliedFPSLimit);
        PlayerPrefs.Save();

        ApplyFPSLimit(appliedFPSLimit);

        Debug.Log("Límite de FPS guardado: " + GetFPSValue(appliedFPSLimit));
    }

    public void ResetChanges()
    {
        pendingFPSLimit = DefaultFPSLimit;
        UpdateDropdown(pendingFPSLimit);

        ApplyChanges();

        Debug.Log("Límite de FPS restablecido a 60.");
    }

    private void UpdateDropdown(int index)
    {
        fpsLimitDropdown.SetValueWithoutNotify(index);
        fpsLimitDropdown.RefreshShownValue();
    }

    private int GetFPSValue(int index)
    {
        return fpsValues[index];
    }

    private void ApplyFPSLimit(int index)
    {
        int targetFPS = GetFPSValue(index);

        QualitySettings.vSyncCount = 0;

        Application.targetFrameRate = targetFPS;
    }
}