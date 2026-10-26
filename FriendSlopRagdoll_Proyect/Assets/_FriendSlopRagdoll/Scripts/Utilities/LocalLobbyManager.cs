using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

public class LocalLobbyManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Spawn positions")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("UI")]
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text joinPromptText;
    [SerializeField] private GameObject playButton;

    [Header("Ready UI")]
    [SerializeField] private TMP_Text readyStatusText;

    [Header("Minigame Selection UI")]
    [SerializeField] private GameObject minigameSelectCanvas;

    private class LobbyPlayer
    {
        public bool isKeyboard;
        public Gamepad gamepad;
        public bool isReady;
    }

    private const int MaxPlayers = 4;

    private readonly System.Collections.Generic.List<LobbyPlayer>
        lobbyPlayers =
            new System.Collections.Generic.List<LobbyPlayer>();

    private int playerCount = 0;
    private bool keyboardJoined = false;

    private readonly System.Collections.Generic.HashSet<Gamepad>
        joinedGamepads =
        new System.Collections.Generic.HashSet<Gamepad>();

    private void Start()
    {
        if (playerPrefab == null)
        {
            Debug.LogError(
                "Asigna el prefab del jugador en LocalLobbyManager."
            );
            enabled = false;
            return;
        }

        UpdateUI();
    }

    private void Update()
    {
        CheckReadyInputs();

        if (playerCount >= MaxPlayers)
            return;

        CheckKeyboardJoin();
        CheckGamepadJoins();
    }

    private void CheckKeyboardJoin()
    {
        if (keyboardJoined || Keyboard.current == null)
            return;

        bool joinPressed =
            Keyboard.current.spaceKey.wasPressedThisFrame ||
            Keyboard.current.enterKey.wasPressedThisFrame;

        if (!joinPressed)
            return;

        if (Mouse.current == null)
        {
            Debug.LogWarning("No se ha detectado el ratón.");
            return;
        }

        GameObject player = CreatePlayer(playerCount);

        if (player == null)
            return;

        PlayerInput input = player.GetComponent<PlayerInput>();

        if (input == null)
        {
            Debug.LogError(
                "El prefab necesita un componente Player Input."
            );
            Destroy(player);
            return;
        }

        input.neverAutoSwitchControlSchemes = true;

        input.SwitchCurrentControlScheme(
            "Keyboard&Mouse",
            Keyboard.current,
            Mouse.current
        );

        keyboardJoined = true;

        lobbyPlayers.Add(new LobbyPlayer
        {
            isKeyboard = true,
            gamepad = null,
            isReady = false
        });

        RegisterPlayer();
        UpdateReadyUI();

        Debug.Log("Jugador de teclado creado.");
    }

    private void CheckGamepadJoins()
    {
        foreach (Gamepad gamepad in Gamepad.all)
        {
            if (playerCount >= MaxPlayers)
                break;

            if (joinedGamepads.Contains(gamepad))
                continue;

            if (!gamepad.buttonSouth.wasPressedThisFrame)
                continue;

            GameObject player = CreatePlayer(playerCount);

            if (player == null)
                continue;

            PlayerInput input = player.GetComponent<PlayerInput>();

            if (input == null)
            {
                Debug.LogError(
                    "El prefab necesita un componente Player Input."
                );
                Destroy(player);
                continue;
            }

            input.neverAutoSwitchControlSchemes = true;

            input.SwitchCurrentControlScheme(
                "Gamepad",
                gamepad
            );

            joinedGamepads.Add(gamepad);

            lobbyPlayers.Add(new LobbyPlayer
            {
                isKeyboard = false,
                gamepad = gamepad,
                isReady = false
            });

            RegisterPlayer();
            UpdateReadyUI();

            Debug.Log(
                "Jugador de mando creado: " + gamepad.displayName
            );
        }
    }
    private GameObject CreatePlayer(int index)
    {
        Vector3 position = GetSpawnPosition(index);
        Quaternion rotation = Quaternion.identity;

        if (spawnPoints != null &&
            index < spawnPoints.Length &&
            spawnPoints[index] != null)
        {
            rotation = spawnPoints[index].rotation;
        }

        GameObject player = Instantiate(
            playerPrefab,
            position,
            rotation
        );

        PlayerColor playerColor = player.GetComponent<PlayerColor>();

        if (playerColor != null)
        {
            playerColor.SetPlayerColor(index);
        }
        else
        {
            Debug.LogWarning(
                "El prefab del jugador no tiene el componente PlayerColor."
            );
        }

        return player;
    }

    private Vector3 GetSpawnPosition(int index)
    {
        if (spawnPoints != null &&
            index < spawnPoints.Length &&
            spawnPoints[index] != null)
        {
            return spawnPoints[index].position;
        }

        return new Vector3(index * 2f, 0f, 0f);
    }
    private void RegisterPlayer()
    {
        playerCount++;
        UpdateUI();

        Debug.Log("Jugadores en el lobby: " + playerCount + "/4");
    }

    private void UpdateUI()
    {
        if (playerCountText != null)
            playerCountText.text = playerCount + "/4";

        if (joinPromptText != null)
        {
            joinPromptText.text = playerCount >= MaxPlayers
                ? "Lobby completo"
                : "Pulsa ESPACIO o INTRO, o el botón A " +
                  "del mando, para unirte";
        }
    }

    private void CheckReadyInputs()
    {
        for (int i = 0; i < lobbyPlayers.Count; i++)
        {
            LobbyPlayer player = lobbyPlayers[i];

            bool pressed = false;

            if (player.isKeyboard && Keyboard.current != null)
            {
                pressed = Keyboard.current.enterKey.wasPressedThisFrame;
            }
            else if (player.gamepad != null &&
                     player.gamepad.added)
            {
                pressed = player.gamepad.startButton.wasPressedThisFrame;
            }

            if (pressed)
            {
                player.isReady = !player.isReady;
                UpdateReadyUI();
            }
        }
    }

    private bool AllPlayersReady()
    {
        if (lobbyPlayers.Count == 0)
            return false;

        foreach (LobbyPlayer player in lobbyPlayers)
        {
            if (!player.isReady)
                return false;
        }

        return true;
    }
    private void UpdateReadyUI()
    {
        if (readyStatusText == null)
            return;

        int readyCount = 0;

        foreach (LobbyPlayer player in lobbyPlayers)
        {
            if (player.isReady)
                readyCount++;
        }

        readyStatusText.text =
            $"Listos {readyCount}/{lobbyPlayers.Count}";

        if (playButton != null)
        {
            bool showButton =
                lobbyPlayers.Count > 0 && AllPlayersReady();

            playButton.SetActive(showButton);
        }
    }

    public void SavePlayersForMinigame()
    {
        LobbyTransferData.Clear();

        for (int i = 0; i < lobbyPlayers.Count; i++)
        {
            LobbyPlayer player = lobbyPlayers[i];

            LobbyTransferData.Players.Add(
                new LobbyTransferData.PlayerData
                {
                    playerIndex = i,
                    isKeyboard = player.isKeyboard,
                    gamepad = player.gamepad,
                    isReady = player.isReady
                }
            );
        }

        Debug.Log(
            "Jugadores guardados para el minijuego: " +
            LobbyTransferData.Players.Count
        );
    }


    public void ConfirmMinigame()
    {
        if (!AllPlayersReady())
        {
            Debug.LogWarning(
                "Todos los jugadores deben estar listos."
            );
            return;
        }

        SavePlayersForMinigame();

        Debug.Log("Cargando SCN_ParchisTest...");

        SceneManager.LoadScene("SCN_ParchisTest");
    }

    public void OpenMinigameSelection()
    {
        if (!AllPlayersReady())
        {
            Debug.LogWarning("Todos los jugadores deben estar listos.");
            return;
        }

        if (minigameSelectCanvas == null)
        {
            Debug.LogError(
                "Asigna Canvas_MiniGameSelect en LocalLobbyManager."
            );
            return;
        }

        minigameSelectCanvas.SetActive(true);

        Debug.Log("Pantalla de selección de minijuegos abierta.");
    }


    public void ReturnToLobby()
    {
        if (minigameSelectCanvas != null)
            minigameSelectCanvas.SetActive(false);

        foreach (LobbyPlayer player in lobbyPlayers)
        {
            player.isReady = false;
        }

        UpdateReadyUI();

        Debug.Log(
            "Vuelta a la lobby: todos los jugadores están no listos."
        );
    }

}







