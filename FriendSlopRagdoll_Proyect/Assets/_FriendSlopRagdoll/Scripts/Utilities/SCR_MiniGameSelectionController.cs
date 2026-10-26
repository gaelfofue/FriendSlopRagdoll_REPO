

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MinigameSelectionController : MonoBehaviour
{
    [Header("Selection UI")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Image confirmButtonImage;
    [SerializeField] private Color disabledColor = Color.gray;
    [SerializeField] private Color readyColor = Color.green;

    [Header("Lobby")]
    [SerializeField] private LocalLobbyManager lobbyManager;

    private MinigameOption selectedMinigame;
    private Outline selectedOutline;

    private void Start()
    {
        UpdateConfirmButton();
    }

    public void SelectMinigame(MinigameOption option)
    {
        if (option == null)
        {
            Debug.LogWarning("La opción de minijuego no es válida.");
            return;
        }

        // Si se vuelve a pulsar el mismo minijuego, deseleccionarlo.
        if (selectedMinigame == option)
        {
            ClearSelection();
            return;
        }

        // Quitar el resaltado de la selección anterior.
        ClearSelection();

        selectedMinigame = option;
        selectedOutline = option.GetComponent<Outline>();

        if (selectedOutline == null)
        {
            Debug.LogWarning(
                "Añade un componente Outline al botón " +
                option.gameObject.name
            );
        }
        else
        {
            selectedOutline.effectColor = Color.yellow;
            selectedOutline.enabled = true;
        }

        UpdateConfirmButton();

        Debug.Log("Minijuego seleccionado: " + option.MinigameName);
    }

    private void ClearSelection()
    {
        if (selectedOutline != null)
            selectedOutline.enabled = false;

        selectedOutline = null;
        selectedMinigame = null;

        UpdateConfirmButton();
    }

    private void UpdateConfirmButton()
    {
        bool canConfirm = selectedMinigame != null;

        if (confirmButton != null)
            confirmButton.interactable = canConfirm;

        if (confirmButtonImage != null)
        {
            confirmButtonImage.color =
                canConfirm ? readyColor : disabledColor;
        }
    }

    public void CancelSelection()
    {
        ClearSelection();

        if (lobbyManager == null)
        {
            Debug.LogError(
                "Asigna LocalLobbyManager en el Inspector."
            );
            return;
        }

        lobbyManager.ReturnToLobby();
    }

    public void ConfirmSelection()
    {
        if (selectedMinigame == null)
        {
            Debug.LogWarning("Selecciona un minijuego primero.");
            return;
        }

        if (lobbyManager == null)
        {
            Debug.LogError(
                "Asigna LocalLobbyManager en el Inspector."
            );
            return;
        }

        if (string.IsNullOrWhiteSpace(selectedMinigame.SceneName))
        {
            Debug.LogError(
                "La opción seleccionada no tiene una escena asignada."
            );
            return;
        }

        lobbyManager.SavePlayersForMinigame();

        if (LobbyTransferData.Players.Count == 0)
        {
            Debug.LogError(
                "No se pudieron guardar los jugadores del lobby."
            );
            return;
        }

        SceneManager.LoadScene(selectedMinigame.SceneName);
    }
}



