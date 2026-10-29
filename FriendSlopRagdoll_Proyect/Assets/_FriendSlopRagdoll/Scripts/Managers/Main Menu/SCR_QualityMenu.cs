using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GraphicsQualityController : MonoBehaviour
{
    [Header("Desplegable de calidad")]
    [SerializeField] private TMP_Dropdown graphicsQualityDropdown;

    [Header("Perfiles URP")]
    [SerializeField] private UniversalRenderPipelineAsset urpLow;
    [SerializeField] private UniversalRenderPipelineAsset urpMedium;
    [SerializeField] private UniversalRenderPipelineAsset urpHigh;

    private void Start()
    {
        if (graphicsQualityDropdown == null)
        {
            Debug.LogWarning(
                "GraphicsQualityController: falta asignar el desplegable."
            );
            return;
        }

        graphicsQualityDropdown.onValueChanged.AddListener(
            ApplyGraphicsQuality
        );

        graphicsQualityDropdown.SetValueWithoutNotify(1);
        graphicsQualityDropdown.RefreshShownValue();
    }

    private void OnDestroy()
    {
        if (graphicsQualityDropdown != null)
        {
            graphicsQualityDropdown.onValueChanged.RemoveListener(
                ApplyGraphicsQuality
            );
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
                "GraphicsQualityController: falta asignar un perfil URP."
            );
            return;
        }

        QualitySettings.renderPipeline = selectedAsset;

        Debug.Log(
            "Calidad gráfica seleccionada: " + selectedAsset.name
        );
    }
}

