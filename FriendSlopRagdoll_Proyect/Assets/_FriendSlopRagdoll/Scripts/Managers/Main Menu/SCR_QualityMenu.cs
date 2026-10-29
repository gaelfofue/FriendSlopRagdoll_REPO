using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GraphicsQualityController : MonoBehaviour
{
    private const string GraphicsQualityKey = "Settings_GraphicsQuality";
    private const int DefaultGraphicsQuality = 1;

    [Header("Desplegable de calidad")]
    [SerializeField] private TMP_Dropdown graphicsQualityDropdown;

    [Header("Perfiles URP")]
    [SerializeField] private UniversalRenderPipelineAsset urpLow;
    [SerializeField] private UniversalRenderPipelineAsset urpMedium;
    [SerializeField] private UniversalRenderPipelineAsset urpHigh;

    private int pendingQuality;
    private int appliedQuality;

    private void Start()
    {
        if (graphicsQualityDropdown == null)
        {
            Debug.LogWarning(
                "GraphicsQualityController: falta asignar el desplegable.",
                this
            );
            return;
        }

        graphicsQualityDropdown.onValueChanged.AddListener(
            OnGraphicsQualityChanged
        );

        appliedQuality = Mathf.Clamp(
            PlayerPrefs.GetInt(
                GraphicsQualityKey,
                DefaultGraphicsQuality
            ),
            0,
            2
        );

        pendingQuality = appliedQuality;

        UpdateDropdown(pendingQuality);
        ApplyGraphicsQuality(appliedQuality);
    }

    private void OnDestroy()
    {
        if (graphicsQualityDropdown != null)
        {
            graphicsQualityDropdown.onValueChanged.RemoveListener(
                OnGraphicsQualityChanged
            );
        }
    }

    private void OnGraphicsQualityChanged(int index)
    {
        pendingQuality = index;
    }

    public void ApplyChanges()
    {
        if (!IsValidQuality(pendingQuality))
        {
            Debug.LogWarning(
                "GraphicsQualityController: el perfil seleccionado no está asignado.",
                this
            );
            return;
        }

        appliedQuality = pendingQuality;

        PlayerPrefs.SetInt(GraphicsQualityKey, appliedQuality);
        PlayerPrefs.Save();

        ApplyGraphicsQuality(appliedQuality);

        Debug.Log("Calidad gráfica guardada: " + appliedQuality);
    }

    public void ResetChanges()
    {
        pendingQuality = DefaultGraphicsQuality;
        UpdateDropdown(pendingQuality);

        ApplyChanges();

        Debug.Log("Calidad gráfica restablecida a Media.");
    }

    private void UpdateDropdown(int index)
    {
        graphicsQualityDropdown.SetValueWithoutNotify(index);
        graphicsQualityDropdown.RefreshShownValue();
    }

    private bool IsValidQuality(int index)
    {
        switch (index)
        {
            case 0:
                return urpLow != null;

            case 1:
                return urpMedium != null;

            case 2:
                return urpHigh != null;

            default:
                return false;
        }
    }

    private void ApplyGraphicsQuality(int index)
    {
        UniversalRenderPipelineAsset selectedAsset = null;

        switch (index)
        {
            case 0:
                selectedAsset = urpLow;
                break;

            case 1:
                selectedAsset = urpMedium;
                break;

            case 2:
                selectedAsset = urpHigh;
                break;
        }

        if (selectedAsset == null)
        {
            Debug.LogWarning(
                "GraphicsQualityController: falta asignar el perfil URP.",
                this
            );
            return;
        }

        QualitySettings.renderPipeline = selectedAsset;
    }
}