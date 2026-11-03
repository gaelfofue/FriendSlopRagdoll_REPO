using UnityEngine;

public class PlayerCustomization : MonoBehaviour
{
    [Header("Customization")]
    [SerializeField] private GameObject customizationPanelPrefab;
    [SerializeField] private Transform customizationAnchor;

    private GameObject customizationPanel;
    private Transform customizationPanelTransform;

    [Header("Referencias del jugador")]
    [SerializeField] private PlayerIdentity playerIdentity;
    [SerializeField] private PlayerNameplateSpawner nameplateSpawner;

    private PlayerColor playerColor;

    private LocalLobbyManager lobbyManager;
    private string currentPlayerName;

    private void Start()
    {
        if (playerIdentity == null)
            playerIdentity = GetComponent<PlayerIdentity>();

        if (nameplateSpawner == null)
            nameplateSpawner = GetComponent<PlayerNameplateSpawner>();

        if (playerColor == null)
            playerColor = GetComponent<PlayerColor>();

        lobbyManager = FindFirstObjectByType<LocalLobbyManager>();

        if (playerIdentity != null)
        {
            currentPlayerName = "Jugador " +
                (playerIdentity.PlayerIndex + 1);
        }

        if (customizationPanelPrefab == null)
        {
            Debug.LogError(
                "Falta asignar el prefab PlayerCustomizationPanel."
            );
            return;
        }

        if (customizationAnchor == null)
        {
            Debug.LogError(
                "Falta asignar CustomizationAnchor."
            );
            return;
        }

        customizationPanel = Instantiate(customizationPanelPrefab);

        customizationPanelTransform = customizationPanel.transform;

        PlayerCustomizationUI customizationUI =
    customizationPanel.GetComponent<PlayerCustomizationUI>();

        if (customizationUI != null)
        {
            customizationUI.Initialize(this, currentPlayerName);

            if (playerColor != null)
            {
                Color color = playerColor.GetPlayerColor(
                    playerIdentity.PlayerIndex
                );

                customizationUI.SetPlayerColor(color);
            }
            else
            {
                Debug.LogWarning(
                    "No se ha encontrado PlayerColor en el jugador."
                );
            }
        }
        else
        {
            Debug.LogError(
                "El prefab necesita el componente PlayerCustomizationUI."
            );
        }

        customizationPanelTransform.position =
            customizationAnchor.position;

        customizationPanelTransform.rotation = Quaternion.identity;

        customizationPanel.SetActive(false);
    }

    private void LateUpdate()
    {
        if (customizationPanelTransform == null ||
            customizationAnchor == null)
            return;

        customizationPanelTransform.position =
            customizationAnchor.position;
    }

    public void OpenCustomization()
    {
        if (customizationPanel != null)
            customizationPanel.SetActive(true);
    }

    public void CloseCustomization()
    {
        if (customizationPanel != null)
            customizationPanel.SetActive(false);
    }

    public void ToggleCustomization()
    {
        if (customizationPanel == null)
            return;

        bool isOpen = customizationPanel.activeSelf;

        if (isOpen)
        {
            customizationPanel.SetActive(false);
        }
        else
        {
            PlayerCustomizationUI customizationUI =
                customizationPanel.GetComponent<PlayerCustomizationUI>();

            if (customizationUI != null)
            {
                customizationUI.ShowMainMenu();
            }

            customizationPanel.SetActive(true);
        }
    }

    public void SavePlayerName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            Debug.LogWarning("El nombre no puede estar vacío.");
            return;
        }

        if (playerIdentity == null || nameplateSpawner == null)
        {
            Debug.LogError(
                "Faltan referencias del jugador para guardar el nombre."
            );
            return;
        }

        if (lobbyManager == null)
        {
            Debug.LogError("No se ha encontrado LocalLobbyManager.");
            return;
        }

        newName = newName.Trim();

        lobbyManager.UpdatePlayerName(
            playerIdentity.PlayerIndex,
            newName
        );

        nameplateSpawner.UpdatePlayerName(newName);

        currentPlayerName = newName;

        Debug.Log("Nombre personalizado: " + currentPlayerName);
    }
}