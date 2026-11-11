using System.Collections;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private FallingPiece piecePrefab;
    [SerializeField] private FallingPiece specialPiecePrefab;
    [SerializeField] private FallingPiece epicPiecePrefab;

    [Header("Probabilidades de aparición")]
    [Min(0f)]
    [SerializeField] private float normalSpawnWeight = 75f;

    [Min(0f)]
    [SerializeField] private float specialSpawnWeight = 20f;

    [Min(0f)]
    [SerializeField] private float epicSpawnWeight = 5f;

    [Header("Área de spawn")]
    [SerializeField] private Transform spawnCenter;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private float spawnHeight = 15f;

    [Header("Ritmo aleatorio de spawn")]
    [SerializeField] private float minSpawnInterval = 0.8f;
    [SerializeField] private float maxSpawnInterval = 2.5f;
    [SerializeField] private int maxPiecesOnBoard = 15;

    private int currentPieceCount = 0;
    private bool isSpawning = false;

    public void StartSpawning()
    {
        if (isSpawning) return;

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
            if (currentPieceCount < maxPiecesOnBoard)
                SpawnOne();

            float randomInterval = Random.Range(
                minSpawnInterval,
                maxSpawnInterval
            );

            yield return new WaitForSeconds(randomInterval);
        }
    }

    private FallingPiece SelectPiecePrefab()
    {
        float normalWeight = piecePrefab != null
            ? normalSpawnWeight : 0f;

        float specialWeight = specialPiecePrefab != null
            ? specialSpawnWeight : 0f;

        float epicWeight = epicPiecePrefab != null
            ? epicSpawnWeight : 0f;

        float totalWeight =
            normalWeight + specialWeight + epicWeight;

        if (totalWeight <= 0f)
        {
            Debug.LogError("[PieceSpawner] No hay prefabs válidos.");
            return null;
        }

        float randomValue = Random.Range(0f, totalWeight);

        if (randomValue < normalWeight)
            return piecePrefab;

        randomValue -= normalWeight;

        if (randomValue < specialWeight)
            return specialPiecePrefab;

        return epicPiecePrefab;
    }

    private void SpawnOne()
    {
        if (spawnCenter == null)
        {
            Debug.LogError("[PieceSpawner] Falta spawnCenter.");
            return;
        }

        FallingPiece selectedPrefab = SelectPiecePrefab();

        if (selectedPrefab == null)
            return;

        Vector2 randomCircle =
            Random.insideUnitCircle * spawnRadius;

        Vector3 spawnPos =
            spawnCenter.position +
            new Vector3(randomCircle.x, spawnHeight, randomCircle.y);

        FallingPiece piece = Instantiate(
            selectedPrefab,
            spawnPos,
            Random.rotation
        );

        currentPieceCount++;

        PieceTracker tracker =
            piece.gameObject.AddComponent<PieceTracker>();

        tracker.OnDestroyed += () => currentPieceCount--;
    }

    private class PieceTracker : MonoBehaviour
    {
        public System.Action OnDestroyed;

        void OnDestroy()
        {
            OnDestroyed?.Invoke();
        }
    }
}