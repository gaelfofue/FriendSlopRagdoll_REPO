using UnityEngine;

public class PlayerNameplateSpawner : MonoBehaviour
{
    [Header("Nameplate")]
    [SerializeField] private GameObject nameplatePrefab;

    [Header("Anchor")]
    [SerializeField] private Transform nameplateAnchor;

    private PlayerNameplate currentNameplate;

    public void CreateNameplate(string playerName, Color playerColor)
    {
        if (nameplatePrefab == null)
        {
            Debug.LogError("Falta asignar el prefab PlayerNameplate.");
            return;
        }

        if (nameplateAnchor == null)
        {
            Debug.LogError("Falta asignar NameplateAnchor.");
            return;
        }

        GameObject nameplateObject = Instantiate(
            nameplatePrefab,
            nameplateAnchor
        );

        nameplateObject.transform.localPosition = Vector3.zero;
        nameplateObject.transform.localRotation = Quaternion.identity;

        currentNameplate = nameplateObject.GetComponent<PlayerNameplate>();

        if (currentNameplate == null)
        {
            Debug.LogError("El prefab necesita el componente PlayerNameplate.");
            return;
        }

        currentNameplate.Setup(playerName, playerColor);
    }
}