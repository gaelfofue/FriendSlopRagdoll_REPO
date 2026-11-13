
using UnityEngine;

public class RagdollHealthMonitor : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerRespawnController respawnController;

    [Header("Separacion de articulaciones")]
    [Tooltip("Distancia maxima permitida entre los puntos de union de un Joint.")]
    [SerializeField] private float maxJointSeparation = 0.5f;

    [Header("Velocidad lineal")]
    [Tooltip("Velocidad maxima permitida en metros por segundo.")]
    [SerializeField] private float maxLinearSpeed = 30f;

    [Header("Velocidad angular")]
    [Tooltip("Velocidad angular maxima permitida en grados por segundo.")]
    [SerializeField] private float maxAngularSpeed = 1500f;

    [Header("Tiempo de deteccion")]
    [Tooltip("Tiempo que debe durar la anomalia antes de hacer respawn.")]
    [SerializeField] private float anomalyDuration = 0.2f;

    [Tooltip("Tiempo inicial durante el que no se comprueban anomalias.")]
    [SerializeField] private float startupGracePeriod = 1f;

    [Header("Opciones")]
    [SerializeField] private bool checkJointSeparation = true;
    [SerializeField] private bool checkLinearSpeed = true;
    [SerializeField] private bool checkAngularSpeed = true;

    private ConfigurableJoint[] joints;
    private Rigidbody[] bodies;

    private float anomalyTimer;
    private float startupTimer;

    private void Awake()
    {
        if (respawnController == null)
            respawnController = GetComponent<PlayerRespawnController>();

        joints = GetComponentsInChildren<ConfigurableJoint>(true);
        bodies = GetComponentsInChildren<Rigidbody>(true);
    }

    private void FixedUpdate()
    {
        if (respawnController == null)
            return;

        if (respawnController.IsRespawning)
        {
            anomalyTimer = 0f;
            return;
        }

        // Evitamos detectar anomalías durante el arranque.
        if (startupTimer < startupGracePeriod)
        {
            startupTimer += Time.fixedDeltaTime;
            return;
        }

        bool anomalyDetected = false;

        if (checkJointSeparation && CheckJointSeparation())
            anomalyDetected = true;

        if (checkLinearSpeed && CheckLinearSpeed())
            anomalyDetected = true;

        if (checkAngularSpeed && CheckAngularSpeed())
            anomalyDetected = true;

        if (anomalyDetected)
        {
            anomalyTimer += Time.fixedDeltaTime;

            if (anomalyTimer >= anomalyDuration)
            {
                respawnController.Respawn();
                anomalyTimer = 0f;
            }
        }
        else
        {
            anomalyTimer = 0f;
        }
    }

    private bool CheckJointSeparation()
    {
        foreach (ConfigurableJoint joint in joints)
        {
            if (joint == null || joint.connectedBody == null)
                continue;

            Vector3 ownAnchor =
                joint.transform.TransformPoint(joint.anchor);

            Vector3 connectedAnchor =
                joint.connectedBody.transform.TransformPoint(
                    joint.connectedAnchor
                );

            float distance = Vector3.Distance(
                ownAnchor,
                connectedAnchor
            );

            if (distance > maxJointSeparation)
                return true;
        }

        return false;
    }

    private bool CheckLinearSpeed()
    {
        foreach (Rigidbody body in bodies)
        {
            if (body == null || body.isKinematic)
                continue;

            if (body.linearVelocity.magnitude > maxLinearSpeed)
                return true;
        }

        return false;
    }

    private bool CheckAngularSpeed()
    {
        foreach (Rigidbody body in bodies)
        {
            if (body == null || body.isKinematic)
                continue;

            float angularSpeed =
                body.angularVelocity.magnitude * Mathf.Rad2Deg;

            if (angularSpeed > maxAngularSpeed)
                return true;
        }

        return false;
    }
}