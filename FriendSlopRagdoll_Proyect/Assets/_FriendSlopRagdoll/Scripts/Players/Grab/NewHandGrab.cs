
using UnityEngine;

public class NewHandGrab : MonoBehaviour
{
    [Header("Arm Joints")]
    [SerializeField] ConfigurableJoint upperArmJoint;
    [SerializeField] ConfigurableJoint foreArmJoint;

    [SerializeField] float grabbingSpring = 100f;
    [SerializeField] float carryingSpring = 255f;
    [SerializeField] float punchingSpring = 255f;

    float upperArmAwakeSpring;
    float foreArmAwakeSpring;

    [SerializeField] Animator anim;

    // A fixed joint created on the fly
    FixedJoint fixedJoint;

    // Our own rigidbody
    Rigidbody rb;

    // Player reference
    NewPlayerController player;

    //Know if its punching
    bool isPunching;

    void Awake()
    {
        player = transform.root.GetComponent<NewPlayerController>();
        rb = GetComponent<Rigidbody>();

        upperArmAwakeSpring = upperArmJoint.slerpDrive.positionSpring;
        foreArmAwakeSpring = foreArmJoint.slerpDrive.positionSpring;

        // Change solver iterations to prevent joint from flexing too much
        rb.solverIterations = 100;
    }

    public void UpdateState()
    {
        // Update grabbing animation
        anim.SetBool("isGrabing", player.isGrabingActive);

        // If grabbing is no longer active, release the object
        if (!player.isGrabingActive && fixedJoint != null)
        {
            ReleaseObject();
        }

        // Update carrying animation
        anim.SetBool("isCarrying", fixedJoint != null);

        // The punch has priority over grab and carry
        if(isPunching)
        {
            SetArmSpring(punchingSpring);
        }
        else if (fixedJoint != null)
        {
            SetArmSpring(carryingSpring);
        }
        else if (player.isGrabingActive && !player.isPunching)
        {
            SetArmSpring(grabbingSpring);
        }
        else
        {
            RestoreArmSpring();
        }
    }

    public void CancelGrabForPunch()
    {
        ReleaseObject();
        anim.SetBool("isCarrying", false);
    }

    public void SetPunching(bool value)
    {
        isPunching = value;
    }

    void ReleaseObject()
    {
        if (fixedJoint == null)
            return;

        // Give the connected rigidbody a bit of force when we let go
        if (fixedJoint.connectedBody != null)
        {
            float forceAmountMultiplier = 0.1f;

            // Check if the connected object belongs to another player
            NewPlayerController otherPlayer =
                fixedJoint.connectedBody.transform.root.GetComponent<NewPlayerController>();

            if (otherPlayer != null)
            {
                if (otherPlayer.isActiveRagdoll)
                    forceAmountMultiplier = 10;
                else
                    forceAmountMultiplier = 15;
            }

            // Toss the object away before removing the joint
            Vector3 throwDirection =
                player.transform.forward + Vector3.up * 0.25f;

            fixedJoint.connectedBody.AddForce(
                throwDirection * forceAmountMultiplier,
                ForceMode.Impulse
            );
        }

        Destroy(fixedJoint);
        fixedJoint = null;
    }

    bool TryCarryObject(Collision collision)
    {
        // Check that we are not in ragdoll mode
        if (!player.isActiveRagdoll)
            return false;

        // Check that grabbing is active
        if (!player.isGrabingActive)
            return false;

        //Check that we arent punching
        if (player.isPunching || isPunching)
            return false;

        // Check that we are not already carrying something
        if (fixedJoint != null)
            return false;

        // Avoid trying to grab yourself
        if (collision.transform.root == player.transform)
            return false;

        // Get the other rigidbody
        Rigidbody otherObjectRigidbody = collision.rigidbody;

        if (otherObjectRigidbody == null)
            return false;

        // Avoid connecting to our own rigidbody
        if (otherObjectRigidbody == rb)
            return false;

        // Add a fixed joint
        fixedJoint = gameObject.AddComponent<FixedJoint>();

        // Connect the joint to the other object's rigidbody
        fixedJoint.connectedBody = otherObjectRigidbody;

        // We'll take care of the anchor point on our own
        fixedJoint.autoConfigureConnectedAnchor = false;

        // Transform the collision point from world to local space
        fixedJoint.connectedAnchor =
            otherObjectRigidbody.transform.InverseTransformPoint(
                collision.GetContact(0).point
            );

        // Set animator to carrying
        anim.SetBool("isCarrying", true);

        return true;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Attempt to carry the object
        TryCarryObject(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        // Also attempt to grab if the button is pressed
        // while the hand is already touching an object
        TryCarryObject(collision);
    }

    void SetArmSpring(float spring)
    {
        JointDrive upperDrive = upperArmJoint.slerpDrive;
        upperDrive.positionSpring = spring;
        upperArmJoint.slerpDrive = upperDrive;

        JointDrive forearmDrive = foreArmJoint.slerpDrive;
        forearmDrive.positionSpring = spring;
        foreArmJoint.slerpDrive = forearmDrive;
    }

    void RestoreArmSpring()
    {
        JointDrive upperDrive = upperArmJoint.slerpDrive;
        upperDrive.positionSpring = upperArmAwakeSpring;
        upperArmJoint.slerpDrive = upperDrive;

        JointDrive forearmDrive = foreArmJoint.slerpDrive;
        forearmDrive.positionSpring = foreArmAwakeSpring;
        foreArmJoint.slerpDrive = forearmDrive;
    }
}