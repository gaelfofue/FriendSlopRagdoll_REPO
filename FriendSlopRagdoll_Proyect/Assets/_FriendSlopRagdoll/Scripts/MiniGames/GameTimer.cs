using System;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private float matchDuration = 120f; // 2 minutos

    private float remainingTime;
    private bool isRunning = false;

    public float RemainingTime => remainingTime;
    public bool IsOvertime { get; private set; } = false;

    // Evento que se dispara cuando el timer llega a 0 por primera vez
    public event Action OnTimerFinished;

    void Start()
    {
        remainingTime = matchDuration;
        isRunning = true; // Se activa al empezar, no cuando alguien coge el alfiler
    }

    void Update()
    {
        if (!isRunning || IsOvertime) return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            IsOvertime = true;
            OnTimerFinished?.Invoke();
        }
    }

    // Método por si quieres pararlo al terminar el juego
    public void StopTimer()
    {
        isRunning = false;
    }
}