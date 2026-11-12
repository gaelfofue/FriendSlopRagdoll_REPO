using System.Collections;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    [System.Serializable]
    private class PieceVariant
    {
        public FallingPiece prefab;
        public Material whiteMaterial;
        public Material blackMaterial;
    }

    [Header("Prefabs y materiales")]
    [SerializeField] private PieceVariant chessPiece1;
    [SerializeField] private PieceVariant chessPiece2;
    [SerializeField] private PieceVariant checkersPiece;

    [Header("Probabilidades de aparición")]
    [Min(0f)]
    [SerializeField] private float normalSpawnWeight = 75f;

    [Min(0f)]
    [SerializeField] private float specialSpawnWeight = 20f;

    [Min(0f)]
    [SerializeField] private float epicSpawnWeight = 5f;

    [Header("Área de spawn")]
    [SerializeField] private BoxCollider spawnArea;
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

    private PieceVariant SelectPieceVariant()
    {
        float totalWeight = 0f;

        if (IsValid(chessPiece1))
            totalWeight += normalSpawnWeight;

        if (IsValid(chessPiece2))
            totalWeight += specialSpawnWeight;

        if (IsValid(checkersPiece))
            totalWeight += epicSpawnWeight;

        if (totalWeight <= 0f)
        {
            Debug.LogError(
                "[PieceSpawner] No hay prefabs y materiales válidos."
            );
            return null;
        }

        float randomValue = Random.Range(0f, totalWeight);

        if (IsValid(chessPiece1))
        {
            if (randomValue < normalSpawnWeight)
                return chessPiece1;

            randomValue -= normalSpawnWeight;
        }

        if (IsValid(chessPiece2))
        {
            if (randomValue < specialSpawnWeight)
                return chessPiece2;

            randomValue -= specialSpawnWeight;
        }

        if (IsValid(checkersPiece))
            return checkersPiece;

        return null;
    }

    private bool IsValid(PieceVariant variant)
    {
        return variant != null &&
               variant.prefab != null &&
               (variant.whiteMaterial != null ||
                variant.blackMaterial != null);
    }


    private void SpawnOne()
    {
        if (spawnArea == null)
        {
            Debug.LogError("[PieceSpawner] Falta asignar Spawn Area.");
            return;
        }

        PieceVariant selected = SelectPieceVariant();
        if (selected == null) return;

        Material selectedMaterial;

        if (selected.whiteMaterial != null && selected.blackMaterial != null)
        {
            selectedMaterial = Random.value < 0.5f
                ? selected.whiteMaterial
                : selected.blackMaterial;
        }
        else
        {
            selectedMaterial = selected.whiteMaterial != null
                ? selected.whiteMaterial
                : selected.blackMaterial;
        }

        Vector3 randomLocalPoint = new Vector3(
            Random.Range(-0.5f, 0.5f) * spawnArea.size.x + spawnArea.center.x,
            Random.Range(-0.5f, 0.5f) * spawnArea.size.y + spawnArea.center.y,
            Random.Range(-0.5f, 0.5f) * spawnArea.size.z + spawnArea.center.z
        );

        Vector3 spawnPosition = spawnArea.transform.TransformPoint(randomLocalPoint);

        FallingPiece piece = Instantiate (selected.prefab, spawnPosition, Random.rotation);

        Renderer pieceRenderer = piece.GetComponentInChildren<MeshRenderer>();

        if (pieceRenderer != null)
            pieceRenderer.sharedMaterial = selectedMaterial;

        currentPieceCount++;

        PieceTracker tracker = piece.gameObject.AddComponent<PieceTracker>();
        tracker.OnDestroyed += () => currentPieceCount--;
    }

    private class PieceTracker : MonoBehaviour
    {
        public System.Action OnDestroyed;

        private void OnDestroy()
        {
            OnDestroyed?.Invoke();
        }
    }
}
