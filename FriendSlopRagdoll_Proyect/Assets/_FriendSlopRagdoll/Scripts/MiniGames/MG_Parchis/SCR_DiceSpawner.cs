using System.Collections;
using UnityEngine;

public class DiceSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject dicePrefab;

    [Header("Área de aparición")]
    [SerializeField] private BoxCollider spawnArea;

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

        float randomY = Random.Range(minHeight, maxHeight);

        Vector3 spawnPosition = new Vector3(
            randomX,
            bounds.center.y + randomY,
            randomZ
        );

        GameObject newDice = Instantiate(
            dicePrefab,
            spawnPosition,
            Random.rotation
        );

        currentDiceCount++;

        DiceCounter counter = newDice.AddComponent<DiceCounter>();
        counter.OnDestroyed += () => currentDiceCount--;
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