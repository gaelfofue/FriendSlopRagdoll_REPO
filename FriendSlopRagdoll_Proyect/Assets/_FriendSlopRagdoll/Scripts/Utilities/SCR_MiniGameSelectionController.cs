
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MinigameSelectionController : MonoBehaviour
{
    [Header("Selection UI")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Image confirmButtonImage;
    [SerializeField] private Color disabledColor = Color.gray;
    [SerializeField] private Color readyColor = Color.green;

    [Header("Selection Appearance")]
    [SerializeField]
    private Color selectedColor =
        new Color(1f, 0.8f, 0.2f, 1f);

    [Header("Lobby")]
    [SerializeField] private LocalLobbyManager lobbyManager;

    private MinigameOption selectedMinigame;
    private Image selectedOptionImage;

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

        selectedMinigame = option;

        selectedOptionImage = option.GetComponent<Image>();

        if (selectedOptionImage == null)
            selectedOptionImage = option.GetComponentInChildren<Image>();

        UpdateConfirmButton();

        Debug.Log("Minijuego seleccionado: " + option.MinigameName);
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

        Debug.Log("Jugadores guardados: " +
                  LobbyTransferData.Players.Count);

        SceneManager.LoadScene(selectedMinigame.SceneName);
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
}


