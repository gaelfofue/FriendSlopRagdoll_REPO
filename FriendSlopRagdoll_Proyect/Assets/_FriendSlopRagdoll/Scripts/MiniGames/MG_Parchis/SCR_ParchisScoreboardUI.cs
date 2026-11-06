using UnityEngine;
using TMPro;

public class ParchisScoreboardUI : MonoBehaviour
{
    [Header("Textos de puntuación")]
    [SerializeField] private TMP_Text[] scoreTexts = new TMP_Text[4];

    [Header("Zonas de entrega")]
    [SerializeField] private DeliveryZone[] deliveryZones = new DeliveryZone[4];

    private void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            if (scoreTexts[i] == null || deliveryZones[i] == null)
                continue;

            float points = deliveryZones[i].TotalPoints;
            scoreTexts[i].text = points.ToString("0.##");
        }
    }

    private string GetPlayerName(int playerIndex)
    {
        switch (playerIndex)
        {
            case 0: return "0";
            case 1: return "0";
            case 2: return "0";
            case 3: return "0";
            default: return "JUGADOR";
        }
    }
}