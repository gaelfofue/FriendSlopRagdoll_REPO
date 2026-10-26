
using UnityEngine;

public class PlayerColor : MonoBehaviour
{
    [Header("Materiales por jugador")]
    [SerializeField] private Material materialAzul;
    [SerializeField] private Material materialRojo;
    [SerializeField] private Material materialVerde;
    [SerializeField] private Material materialRosa;

    [Header("Renderer del modelo")]
    [SerializeField] private Renderer modeloRenderer;

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



