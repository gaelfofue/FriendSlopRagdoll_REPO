using UnityEngine;

[RequireComponent(typeof(ConfigurableJoint))]
public class ArmController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] ConfigurableJoint shoulderJoint;
    [SerializeField] PlayerInputRelay input;

    [Header("Arm Identity")]
    [SerializeField] bool isLeftArm;

    [Header("Poses (LOCAL del hueso)")]
    [SerializeField] Vector3 idlePoseEuler = Vector3.zero;
    [SerializeField] Vector3 forwardPoseEuler = new Vector3(-90f, 0f, 0f);

    [Header("Smoothing")]
    [SerializeField] float flexSmoothSpeed = 8f;

    float rawFlexInput;
    float smoothedFlex;

    Quaternion startLocalRotation;   // rotación local inicial del hueso
    Quaternion jointSpace;           // espacio del joint (axis/secondaryAxis)

    public float CurrentFlexAmount => smoothedFlex;

    void Awake()
    {
        if (!shoulderJoint) shoulderJoint = GetComponent<ConfigurableJoint>();

        // Rotación local inicial REAL del hueso
        startLocalRotation = transform.localRotation;

        // Base del espacio del joint (muy importante)
        jointSpace = Quaternion.LookRotation(shoulderJoint.axis, shoulderJoint.secondaryAxis);

        // Recomendado para controlar rotación con targetRotation
        shoulderJoint.configuredInWorldSpace = false;

        // Recomendación fuerte: usar SLERP para que “solo levante” sin pelearte con ejes X/Y/Z
        shoulderJoint.rotationDriveMode = RotationDriveMode.Slerp;
    }

    void OnEnable()
    {
        if (isLeftArm) input.OnFlexLeftArm += HandleFlexInput;
        else input.OnFlexRightArm += HandleFlexInput;
    }

    void OnDisable()
    {
        if (isLeftArm) input.OnFlexLeftArm -= HandleFlexInput;
        else input.OnFlexRightArm -= HandleFlexInput;
    }

    void HandleFlexInput(float value)
    {
        rawFlexInput = Mathf.Clamp01(value);
    }

    void FixedUpdate()
    {
        smoothedFlex = Mathf.MoveTowards(
            smoothedFlex,
            rawFlexInput,
            flexSmoothSpeed * Time.fixedDeltaTime
        );

        // Interpola entre poses en LOCAL del hueso
        Vector3 euler = Vector3.Lerp(idlePoseEuler, forwardPoseEuler, smoothedFlex);
        Quaternion desiredLocalRotation = Quaternion.Euler(euler);

        shoulderJoint.targetRotation = ToJointTargetRotation(desiredLocalRotation);
    }

    // Convierte "quiero esta rotación LOCAL del hueso" a targetRotation correcto del ConfigurableJoint
    Quaternion ToJointTargetRotation(Quaternion desiredLocalRotation)
    {
        // Fórmula estándar: targetRotation está en el espacio del joint y es “inversa”
        return Quaternion.Inverse(jointSpace)
             * Quaternion.Inverse(desiredLocalRotation)
             * startLocalRotation
             * jointSpace;
    }
}