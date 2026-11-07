using UnityEngine;

public class SyncPhysicsObject : MonoBehaviour
{
    Rigidbody rb;
    ConfigurableJoint joint;

    [SerializeField] Rigidbody animatedRb;

    [SerializeField] bool syncAnimation = false;

    //Keep track for starting rotation
    Quaternion startLocalRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        joint = rb.GetComponent<ConfigurableJoint>();

        //Store the starting local rotation
        startLocalRotation = transform.localRotation;
    }

    public void UpdateJointFromAnimation()
    {
        if (!syncAnimation)
            return;

        ConfigurableJointExtensions.SetTargetRotationLocal(joint, animatedRb.transform.localRotation, startLocalRotation);
    }
}
