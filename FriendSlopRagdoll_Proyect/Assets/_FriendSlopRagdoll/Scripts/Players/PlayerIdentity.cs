
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerIdentity : MonoBehaviour
{
    [SerializeField] private int playerIndex = -1;

    public int PlayerIndex => playerIndex;

    public void SetPlayerIndex(int index)
    {
        playerIndex = index;
    }
    public PlayerInputRelay InputRelay { get; private set; }

    private void Start()
    {
        InputRelay = GetComponent<PlayerInputRelay>();

        PlayerInput playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("[PlayerIdentity] No se encontró PlayerInput en " + gameObject.name);
            return;
        }

        // Los personajes del lobby se crean antes de guardar
        // los datos para el minijuego. No necesitan identificarse aquí.
        if (LobbyTransferData.Players.Count == 0)
        {
            return;
        }

        IdentifyPlayer(playerInput);

        if (InputRelay == null)
        {
            Debug.LogError("[PlayerIdentity] Falta PlayerInputRelay en " + gameObject.name);
        }

        if (playerIndex < 0)
        {
            Debug.LogError(
                "[PlayerIdentity] No se pudo identificar a " + gameObject.name
            );
        }
        else
        {
            Debug.Log(
                "[PlayerIdentity] " + gameObject.name +
                " identificado como jugador " + (playerIndex + 1)
            );
        }
    }

    private void IdentifyPlayer(PlayerInput playerInput)
    {
        foreach (LobbyTransferData.PlayerData data in LobbyTransferData.Players)
        {
            if (data.isKeyboard)
            {
                foreach (InputDevice device in playerInput.devices)
                {
                    if (device is Keyboard)
                    {
                        playerIndex = data.playerIndex;
                        return;
                    }
                }
            }
            else if (data.gamepad != null)
            {
                foreach (InputDevice device in playerInput.devices)
                {
                    if (device == data.gamepad)
                    {
                        playerIndex = data.playerIndex;
                        return;
                    }
                }
            }
        }
    }
}
