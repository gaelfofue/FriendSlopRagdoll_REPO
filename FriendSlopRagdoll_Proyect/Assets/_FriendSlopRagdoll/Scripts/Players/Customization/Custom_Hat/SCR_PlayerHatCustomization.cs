
using UnityEngine;

public class PlayerHatCustomization : MonoBehaviour
{
    [Header("Punto donde aparecen los sombreros")]
    [SerializeField] private Transform hatAnchor;

    [Header("Sombreros disponibles")]
    [SerializeField] private GameObject[] hatPrefabs;

    private GameObject currentHat;

    private int selectedHatIndex = -1;
    private int previewHatIndex = -1;

    public int SelectedHatIndex => selectedHatIndex;

    private void Start()
    {
        ApplyHat(selectedHatIndex);
    }

    public void BeginSelection()
    {
        previewHatIndex = selectedHatIndex;
        ApplyHat(previewHatIndex);
    }

    public void NextHat()
    {
        if (hatPrefabs == null || hatPrefabs.Length == 0)
            return;

        previewHatIndex++;

        if (previewHatIndex >= hatPrefabs.Length)
            previewHatIndex = -1;

        ApplyHat(previewHatIndex);
    }

    public void PreviousHat()
    {
        if (hatPrefabs == null || hatPrefabs.Length == 0)
            return;

        previewHatIndex--;

        if (previewHatIndex < -1)
            previewHatIndex = hatPrefabs.Length - 1;

        ApplyHat(previewHatIndex);
    }

    public void ConfirmSelection()
    {
        selectedHatIndex = previewHatIndex;
    }

    public void CancelSelection()
    {
        previewHatIndex = selectedHatIndex;
        ApplyHat(selectedHatIndex);
    }

    public void ApplyHat(int index)
    {
        if (currentHat != null)
            Destroy(currentHat);

        if (hatAnchor == null)
        {
            Debug.LogError("Falta asignar HatAnchor.");
            return;
        }

        if (index < 0 || hatPrefabs == null ||
            index >= hatPrefabs.Length)
            return;

        if (hatPrefabs[index] == null)
            return;

        currentHat = Instantiate(
            hatPrefabs[index],
            hatAnchor
        );

        currentHat.transform.localPosition = Vector3.zero;
        currentHat.transform.localRotation = Quaternion.identity;
        currentHat.transform.localScale = Vector3.one;
    }

    public void LoadHat(int index)
    {
        selectedHatIndex = index;
        previewHatIndex = index;
        ApplyHat(index);
    }
}
