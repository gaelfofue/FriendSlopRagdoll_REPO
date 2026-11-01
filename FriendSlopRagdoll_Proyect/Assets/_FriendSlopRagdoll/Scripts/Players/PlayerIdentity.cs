
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerIdentity : MonoBehaviour
{
    [SerializeField] private int playerIndex = -1;

    public int PlayerIndex => playerIndex;

    public PlayerInputRelay InputRelay { get; private set; }

    private void Awake()
    {
        InputRelay = GetComponent<PlayerInputRelay>();

        PlayerInput playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError(
                "PlayerIdentity: falta PlayerInput."
            );
            return;
        }

        foreach (LobbyTransferData.PlayerData data
                 in LobbyTransferData.Players)
        {
            // Identificar al jugador de teclado.
            if (data.isKeyboard)
            {
                if (playerInput.currentControlScheme ==
                    "Keyboard&Mouse")
                {
                    playerIndex = data.playerIndex;
                    break;
                }
            }
            // Identificar al jugador de mando.
            else if (data.gamepad != null)
            {
                foreach (InputDevice device in playerInput.devices)
                {
                    if (device == data.gamepad)
                    {
                        playerIndex = data.playerIndex;
                        break;
                    }
                }

                if (playerIndex >= 0)
                    break;
            }
        }

        if (InputRelay == null)
        {
            Debug.LogError(
                "PlayerIdentity: falta PlayerInputRelay."
            );
        }

        if (playerIndex < 0)
        {
            Debug.LogError(
                "PlayerIdentity: no se pudo identificar al jugador."
            );
        }
        else
        {
            Debug.Log(
                $"PlayerIdentity: soy el jugador {playerIndex + 1}."
            );
        }
    }
}
