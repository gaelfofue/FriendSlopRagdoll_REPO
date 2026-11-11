
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MinigameIntroController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Light stageSpotlight;
    [SerializeField] private Light directionalLight;
    [SerializeField] private float finalDirectionalIntensity = 0.5f;
    [SerializeField] private float directionalFadeDuration = 1.5f;
    [SerializeField] private Image blackScreen;
    [SerializeField] private ParchisMinigameManager minigameManager;

    [Header("Iluminación")]
    [SerializeField] private float finalLightIntensity = 750f;
    [SerializeField] private float flickerDuration = 4f;

    [Header("Encendido")]
    [SerializeField] private float finalFadeDuration = 1.5f;
    [SerializeField] private float blackScreenFadeDuration = 2f;

    private void Awake()
    {
        // Apagar el foco del escenario.
        if (stageSpotlight != null)
        {
            stageSpotlight.intensity = 0f;
        }

        // Apagar la luz direccional.
        if (directionalLight != null)
        {
            directionalLight.intensity = 0f;
        }

        // Mantener la pantalla completamente negra.
        if (blackScreen != null)
        {
            Color color = blackScreen.color;
            color.a = 1f;
            blackScreen.color = color;
            blackScreen.gameObject.SetActive(true);
        }
    }

    private void Start()
    {
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        if (stageSpotlight == null ||
            directionalLight == null ||
            blackScreen == null ||
            minigameManager == null)
        {
            Debug.LogError(
                "[MinigameIntroController] Faltan referencias."
            );
            yield break;
        }

        // Esperar a que el minijuego termine de prepararse.
        while (!minigameManager.IsInitialized)
        {
            yield return null;
        }

        // REVELAR POCO A POCO EL ESCENARIO.
        float revealElapsed = 0f;

        while (revealElapsed < blackScreenFadeDuration)
        {
            revealElapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                revealElapsed / blackScreenFadeDuration
            );

            Color color = blackScreen.color;
            color.a = 1f - t;
            blackScreen.color = color;

            yield return null;
        }

        blackScreen.color = new Color(0f, 0f, 0f, 0f);
        blackScreen.gameObject.SetActive(false);

        // PARPADEO IRREGULAR CON TRANSICIONES SUAVES.
        float flickerElapsed = 0f;

        while (flickerElapsed < flickerDuration)
        {
            float chance = Random.value;
            float targetIntensity;

            if (chance < 0.25f)
            {
                // Luz residual.
                targetIntensity = Random.Range(
                    finalLightIntensity * 0.005f,
                    finalLightIntensity * 0.015f
                );
            }
            else if (chance < 0.55f)
            {
                // Luz débil.
                targetIntensity = Random.Range(
                    finalLightIntensity * 0.03f,
                    finalLightIntensity * 0.12f
                );
            }
            else if (chance < 0.85f)
            {
                // Destello fuerte.
                targetIntensity = Random.Range(
                    finalLightIntensity * 0.45f,
                    finalLightIntensity * 0.8f
                );
            }
            else
            {
                // Chispazo máximo.
                targetIntensity = finalLightIntensity;
            }

            float transitionDuration = Random.Range(0.06f, 0.18f);
            float transitionElapsed = 0f;

            float flickerStartIntensity = stageSpotlight.intensity;

            // Transición hacia la nueva intensidad.
            while (transitionElapsed < transitionDuration)
            {
                transitionElapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(
                    transitionElapsed / transitionDuration
                );

                stageSpotlight.intensity = Mathf.Lerp(
                    flickerStartIntensity,
                    targetIntensity,
                    t
                );

                yield return null;
            }

            stageSpotlight.intensity = targetIntensity;

            float pauseDuration = Random.Range(0.03f, 0.16f);

            yield return new WaitForSecondsRealtime(pauseDuration);

            flickerElapsed += transitionDuration + pauseDuration;
        }

        // ENCENDIDO DEFINITIVO DEL FOCO.
        float finalStartIntensity = stageSpotlight.intensity;
        float finalElapsed = 0f;

        while (finalElapsed < finalFadeDuration)
        {
            finalElapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                finalElapsed / finalFadeDuration
            );

            t = Mathf.SmoothStep(0f, 1f, t);

            stageSpotlight.intensity = Mathf.Lerp(
                finalStartIntensity,
                finalLightIntensity,
                t
            );

            yield return null;
        }

        stageSpotlight.intensity = finalLightIntensity;

        // TRANSICIÓN SUAVE DE LA LUZ DIRECCIONAL.
        float directionalElapsed = 0f;

        while (directionalElapsed < directionalFadeDuration)
        {
            directionalElapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                directionalElapsed / directionalFadeDuration
            );

            t = Mathf.SmoothStep(0f, 1f, t);

            directionalLight.intensity = Mathf.Lerp(
                0f,
                finalDirectionalIntensity,
                t
            );

            yield return null;
        }

        directionalLight.intensity = finalDirectionalIntensity;

        // DESBLOQUEAR A LOS JUGADORES.
        minigameManager.UnlockPlayers();

        // COMENZAR EL MINIJUEGO.
        minigameManager.StartMinigame();

        Debug.Log(
            "[MinigameIntroController] ¡Introducción terminada!"
        );
    }
}
