using System.Collections;
using UnityEngine;

public class DiceSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject dicePrefab;

    [Header("Aviso de aterrizaje")]
    [SerializeField] private GameObject warningPrefab;
    [SerializeField] private float warningDuration = 2f;
    [SerializeField] private float groundOffset = 0.05f;

    [Header("Área de aparición")]
    [SerializeField] private BoxCollider spawnArea;
    [SerializeField] private LayerMask groundLayer;

    [Header("Altura de aparición")]
    [SerializeField] private float minHeight = 10f;
    [SerializeField] private float maxHeight = 18f;

    [Header("Tiempo entre dados")]
    [SerializeField] private float minSpawnInterval = 5f;
    [SerializeField] private float maxSpawnInterval = 12f;

    [Header("Límite de dados")]
    [SerializeField] private int maxDiceOnMap = 3;

    private int currentDiceCount;
    private bool isSpawning;

    public void StartSpawning()
    {
        if (isSpawning)
            return;

        isSpawning = true;
        StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            if (currentDiceCount < maxDiceOnMap)
                SpawnDice();

            float waitTime = Random.Range(
                minSpawnInterval,
                maxSpawnInterval
            );

            yield return new WaitForSeconds(waitTime);
        }
    }

    private void SpawnDice()
    {
        if (dicePrefab == null || spawnArea == null)
        {
            Debug.LogWarning("Falta asignar el prefab o el área de aparición.");
            return;
        }

        Bounds bounds = spawnArea.bounds;

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);

        // Calculamos una sola posición de aterrizaje.
        Vector3 rayOrigin = new Vector3(
            randomX,
            bounds.max.y + 50f,
            randomZ
        );

        if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 100f, groundLayer))
        {
            Debug.LogWarning("No se encontró el suelo para el dado.");
            return;
        }

        Vector3 landingPosition = hit.point + Vector3.up * groundOffset;

        float randomY = Random.Range(minHeight, maxHeight);

        Vector3 spawnPosition = new Vector3(
            landingPosition.x,
            landingPosition.y + randomY,
            landingPosition.z
        );

        StartCoroutine(SpawnDiceSequence(landingPosition, spawnPosition));
    }

    private IEnumerator SpawnDiceSequence(
    Vector3 landingPosition,
    Vector3 spawnPosition)
    {
        Debug.Log("SECUENCIA DE DADO INICIADA");

        GameObject warning = null;

        if (warningPrefab != null)
        {
            warning = Instantiate(
                warningPrefab,
                landingPosition,
                Quaternion.identity
            );

            warning.SetActive(true);

            Renderer[] renderers = warning.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer r in renderers)
            {
                r.enabled = true;
            }

            LineRenderer[] lines = warning.GetComponentsInChildren<LineRenderer>(true);

            foreach (LineRenderer line in lines)
            {
                line.enabled = true;
            }
        }

        yield return new WaitForSeconds(warningDuration);

        GameObject newDice = Instantiate(
            dicePrefab,
            spawnPosition,
            Random.rotation
        );

        currentDiceCount++;

        DiceCounter counter = newDice.AddComponent<DiceCounter>();
        counter.OnDestroyed += () => currentDiceCount--;

        if (warning != null)
            Destroy(warning);
    }
}

public class DiceCounter : MonoBehaviour
{
    public System.Action OnDestroyed;

    private void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }
}