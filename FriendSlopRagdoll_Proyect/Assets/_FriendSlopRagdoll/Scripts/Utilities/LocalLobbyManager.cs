using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class LocalLobbyManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Spawn positions")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("UI")]
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text joinPromptText;

    private const int MaxPlayers = 4;

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
        RegisterPlayer();

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
            RegisterPlayer();

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

        return Instantiate(
            playerPrefab,
            position,
            rotation
        );
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
}







