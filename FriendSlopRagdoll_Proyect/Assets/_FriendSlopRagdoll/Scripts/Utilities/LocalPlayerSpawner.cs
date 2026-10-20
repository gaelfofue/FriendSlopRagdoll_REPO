
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class LocalPlayerSpawner : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Spawn positions")]
    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Asigna el prefab del jugador en LocalPlayerSpawner.");
            return;
        }

        SpawnKeyboardPlayer();
        SpawnGamepadPlayers();
    }

    private void SpawnKeyboardPlayer()
    {
        if (Keyboard.current == null)
        {
            Debug.LogWarning("No se ha detectado un teclado.");
            return;
        }

        GameObject player = Instantiate(
            playerPrefab,
            GetSpawnPosition(0),
            Quaternion.identity
        );

        PlayerInput input = player.GetComponent<PlayerInput>();

        if (input == null)
        {
            Debug.LogError("El prefab necesita un componente Player Input.");
            Destroy(player);
            return;
        }

        input.SwitchCurrentControlScheme(
            "Keyboard&Mouse",
            Keyboard.current,
            Mouse.current
        );

        Debug.Log("Jugador de teclado creado.");
    }

    private void SpawnGamepadPlayers()
    {
        Gamepad[] gamepads = Gamepad.all.ToArray();

        for (int i = 0; i < gamepads.Length && i < 3; i++)
        {
            GameObject player = Instantiate(
                playerPrefab,
                GetSpawnPosition(i + 1),
                Quaternion.identity
            );

            PlayerInput input = player.GetComponent<PlayerInput>();

            if (input == null)
            {
                Debug.LogError("El prefab necesita un componente Player Input.");
                Destroy(player);
                continue;
            }

            input.SwitchCurrentControlScheme(
                "Gamepad",
                gamepads[i]
            );

            Debug.Log($"Jugador de mando creado: {i + 2}");
        }
    }

    private Vector3 GetSpawnPosition(int index)
    {
        if (spawnPoints != null && index < spawnPoints.Length
            && spawnPoints[index] != null)
        {
            return spawnPoints[index].position;
        }

        return new Vector3(index * 2f, 0f, 0f);
    }
}

