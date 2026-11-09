using UnityEngine;

public class SyncPhysicsObject : MonoBehaviour
{
    Rigidbody rb;
    ConfigurableJoint joint;

    [SerializeField] Rigidbody animatedRb;

    [SerializeField] bool syncAnimation = false;

    //Keep track for starting rotation
    Quaternion startLocalRotation;

    float startSlerpPositionSpring = 0.0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        joint = rb.GetComponent<ConfigurableJoint>();

        //Store values on start
        startLocalRotation = transform.localRotation;
        startSlerpPositionSpring = joint.slerpDrive.positionSpring;
    }

    public void UpdateJointFromAnimation()
    {
        if (!syncAnimation)
            return;

        ConfigurableJointExtensions.SetTargetRotationLocal(joint, animatedRb.transform.localRotation, startLocalRotation);
    }

    public void MakeRagdoll()
    {
        JointDrive jointDrive = joint.slerpDrive;
        jointDrive.positionSpring = 1;
        joint.slerpDrive = jointDrive;
    }

    public void MakeActiveRagdoll()
    {
        JointDrive jointDrive = joint.slerpDrive;
        jointDrive.positionSpring = startSlerpPositionSpring;
        joint.slerpDrive = jointDrive;
    }
}
