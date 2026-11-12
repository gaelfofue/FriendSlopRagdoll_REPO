
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;

public class PlayerCustomizationUI : MonoBehaviour
{
    [Header("Menús")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject nameMenu;
    [SerializeField] private GameObject headMenu;

    [Header("Nombre del jugador")]
    [SerializeField] private TMP_InputField nameInputField;

    [Header("Apariencia del panel")]
    [SerializeField] private Image panelBackground;
    [SerializeField] private Button firstMainMenuButton;
    [SerializeField] private Button firstNameMenuButton;
    [SerializeField] private Button firstHeadMenuButton;

    [Header("Teclado virtual")]
    [SerializeField] private PlayerVirtualKeyboard virtualKeyboard;

    private PlayerInput playerInput;
    private InputAction virtualKeyboardAction;

    private PlayerCustomization playerCustomization;
    private PlayerHatCustomization hatCustomization;
    private MultiplayerEventSystem multiplayerEventSystem;

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        nameMenu.SetActive(false);
        headMenu.SetActive(false);

        if (virtualKeyboard != null)
            virtualKeyboard.gameObject.SetActive(false);

        SelectButton(firstMainMenuButton);
    }

    public void ShowNameMenu()
    {
        mainMenu.SetActive(false);
        nameMenu.SetActive(true);
        headMenu.SetActive(false);

        if (virtualKeyboard != null)
            virtualKeyboard.gameObject.SetActive(false);

        SelectButton(firstNameMenuButton);
    }

    public void ShowHeadMenu()
    {
        if (hatCustomization == null)
        {
            Debug.LogError("No se encontró PlayerHatCustomization.");
            return;
        }

        mainMenu.SetActive(false);
        nameMenu.SetActive(false);
        headMenu.SetActive(true);

        hatCustomization.BeginSelection();

        SelectButton(firstHeadMenuButton);
    }

    public void NextHat()
    {
        if (hatCustomization != null)
            hatCustomization.NextHat();
    }

    public void PreviousHat()
    {
        if (hatCustomization != null)
            hatCustomization.PreviousHat();
    }

    public void ConfirmHat()
    {
        if (hatCustomization == null)
            return;

        hatCustomization.ConfirmSelection();
        ShowMainMenu();
    }

    public void CancelHat()
    {
        if (hatCustomization != null)
            hatCustomization.CancelSelection();

        ShowMainMenu();
    }

    private void SelectButton(Button button)
    {
        if (multiplayerEventSystem == null || button == null)
            return;

        multiplayerEventSystem.SetSelectedGameObject(null);
        multiplayerEventSystem.SetSelectedGameObject(button.gameObject);
    }

    public void Initialize(
        PlayerCustomization customization,
        string currentName,
        PlayerInput input)
    {
        playerCustomization = customization;
        playerInput = input;

        if (playerCustomization != null)
        {
            hatCustomization =
                playerCustomization.GetComponent<PlayerHatCustomization>();
        }

        if (hatCustomization == null)
        {
            Debug.LogError(
                "El jugador no tiene PlayerHatCustomization."
            );
        }

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

        multiplayerEventSystem =
            GetComponentInChildren<MultiplayerEventSystem>(true);

        if (virtualKeyboard != null)
        {
            virtualKeyboard.Initialize(
                multiplayerEventSystem,
                firstNameMenuButton
            );
        }
    }

    public void ConfirmName()
    {
        if (playerCustomization == null)
            return;

        if (nameInputField == null)
            return;

        playerCustomization.SavePlayerName(nameInputField.text);
    }

    public void SetPlayerColor(Color playerColor)
    {
        if (panelBackground != null)
            panelBackground.color = playerColor;
    }

    private void OnVirtualKeyboardPressed(
        InputAction.CallbackContext context)
    {
        if (nameMenu == null || !nameMenu.activeSelf)
            return;

        if (virtualKeyboard == null)
            return;

        if (virtualKeyboard.gameObject.activeSelf)
            virtualKeyboard.HideKeyboard();
        else
            virtualKeyboard.ShowKeyboard();
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
            virtualKeyboardAction.performed -= OnVirtualKeyboardPressed;
    }

    private void OnEnable()
    {
        SubscribeVirtualKeyboard();
    }
}
