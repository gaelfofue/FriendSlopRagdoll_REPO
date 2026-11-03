using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameplate : MonoBehaviour
{
    [Header("Referencias visuales")]
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text arrowText;

    public void Setup(string playerName, Color playerColor)
    {
        if (nameText != null)
            nameText.text = playerName;

        if (background != null)
            background.color = playerColor;

        if (arrowText != null)
            arrowText.color = playerColor;
    }
}