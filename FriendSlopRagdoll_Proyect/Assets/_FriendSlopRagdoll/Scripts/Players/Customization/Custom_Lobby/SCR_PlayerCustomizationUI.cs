using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerCustomizationUI : MonoBehaviour
{
    [Header("Menús")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject nameMenu;

    [Header("Nombre del jugador")]
    [SerializeField] private TMP_InputField nameInputField;

    [Header("Apariencia del panel")]
    [SerializeField] private Image panelBackground;

    private PlayerCustomization playerCustomization;

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        nameMenu.SetActive(false);
    }

    public void ShowNameMenu()
    {
        mainMenu.SetActive(false);
        nameMenu.SetActive(true);
    }

    public void Initialize(
    PlayerCustomization customization,
    string currentName)
    {
        playerCustomization = customization;

        if (nameInputField != null)
            nameInputField.text = currentName;
    }

    public void ConfirmName()
    {
        if (playerCustomization == null)
        {
            Debug.LogError(
                "No se ha conectado PlayerCustomization."
            );
            return;
        }

        if (nameInputField == null)
        {
            Debug.LogError(
                "No se ha asignado NameInputField."
            );
            return;
        }

        playerCustomization.SavePlayerName(nameInputField.text);
    }

    public void SetPlayerColor(Color playerColor)
    {
        if (panelBackground != null)
        {
            panelBackground.color = playerColor;
        }
    }
}