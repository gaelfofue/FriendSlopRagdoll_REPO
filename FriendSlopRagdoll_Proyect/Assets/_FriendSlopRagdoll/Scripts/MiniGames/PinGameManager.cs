using UnityEngine;

public class PinGameManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private WinnerSequence winnerSequence; // El que ya hicimos
    [SerializeField] private GameObject pinWorldObject;     // El alfiler físico del suelo

    [Header("Escena")]
    [SerializeField] private string nextSceneName = "Menu";

    private PinHolder currentHolder;
    private bool gameEnded = false;

    void Start()
    {
        // Suscribirse al evento de fin de timer
        if (gameTimer != null)
            gameTimer.OnTimerFinished += OnTimerFinished;
    }

    // Llamado por PinPickup cuando alguien coge el alfiler del suelo
    public void OnPinPickedUp(PinHolder holder)
    {
        if (gameEnded) return;

        // Destruimos el alfiler del suelo (ya no se necesita)
        if (pinWorldObject != null)
            Destroy(pinWorldObject);

        currentHolder = holder;
        holder.GivePin();

        // Caso especial: si ya estamos en overtime, coger el alfiler = ganar
        if (gameTimer.IsOvertime)
        {
            EndGame(holder);
        }
    }

    // Llamado por PinSteal cuando dos jugadores chocan y uno tiene el alfiler
    public void StealPin(PinHolder from, PinHolder to)
    {
        if (gameEnded) return;
        if (from == null || to == null) return;

        from.RemovePin();
        to.GivePin();
        currentHolder = to;

        // Si estamos en overtime y alguien roba el alfiler, no gana todavía:
        // tiene que aguantarlo hasta... bueno, en overtime la regla era "quien lo coja gana".
        // Si quieres que en overtime ganar sea instantáneo al tocarlo, descomenta:
        // if (gameTimer.IsOvertime) EndGame(to);
    }

    // Llamado por GameTimer cuando el tiempo llega a 0
    private void OnTimerFinished()
    {
        // Si alguien tiene el alfiler cuando acaba el tiempo -> gana
        if (currentHolder != null)
        {
            EndGame(currentHolder);
        }
        // Si nadie lo tiene -> entramos en overtime (el timer ya está en IsOvertime = true)
        // No hacemos nada más: el juego sigue hasta que alguien coja el alfiler.
    }

    private void EndGame(PinHolder winner)
    {
        if (gameEnded) return;
        gameEnded = true;

        if (gameTimer != null) gameTimer.StopTimer();

        // Reutilizamos la secuencia de victoria existente
        if (winnerSequence != null)
        {
            // Necesitamos pasarle el pickup para el LookAt.
            // Como el alfiler ahora está encima del jugador, usamos el propio jugador como "pickup".
            winnerSequence.StartWinnerSequence(winner.gameObject, winner.gameObject);
        }
        else
        {
            Debug.LogWarning("Falta asignar WinnerSequence en el PinGameManager");
        }
    }
}