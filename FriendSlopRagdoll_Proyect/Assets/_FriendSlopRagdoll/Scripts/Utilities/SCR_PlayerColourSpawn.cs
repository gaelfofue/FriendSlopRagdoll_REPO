using UnityEngine;

public class PlayerColor : MonoBehaviour
{
    private static readonly Color[] PlayerColors =
    {
        new Color(0.19f, 0.45f, 1f),  // Jugador 1: azul
        new Color(0.95f, 0.12f, 0.16f), // Jugador 2: rojo
        new Color(0.12f, 0.8f, 0.3f),   // Jugador 3: verde
        new Color(1f, 0.4f, 0.72f)      // Jugador 4: rosa
    };

    private Renderer[] renderers;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    public void SetPlayerColor(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= PlayerColors.Length)
        {
            Debug.LogWarning("Índice de jugador no válido: " + playerIndex);
            return;
        }

        Color color = PlayerColors[playerIndex];

        foreach (Renderer rend in renderers)
        {
            MaterialPropertyBlock properties =
                new MaterialPropertyBlock();

            rend.GetPropertyBlock(properties);

            properties.SetColor("_Color", color);
            properties.SetColor("_BaseColor", color);

            rend.SetPropertyBlock(properties);
        }
    }
}

