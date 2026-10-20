using System;
using System.Collections;
using UnityEngine;
using Cinemachine;

public class WinnerSequence : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineVirtualCamera winnerVCam;
    [SerializeField] private float zoomFOV = 30f;

    [Header("Slow Motion")]
    [SerializeField] private float slowMotionTimeScale = 0.2f;
    [SerializeField] private float slowMotionDuration = 3f;

    public event Action OnSequenceFinished;

    private float originalFixedDeltaTime;
    private bool isPlaying = false;

    void Awake()
    {
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    public void PlaySequence(GameObject followTarget, GameObject lookAtTarget)
    {
        if (isPlaying) return;
        isPlaying = true;
        StartCoroutine(SequenceRoutine(followTarget, lookAtTarget));
    }

    private IEnumerator SequenceRoutine(GameObject followTarget, GameObject lookAtTarget)
    {
        if (winnerVCam != null)
        {
            winnerVCam.Follow = followTarget.transform;
            winnerVCam.LookAt = lookAtTarget.transform;

            var lens = winnerVCam.m_Lens;
            lens.FieldOfView = zoomFOV;
            winnerVCam.m_Lens = lens;

            winnerVCam.Priority = 100;
        }

        Time.timeScale = slowMotionTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;

        yield return new WaitForSecondsRealtime(slowMotionDuration);

        // Siempre restauramos, aunque haya excepciones raras
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        isPlaying = false;
        OnSequenceFinished?.Invoke();
    }
}