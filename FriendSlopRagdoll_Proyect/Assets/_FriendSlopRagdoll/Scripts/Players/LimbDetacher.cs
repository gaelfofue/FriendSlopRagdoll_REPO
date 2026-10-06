using System;
using UnityEngine;

// Vive en el UpperArm. Monitorea el Joint hombro/torso y lo rompe si la fuerza supera el umbral de forma sostenida
public class LimbDetacher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] ConfigurableJoint joint;
    [SerializeField] Rigidbody limbRigidbody;

    [Header("Detach Settings")]
    [SerializeField] float detachForceThreshold = 2000f;
    [SerializeField] float sustainedTimeRequired = 0.15f;

    float overThresholdTimer;

    JointDrive origX, origY, origZ, origAngX, origAngYZ;
    SoftJointLimit origLinearLimit;
    Vector3 origAnchor, origConnectedAnchor, origAxis, origSecondaryAxis;

    public bool IsDetached { get; private set; }
    public event Action<LimbDetacher> OnDetached;
    public event Action<LimbDetacher> OnReattached;

    void Awake() => CacheJointSettings();

    void CacheJointSettings()
    {
        origX = joint.xDrive; origY = joint.yDrive; origZ = joint.zDrive;
        origAngX = joint.angularXDrive; origAngYZ = joint.angularYZDrive;
        origLinearLimit = joint.linearLimit;
        origAnchor = joint.anchor; origConnectedAnchor = joint.connectedAnchor;
        origAxis = joint.axis; origSecondaryAxis = joint.secondaryAxis;
    }

    void FixedUpdate()
    {
        if (IsDetached || joint == null) return;

        float force = joint.currentForce.magnitude;

        if (force >= detachForceThreshold)
        {
            overThresholdTimer += Time.fixedDeltaTime;
            if (overThresholdTimer >= sustainedTimeRequired) Detach();
        }
        else
        {
            overThresholdTimer = 0f;
        }
    }

    void Detach()
    {
        IsDetached = true;
        Destroy(joint);
        joint = null;
        OnDetached?.Invoke(this);
    }

    // LLamado por LimbSocket cuando el brazo vuelve a su sitio

    public void Reattach(Rigidbody torsoBody)
    {
        if (!IsDetached) return;

        joint = limbRigidbody.gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = torsoBody;
        joint.anchor = origAnchor;
        joint.connectedAnchor = origConnectedAnchor;
        joint.axis = origAxis;
        joint.secondaryAxis = origSecondaryAxis;
        joint.xDrive = origX; joint.yDrive = origY; joint.zDrive = origZ;
        joint.angularXDrive = origAngX; joint.angularYZDrive = origAngYZ;
        joint.linearLimit = origLinearLimit;
        // Reconfigurar los Motion/AngularMotion locks que se usen

        IsDetached = false;
        OnReattached?.Invoke(this);
    }
}
