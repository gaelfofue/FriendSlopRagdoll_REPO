using UnityEngine;

public class PinHolder : MonoBehaviour
{
    [SerializeField] private Transform pinAnchor;
    [SerializeField] private GameObject pinVisualPrefab;
    [SerializeField] private int playerId = 0;
    public int PlayerId => playerId;

    private GameObject currentPinVisual;

    public bool HasPin => currentPinVisual != null;

    public void GivePin()
    {
        if (HasPin) return;

        if (pinVisualPrefab == null)
        {
            Debug.LogError($"[PinHolder] {name} no tiene pinVisualPrefab asignado.");
            return;
        }
        if (pinAnchor == null)
        {
            Debug.LogError($"[PinHolder] {name} no tiene pinAnchor asignado.");
            return;
        }

        currentPinVisual = Instantiate(pinVisualPrefab, pinAnchor);
        currentPinVisual.transform.localPosition = Vector3.zero;
        currentPinVisual.transform.localRotation = Quaternion.identity;
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