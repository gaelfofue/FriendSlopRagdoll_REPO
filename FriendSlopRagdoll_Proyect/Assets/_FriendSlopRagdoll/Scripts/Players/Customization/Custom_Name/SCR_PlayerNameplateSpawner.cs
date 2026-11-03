using UnityEngine;

public class PlayerNameplateSpawner : MonoBehaviour
{
    [Header("Nameplate")]
    [SerializeField] private GameObject nameplatePrefab;

    [Header("Anchor")]
    [SerializeField] private Transform nameplateAnchor;

    private PlayerNameplate currentNameplate;
    private Transform currentNameplateTransform;

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

        GameObject nameplateObject = Instantiate(nameplatePrefab);

        currentNameplateTransform = nameplateObject.transform;

        nameplateObject.transform.position = nameplateAnchor.position;
        nameplateObject.transform.rotation = Quaternion.identity;

        currentNameplate = nameplateObject.GetComponent<PlayerNameplate>();

        if (currentNameplate == null)
        {
            Debug.LogError("El prefab necesita el componente PlayerNameplate.");
            return;
        }

        currentNameplate.Setup(playerName, playerColor);
    }

    private void LateUpdate()
    {
        if (currentNameplateTransform == null || nameplateAnchor == null)
            return;

        currentNameplateTransform.position = nameplateAnchor.position;
    }
}