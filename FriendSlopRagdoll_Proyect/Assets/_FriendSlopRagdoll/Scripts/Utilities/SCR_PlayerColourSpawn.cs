using UnityEngine;
public class PlayerColor : MonoBehaviour
{
    [Header("Materiales por jugador")]
    [SerializeField] private Material materialAzul;
    [SerializeField] private Material materialRojo;
    [SerializeField] private Material materialVerde;
    [SerializeField] private Material materialRosa;

    [Header("Colores de las etiquetas")]
    [SerializeField] private Color colorAzul = new Color(0.2f, 0.5f, 1f);
    [SerializeField] private Color colorRojo = new Color(1f, 0.2f, 0.2f);
    [SerializeField] private Color colorVerde = new Color(0.2f, 0.8f, 0.3f);
    [SerializeField] private Color colorRosa = new Color(1f, 0.35f, 0.65f);

    [Header("Renderer del modelo")]
    [SerializeField] private Renderer modeloRenderer;

    public Color GetPlayerColor(int playerIndex)
    {
        switch (playerIndex)
        {
            case 0: return colorAzul;
            case 1: return colorRojo;
            case 2: return colorVerde;
            case 3: return colorRosa;

            default:
                Debug.LogError("Índice de jugador no válido: " + playerIndex);
                return Color.white;
        }
    }

    public void SetPlayerColor(int playerIndex)
    {
        Material materialElegido = null;

        switch (playerIndex)
        {
            case 0:
                materialElegido = materialAzul;
                break;

            case 1:
                materialElegido = materialRojo;
                break;

            case 2:
                materialElegido = materialVerde;
                break;

            case 3:
                materialElegido = materialRosa;
                break;
        }

        if (materialElegido == null)
        {
            Debug.LogError(
                "No se ha asignado el material del jugador " +
                (playerIndex + 1)
            );
            return;
        }

        if (modeloRenderer == null)
        {
            Debug.LogError(
                "Asigna el Renderer del modelo en PlayerColor."
            );
            return;
        }

        Material[] materiales = modeloRenderer.sharedMaterials;

        for (int i = 0; i < materiales.Length; i++)
        {
            materiales[i] = materialElegido;
        }

        modeloRenderer.sharedMaterials = materiales;
    }
}