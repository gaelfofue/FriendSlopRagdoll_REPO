
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

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<NewPlayerController>();

        // Guardamos la posición inicial del jugador.
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // Guardamos todos los Rigidbody del jugador.
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
        // Evita que varios colliders provoquen varios respawns.
        if (isRespawning)
            return;

        isRespawning = true;
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        // Esperamos al siguiente paso de física.
        yield return new WaitForFixedUpdate();

        // Detenemos temporalmente la física de todas las partes.
        foreach (Rigidbody body in bodies)
        {
            body.isKinematic = true;
        }

        // Devolvemos el objeto raíz a su posición inicial.
        transform.SetPositionAndRotation(initialPosition, initialRotation);

        // Restauramos la posición y rotación de cada parte física.
        for (int i = 0; i < bodies.Length; i++)
        {
            Transform bodyTransform = bodies[i].transform;

            bodyTransform.localPosition = initialLocalPositions[i];
            bodyTransform.localRotation = initialLocalRotations[i];
        }

        Physics.SyncTransforms();

        // Restauramos el estado físico original de cada Rigidbody.
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

        // Recuperamos el active ragdoll.
        if (playerController != null)
        {
            playerController.MakeActiveRagdoll();
        }

        yield return new WaitForSeconds(respawnDelay);

        isRespawning = false;
    }
}