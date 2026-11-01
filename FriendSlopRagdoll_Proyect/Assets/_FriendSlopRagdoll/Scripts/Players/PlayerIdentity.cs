
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerIdentity : MonoBehaviour
{
    [SerializeField] private int playerIndex = -1;

    public int PlayerIndex => playerIndex;

    public PlayerInputRelay InputRelay { get; private set; }

    private void Start()
    {
        InputRelay = GetComponent<PlayerInputRelay>();

        PlayerInput playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("[PlayerIdentity] Falta PlayerInput.");
            return;
        }

        foreach (LobbyTransferData.PlayerData data
                 in LobbyTransferData.Players)
        {
            if (data.isKeyboard)
            {
                if (playerInput.currentControlScheme == "Keyboard&Mouse")
                {
                    playerIndex = data.playerIndex;
                    break;
                }
            }
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
            Debug.LogError("[PlayerIdentity] Falta PlayerInputRelay.");
            return;
        }

        if (playerIndex < 0)
        {
            Debug.LogError(
                $"[PlayerIdentity] No se pudo identificar a {name}."
            );
        }
        else
        {
            Debug.Log(
                $"[PlayerIdentity] {name} identificado como jugador {playerIndex + 1}."
            );
        }
    }
}
