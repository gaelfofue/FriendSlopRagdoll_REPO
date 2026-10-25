using UnityEngine;
using UnityEngine.InputSystem;

public class MinigamePlayerSpawner : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Spawn positions")]
    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        if (playerPrefab == null)
        {
            Debug.LogError(
                "Asigna el prefab en MinigamePlayerSpawner."
            );
            return;
        }

        if (LobbyTransferData.Players.Count == 0)
        {
            Debug.LogWarning(
                "No hay jugadores guardados desde el lobby."
            );
            return;
        }

        foreach (LobbyTransferData.PlayerData data
                 in LobbyTransferData.Players)
        {
            SpawnPlayer(data);
        }
    }

    private void SpawnPlayer(LobbyTransferData.PlayerData data)
    {
        if (data.isKeyboard)
        {
            if (Keyboard.current == null || Mouse.current == null)
            {
                Debug.LogWarning(
                    "No se encuentra el teclado o el ratón."
                );
                return;
            }
        }
        else
        {
            if (data.gamepad == null || !data.gamepad.added)
            {
                Debug.LogWarning(
                    "El mando del jugador " +
                    (data.playerIndex + 1) +
                    " no está conectado."
                );
                return;
            }
        }

        int index = data.playerIndex;

        Vector3 position = GetSpawnPosition(index);
        Quaternion rotation = GetSpawnRotation(index);

        GameObject player = Instantiate(
            playerPrefab,
            position,
            rotation
        );

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

        if (data.isKeyboard)
        {
            input.SwitchCurrentControlScheme(
                "Keyboard&Mouse",
                Keyboard.current,
                Mouse.current
            );
        }
        else
        {
            input.SwitchCurrentControlScheme(
                "Gamepad",
                data.gamepad
            );
        }

        PlayerColor color = player.GetComponent<PlayerColor>();

        if (color != null)
        {
            color.SetPlayerColor(index);
        }

        Debug.Log(
            "Jugador " + (index + 1) +
            " creado en el minijuego."
        );
    }

    private Vector3 GetSpawnPosition(int index)
    {
        if (spawnPoints != null &&
            index >= 0 &&
            index < spawnPoints.Length &&
            spawnPoints[index] != null)
        {
            return spawnPoints[index].position;
        }

        Debug.LogWarning(
            "Falta el punto de aparición del jugador " +
            (index + 1)
        );

        return new Vector3(index * 2f, 0f, 0f);
    }

    private Quaternion GetSpawnRotation(int index)
    {
        if (spawnPoints != null &&
            index >= 0 &&
            index < spawnPoints.Length &&
            spawnPoints[index] != null)
        {
            return spawnPoints[index].rotation;
        }

        return Quaternion.identity;
    }
}

