using System.Collections.Generic;
using UnityEngine.InputSystem;

public static class LobbyTransferData
{
    [System.Serializable]
    public class PlayerData
    {
        public int playerIndex;
        public bool isKeyboard;
        public Gamepad gamepad;
        public bool isReady;

        public string characterId = "";
        public string skinId = "";
        public string accessoriesData = "";
    }

    public static readonly List<PlayerData> Players =
        new List<PlayerData>();

    public static void Clear()
    {
        Players.Clear();
    }
}

