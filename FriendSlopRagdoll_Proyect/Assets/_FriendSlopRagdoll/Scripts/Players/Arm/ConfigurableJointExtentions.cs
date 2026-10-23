using UnityEngine;
// Utilidad para setear el targetRotation de un ConfigurableJoint
// usando rotaciones locales intuitivas (en vez del espacio interno raro del joint).
// Es una clase estatica pura, sin estado, sin MonoBehaviour.
public static class ConfigurableJointExtensions
{
    public static void SetTargetRotationLocal(this ConfigurableJoint joint, Quaternion targetLocalRotation, Quaternion startLocalRotation)
    {
        Vector3 right = joint.axis;
        Vector3 forward = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
        Vector3 up = Vector3.Cross(forward, right).normalized;
        Quaternion worldToJointSpace = Quaternion.LookRotation(forward, up);

        Quaternion resultRotation = Quaternion.Inverse(worldToJointSpace);
        resultRotation *= Quaternion.Inverse(targetLocalRotation) * startLocalRotation;
        resultRotation *= worldToJointSpace;

        joint.targetRotation = resultRotation;
    }
}
