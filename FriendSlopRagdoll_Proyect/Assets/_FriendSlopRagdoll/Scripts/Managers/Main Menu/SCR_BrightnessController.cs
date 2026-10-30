using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class BrightnessController : MonoBehaviour
{
    private const string BrightnessKey = "Settings_Brightness";
    private const float DefaultBrightness = 75f;

    [Header("Control de brillo")]
    [SerializeField] private Slider brightnessSlider;

    [Header("Volume")]
    [SerializeField] private Volume brightnessVolume;

    private ColorAdjustments colorAdjustments;
    private float pendingBrightness;

    private void Start()
    {
        if (brightnessSlider == null || brightnessVolume == null)
        {
            Debug.LogWarning(
                "BrightnessController: faltan referencias.",
                this
            );
            return;
        }

        if (!brightnessVolume.profile.TryGet(
            out colorAdjustments))
        {
            Debug.LogWarning(
                "BrightnessController: falta Color Adjustments en el Volume.",
                this
            );
            return;
        }

        pendingBrightness = PlayerPrefs.GetFloat(
            BrightnessKey,
            DefaultBrightness
        );

        brightnessSlider.minValue = 0f;
        brightnessSlider.maxValue = 100f;

        brightnessSlider.SetValueWithoutNotify(pendingBrightness);

        brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);

        ApplyBrightness(pendingBrightness);
    }

    private void OnDestroy()
    {
        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.RemoveListener(
                OnBrightnessChanged
            );
        }
    }

    private void OnBrightnessChanged(float value)
    {
        pendingBrightness = value;
        ApplyBrightness(pendingBrightness);
    }

    public void ApplyChanges()
    {
        PlayerPrefs.SetFloat(BrightnessKey, pendingBrightness);
        PlayerPrefs.Save();

        ApplyBrightness(pendingBrightness);

        Debug.Log("Brillo guardado: " + pendingBrightness);
    }

    public void ResetChanges()
    {
        pendingBrightness = DefaultBrightness;

        brightnessSlider.SetValueWithoutNotify(
            DefaultBrightness
        );

        ApplyChanges();

        Debug.Log("Brillo restablecido al 75%.");
    }

    private void ApplyBrightness(float value)
    {
        if (colorAdjustments == null)
            return;

        float exposure = Mathf.Lerp(-2f, 2f, value / 100f);

        colorAdjustments.postExposure.value = exposure;
    }
}