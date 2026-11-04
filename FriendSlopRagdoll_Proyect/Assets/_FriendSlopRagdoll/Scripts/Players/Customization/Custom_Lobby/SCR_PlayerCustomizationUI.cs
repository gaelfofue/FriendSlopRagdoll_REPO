using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;

public class PlayerCustomizationUI : MonoBehaviour
{
    [Header("Menús")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject nameMenu;

    [Header("Nombre del jugador")]
    [SerializeField] private TMP_InputField nameInputField;

    [Header("Apariencia del panel")]
    [SerializeField] private Image panelBackground;
    [SerializeField] private Button firstMainMenuButton;
    [SerializeField] private Button firstNameMenuButton;

    [Header("Teclado virtual")]
    [SerializeField] private PlayerVirtualKeyboard virtualKeyboard;

    private PlayerInput playerInput;
    private InputAction virtualKeyboardAction;

    private PlayerCustomization playerCustomization;
    private MultiplayerEventSystem multiplayerEventSystem;

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        nameMenu.SetActive(false);

        if (multiplayerEventSystem != null &&
            firstMainMenuButton != null)
        {
            multiplayerEventSystem.SetSelectedGameObject(null);

            multiplayerEventSystem.SetSelectedGameObject(
                firstMainMenuButton.gameObject
            );
        }
    }

    public void ShowNameMenu()
    {
        mainMenu.SetActive(false);
        nameMenu.SetActive(true);

        if (virtualKeyboard != null)
        {
            virtualKeyboard.gameObject.SetActive(false);
        }

        if (multiplayerEventSystem != null &&
            firstNameMenuButton != null)
        {
            multiplayerEventSystem.SetSelectedGameObject(null);

            multiplayerEventSystem.SetSelectedGameObject(
                firstNameMenuButton.gameObject
            );
        }
    }

    public void Initialize(PlayerCustomization customization,string currentName,PlayerInput input)
    {
        playerCustomization = customization;

        playerInput = input;

        if (nameInputField != null && playerInput != null)
        {
            bool usesGamepad =
                playerInput.currentControlScheme == "Gamepad";

            nameInputField.readOnly = usesGamepad;
        }

        if (playerInput != null)
        {
            virtualKeyboardAction =
                playerInput.actions["VirtualKeyboard"];
            SubscribeVirtualKeyboard();
        }
        else
        {
            Debug.LogError(
                "No se encontró el PlayerInput del jugador."
            );
        }

        if (nameInputField != null)
            nameInputField.text = currentName;

        multiplayerEventSystem = GetComponentInChildren<MultiplayerEventSystem>(true);

        if (virtualKeyboard != null)
        {
            virtualKeyboard.Initialize(multiplayerEventSystem,firstNameMenuButton);
        }
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


    private void OnVirtualKeyboardPressed(InputAction.CallbackContext context)
    {
        if (nameMenu == null || !nameMenu.activeSelf)
            return;

        if (virtualKeyboard == null)
            return;

        if (virtualKeyboard.gameObject.activeSelf)
        {
            virtualKeyboard.HideKeyboard();
        }
        else
        {
            virtualKeyboard.ShowKeyboard();
        }
    }

    private void SubscribeVirtualKeyboard()
    {
        if (virtualKeyboardAction != null)
        {
            virtualKeyboardAction.performed -= OnVirtualKeyboardPressed;
            virtualKeyboardAction.performed += OnVirtualKeyboardPressed;
        }
    }

    private void OnDisable()
    {
        if (virtualKeyboardAction != null)
        {
            virtualKeyboardAction.performed -= OnVirtualKeyboardPressed;
        }
    }

    private void OnEnable()
    {
        SubscribeVirtualKeyboard();
    }

}