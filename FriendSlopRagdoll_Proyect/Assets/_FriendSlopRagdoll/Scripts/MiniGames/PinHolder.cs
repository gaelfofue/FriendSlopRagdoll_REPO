using UnityEngine;

public class PinHolder : MonoBehaviour
{
    [Tooltip("Punto donde aparecerá el alfiler encima del jugador")]
    [SerializeField] private Transform pinAnchor;

    [Tooltip("Prefab visual del alfiler (el modelo que aparece encima del personaje)")]
    [SerializeField] private GameObject pinVisualPrefab;

    private GameObject currentPinVisual;

    public bool HasPin => currentPinVisual != null;

    public void GivePin()
    {
        if (HasPin) return;

        if (pinVisualPrefab != null && pinAnchor != null)
        {
            currentPinVisual = Instantiate(pinVisualPrefab, pinAnchor);
            currentPinVisual.transform.localPosition = Vector3.zero;
            currentPinVisual.transform.localRotation = Quaternion.identity;
        }
    }

    public void RemovePin()
    {
        if (currentPinVisual != null)
        {
            Destroy(currentPinVisual);
            currentPinVisual = null;
        }
    }
}