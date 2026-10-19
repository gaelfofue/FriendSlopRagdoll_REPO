using UnityEngine;

public class PinGameManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private WinnerSequence winnerSequence;
    [SerializeField] private GameObject pinWorldObject;

    private PinHolder currentHolder;
    private bool gameEnded = false;

    // Callback que el GameManager global asignará para saber quién ganó
    public System.Action<GameObject> OnWinnerDecided;

    void Start()
    {
        if (gameTimer == null)
        {
            Debug.LogError("[PinGameManager] Falta asignar gameTimer.");
            return;
        }

        gameTimer.OnTimerFinished += OnTimerFinished;
    }

    public void OnPinPickedUp(PinHolder holder)
    {
        if (gameEnded) return;

        if (pinWorldObject != null)
            Destroy(pinWorldObject);

        currentHolder = holder;
        holder.GivePin();

        if (gameTimer.IsOvertime)
            EndGame(holder);
    }

    public void StealPin(PinHolder from, PinHolder to)
    {
        if (gameEnded) return;
        if (from == null || to == null) return;

        from.RemovePin();
        to.GivePin();
        currentHolder = to;
    }

    private void OnTimerFinished()
    {
        if (currentHolder != null)
            EndGame(currentHolder);
    }

    private void EndGame(PinHolder winner)
    {
        if (gameEnded) return;
        gameEnded = true;

        if (gameTimer != null) gameTimer.StopTimer();

        if (winnerSequence != null)
            winnerSequence.PlaySequence(winner.gameObject, winner.gameObject);

        // Avisamos al GameManager global de que hay ganador.
        // El global es el que decide cuándo cambiar de escena.
        OnWinnerDecided?.Invoke(winner.gameObject);
    }
}