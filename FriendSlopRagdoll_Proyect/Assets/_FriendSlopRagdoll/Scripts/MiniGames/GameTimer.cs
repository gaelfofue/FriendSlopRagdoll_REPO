using System;
using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float matchDuration = 120f;

    [Header("HUD")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color overtimeColor = Color.red;
    [SerializeField] private string overtimeLabel = "OVERTIME";

    private float remainingTime;
    private bool isRunning = false;

    public float RemainingTime => remainingTime;
    public bool IsOvertime { get; private set; } = false;

    public event Action OnTimerFinished;

    void Awake()
    {
        remainingTime = matchDuration;
    }

    void Start()
    {
        isRunning = true;
        UpdateHUD();
    }

    void Update()
    {
        if (!isRunning) return;

        if (!IsOvertime)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                IsOvertime = true;
                OnTimerFinished?.Invoke();
            }
        }

        UpdateHUD();
    }

    public void StopTimer() => isRunning = false;

    private void UpdateHUD()
    {
        if (timerText == null) return;

        if (IsOvertime)
        {
            timerText.text = overtimeLabel;
            timerText.color = overtimeColor;
        }
        else
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
            timerText.color = normalColor;
        }
    }
}