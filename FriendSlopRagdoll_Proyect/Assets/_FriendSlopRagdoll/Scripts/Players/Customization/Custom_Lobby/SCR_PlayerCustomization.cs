using UnityEngine;

public class PlayerCustomization : MonoBehaviour
{
    [Header("Customization")]
    [SerializeField] private Transform customizationAnchor;

    private GameObject customizationPanel;

    public void OpenCustomization()
    {
        if (customizationPanel != null)
            customizationPanel.SetActive(true);
    }

    public void CloseCustomization()
    {
        if (customizationPanel != null)
            customizationPanel.SetActive(false);
    }
}