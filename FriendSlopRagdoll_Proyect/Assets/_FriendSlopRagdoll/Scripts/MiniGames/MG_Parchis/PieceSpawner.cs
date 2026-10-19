using System.Collections;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private FallingPiece piecePrefab;

    [Header("Área de spawn")]
    [Tooltip("Centro de la zona donde caen las fichas")]
    [SerializeField] private Transform spawnCenter;
    [Tooltip("Radio horizontal de dispersión (evita las esquinas)")]
    [SerializeField] private float spawnRadius = 5f;
    [Tooltip("Altura desde la que caen")]
    [SerializeField] private float spawnHeight = 15f;

    [Header("Ritmo de spawn")]
    [SerializeField] private float spawnInterval = 1.5f;
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

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnOne()
    {
        if (piecePrefab == null || spawnCenter == null)
        {
            Debug.LogError("[PieceSpawner] Falta piecePrefab o spawnCenter.");
            return;
        }

        // Posición aleatoria en un círculo (no en cuadrado, para no acercarse a esquinas)
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = spawnCenter.position + new Vector3(randomCircle.x, spawnHeight, randomCircle.y);

        FallingPiece piece = Instantiate(piecePrefab, spawnPos, Random.rotation);
        currentPieceCount++;

        // Cuando se destruya, decrementamos el contador
        // (usamos un componente auxiliar para no ensuciar FallingPiece)
        var tracker = piece.gameObject.AddComponent<PieceTracker>();
        tracker.OnDestroyed += () => currentPieceCount--;
    }

    // Pequeño helper para saber cuándo muere una ficha
    private class PieceTracker : MonoBehaviour
    {
        public System.Action OnDestroyed;
        void OnDestroy() => OnDestroyed?.Invoke();
    }
}