using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class PlayerVirtualKeyboard : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform keyboardContainer;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button firstKeyButton;

    [Header("Configuración")]
    [SerializeField] private int maxNameLength = 12;
    [SerializeField] private Button returnToNameMenuButton;

    private MultiplayerEventSystem multiplayerEventSystem;

    private readonly string[] keys =
    {
        "A", "B", "C", "D", "E", "F", "G",
        "H", "I", "J", "K", "L", "M", "N",
        "O", "P", "Q", "R", "S", "T", "U",
        "V", "W", "X", "Y", "Z",
        "BORRAR", "ESPACIO", "OK"
    };

    public void Initialize(MultiplayerEventSystem eventSystem,Button returnButton)
    {
        multiplayerEventSystem = eventSystem;
        returnToNameMenuButton = returnButton;
    }

    public void HideKeyboard()
    {
        gameObject.SetActive(false);

        if (multiplayerEventSystem != null &&
            returnToNameMenuButton != null)
        {
            multiplayerEventSystem.SetSelectedGameObject(null);

            multiplayerEventSystem.SetSelectedGameObject(
                returnToNameMenuButton.gameObject
            );
        }
    }

    private void Awake()
    {
        GenerateKeyboard();
    }

    private void GenerateKeyboard()
    {
        foreach (string key in keys)
        {
            GameObject keyObject = new GameObject(
                "Key_" + key,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button)
            );

            keyObject.transform.SetParent(keyboardContainer, false);

            Image image = keyObject.GetComponent<Image>();
            image.color = new Color(0.25f, 0.25f, 0.25f);

            Button button = keyObject.GetComponent<Button>();

            button.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = button.colors;

            colors.normalColor = new Color(0.22f, 0.25f, 0.32f);
            colors.highlightedColor = new Color(0.35f, 0.65f, 1f);
            colors.selectedColor = new Color(1f, 0.78f, 0.15f);
            colors.pressedColor = new Color(0.15f, 0.85f, 0.65f);

            colors.colorMultiplier = 1.3f;
            colors.fadeDuration = 0.08f;

            button.colors = colors;

            Navigation navigation = new Navigation();
            navigation.mode = Navigation.Mode.Automatic;
            button.navigation = navigation;

            GameObject textObject = new GameObject(
                "Text",
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

            textObject.transform.SetParent(keyObject.transform, false);

            RectTransform textRect =
                textObject.GetComponent<RectTransform>();

            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI text =
                textObject.GetComponent<TextMeshProUGUI>();

            text.text = key;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 17;
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;

            string selectedKey = key;
            button.onClick.AddListener(
                () => PressKey(selectedKey)
            );

            if (firstKeyButton == null)
            {
                firstKeyButton = button;
            }
        }
    }

    private void PressKey(string key)
    {
        if (nameInputField == null)
            return;

        if (key == "BORRAR")
        {
            if (nameInputField.text.Length > 0)
            {
                nameInputField.text =
                    nameInputField.text.Substring(
                        0,
                        nameInputField.text.Length - 1
                    );
            }
        }
        else if (key == "ESPACIO")
        {
            AddCharacter(" ");
        }
        else if (key == "OK")
        {
            HideKeyboard();
        }
        else
        {
            AddCharacter(key);
        }
    }

    private void AddCharacter(string character)
    {
        if (nameInputField.text.Length >= maxNameLength)
            return;

        nameInputField.text += character;
    }

    public void ShowKeyboard()
    {
        gameObject.SetActive(true);

        if (multiplayerEventSystem != null &&
            firstKeyButton != null)
        {
            multiplayerEventSystem.SetSelectedGameObject(null);
            multiplayerEventSystem.SetSelectedGameObject(
                firstKeyButton.gameObject
            );
        }
    }
}