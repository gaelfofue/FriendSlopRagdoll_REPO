
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
            Debug.LogError("[PlayerIdentity] No hay PlayerInput en " + name);
            return;
        }

        bool hasKeyboard = false;

        foreach (InputDevice device in playerInput.devices)
        {
            if (device is Keyboard)
            {
                hasKeyboard = true;
                break;
            }
        }

        foreach (LobbyTransferData.PlayerData data in LobbyTransferData.Players)
        {
            if (data.isKeyboard)
            {
                if (hasKeyboard)
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
            Debug.LogError("[PlayerIdentity] Falta PlayerInputRelay en " + name);
        }

        if (playerIndex < 0)
        {
            string devices = "";

            foreach (InputDevice device in playerInput.devices)
                devices += device.displayName + " | ";

            Debug.LogError(
                "[PlayerIdentity] No se pudo identificar a " + name +
                "\nEsquema: " + playerInput.currentControlScheme +
                "\nDispositivos asignados: " + devices +
                "\nJugadores guardados en el lobby: " + LobbyTransferData.Players.Count
            );
        }
        else
        {
            Debug.Log(
                "[PlayerIdentity] " + name +
                " identificado como jugador " + (playerIndex + 1)
            );
        }
    }
}
