using UnityEngine;

// TEMPORAL. Ponlo en el UpperArm para ver en el Inspector que esta pasando
// con el joint. Borralo cuando el control funcione.
[RequireComponent(typeof(ConfigurableJoint))]
public class ArmDebugger : MonoBehaviour
{
    [SerializeField] ConfigurableJoint joint;
    [SerializeField] Rigidbody body;
    [SerializeField] bool logEverySecond = true;

    float timer;

    void Reset() { joint = GetComponent<ConfigurableJoint>(); body = GetComponent<Rigidbody>(); }

    void FixedUpdate()
    {
        if (!logEverySecond) return;
        timer += Time.fixedDeltaTime;
        if (timer < 1f) return;
        timer = 0f;

        Debug.Log(
            $"[ArmJointDebug] {name}\n" +
            $"  angularXMotion={joint.angularXMotion} angularYMotion={joint.angularYMotion} angularZMotion={joint.angularZMotion}\n" +
            $"  angularXDrive.spring={joint.angularXDrive.positionSpring} damper={joint.angularXDrive.positionDamper} maxForce={joint.angularXDrive.maximumForce}\n" +
            $"  angularYZDrive.spring={joint.angularYZDrive.positionSpring} damper={joint.angularYZDrive.positionDamper} maxForce={joint.angularYZDrive.maximumForce}\n" +
            $"  targetRotation={joint.targetRotation.eulerAngles}\n" +
            $"  currentTorque={joint.currentTorque.magnitude:F2}\n" +
            $"  body.isKinematic={body.isKinematic} useGravity={body.useGravity}\n" +
            $"  body.angularVelocity={body.angularVelocity.magnitude:F2}"
        );
    }
}