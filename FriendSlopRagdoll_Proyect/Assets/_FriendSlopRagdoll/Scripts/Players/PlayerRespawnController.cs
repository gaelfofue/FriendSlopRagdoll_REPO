
using System.Collections;
using UnityEngine;

public class PlayerRespawnController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private NewPlayerController playerController;

    [Header("Configuración")]
    [SerializeField] private float respawnDelay = 0.1f;

    private Rigidbody[] bodies;
    private Vector3[] initialLocalPositions;
    private Quaternion[] initialLocalRotations;
    private bool[] initialKinematicStates;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private bool isRespawning;

    public bool IsRespawning => isRespawning;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<NewPlayerController>();

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        bodies = GetComponentsInChildren<Rigidbody>(true);

        initialLocalPositions = new Vector3[bodies.Length];
        initialLocalRotations = new Quaternion[bodies.Length];
        initialKinematicStates = new bool[bodies.Length];

        for (int i = 0; i < bodies.Length; i++)
        {
            Transform bodyTransform = bodies[i].transform;

            initialLocalPositions[i] = bodyTransform.localPosition;
            initialLocalRotations[i] = bodyTransform.localRotation;
            initialKinematicStates[i] = bodies[i].isKinematic;
        }
    }

    public void Respawn()
    {
        if (isRespawning)
            return;

        isRespawning = true;
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForFixedUpdate();

        // Congelamos todos los cuerpos.
        foreach (Rigidbody body in bodies)
        {
            body.isKinematic = true;
        }

        // Restauramos la posición del jugador.
        transform.SetPositionAndRotation(initialPosition, initialRotation);

        // Restauramos las partes físicas.
        for (int i = 0; i < bodies.Length; i++)
        {
            Transform bodyTransform = bodies[i].transform;

            bodyTransform.localPosition = initialLocalPositions[i];
            bodyTransform.localRotation = initialLocalRotations[i];
        }

        Physics.SyncTransforms();

        // Restauramos velocidades y estados físicos.
        for (int i = 0; i < bodies.Length; i++)
        {
            Rigidbody body = bodies[i];

            body.isKinematic = initialKinematicStates[i];

            if (!body.isKinematic)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }

        // Reactivamos el active ragdoll.
        if (playerController != null)
        {
            playerController.MakeActiveRagdoll();
        }

        yield return new WaitForSeconds(respawnDelay);

        isRespawning = false;
    }
}