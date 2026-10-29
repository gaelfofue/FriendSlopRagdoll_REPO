
using System;
using System.Collections;
using UnityEngine;

public class WinnerSequence : MonoBehaviour
{
    [Header("Cámara")]
    [SerializeField] private Camera mainCamera;

    [Header("Movimiento y zoom")]
    [SerializeField] private float zoomDistance = 5f;
    [SerializeField] private float zoomHeight = 2f;
    [SerializeField] private float zoomDuration = 1.5f;
    [SerializeField] private float targetHeight = 1f;
    [SerializeField] private float winnerFOV = 30f;

    [Header("Cámara lenta")]
    [SerializeField] private float slowMotionTimeScale = 0.2f;
    [SerializeField] private float slowMotionDuration = 3f;

    public event Action OnSequenceFinished;

    private float originalFixedDeltaTime;
    private float originalTimeScale;
    private bool isPlaying = false;

    void Awake()
    {
        originalFixedDeltaTime = Time.fixedDeltaTime;
        originalTimeScale = Time.timeScale;

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    public void PlaySequence(
        GameObject followTarget,
        GameObject lookAtTarget)
    {
        if (isPlaying) return;

        if (mainCamera == null || followTarget == null)
        {
            Debug.LogError(
                "WinnerSequence: falta cámara o ganador."
            );
            OnSequenceFinished?.Invoke();
            return;
        }

        isPlaying = true;

        StartCoroutine(
            SequenceRoutine(followTarget.transform)
        );
    }

    private IEnumerator SequenceRoutine(Transform winner)
    {
        Transform cam = mainCamera.transform;

        Vector3 startPosition = cam.position;
        Quaternion startRotation = cam.rotation;
        float startFOV = mainCamera.fieldOfView;

        Vector3 direction =
            (startPosition - winner.position).normalized;

        if (direction.sqrMagnitude < 0.001f)
            direction = -winner.forward;

        Vector3 targetPosition =
            winner.position +
            direction * zoomDistance +
            Vector3.up * zoomHeight;

        Vector3 lookPosition =
            winner.position + Vector3.up * targetHeight;

        Quaternion targetRotation =
            Quaternion.LookRotation(
                lookPosition - targetPosition
            );

        // Activar cámara lenta
        Time.timeScale = slowMotionTimeScale;
        Time.fixedDeltaTime =
            originalFixedDeltaTime * Time.timeScale;

        // Zoom y movimiento suave
        float elapsed = 0f;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, zoomDuration)
            );

            t = Mathf.SmoothStep(0f, 1f, t);

            if (winner == null) break;

            direction =
                (startPosition - winner.position).normalized;

            targetPosition =
                winner.position +
                direction * zoomDistance +
                Vector3.up * zoomHeight;

            lookPosition =
                winner.position + Vector3.up * targetHeight;

            targetRotation = Quaternion.LookRotation(
                lookPosition - targetPosition
            );

            cam.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                t
            );

            cam.rotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                t
            );

            mainCamera.fieldOfView = Mathf.Lerp(
                startFOV,
                winnerFOV,
                t
            );

            yield return null;
        }

        // Mantener la cámara enfocada en el ganador
        float holdTime = Mathf.Max(
            0f,
            slowMotionDuration - zoomDuration
        );

        float holdElapsed = 0f;

        while (holdElapsed < holdTime)
        {
            holdElapsed += Time.unscaledDeltaTime;

            if (winner != null)
            {
                cam.LookAt(
                    winner.position + Vector3.up * targetHeight
                );
            }

            yield return null;
        }

        // Restaurar tiempo normal
        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        isPlaying = false;

        OnSequenceFinished?.Invoke();
    }

    void OnDisable()
    {
        if (isPlaying)
        {
            Time.timeScale = originalTimeScale;
            Time.fixedDeltaTime = originalFixedDeltaTime;
            isPlaying = false;
        }
    }
}
