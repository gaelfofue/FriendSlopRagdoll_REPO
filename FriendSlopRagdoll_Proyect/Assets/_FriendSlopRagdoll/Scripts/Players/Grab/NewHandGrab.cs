
using UnityEngine;

public class NewHandGrab : MonoBehaviour
{
    [SerializeField] Animator anim;

    // A fixed joint created on the fly
    FixedJoint fixedJoint;

    // Our own rigidbody
    Rigidbody rb;

    // Player reference
    NewPlayerController player;

    void Awake()
    {
        player = transform.root.GetComponent<NewPlayerController>();
        rb = GetComponent<Rigidbody>();

        // Change solver iterations to prevent joint from flexing too much
        rb.solverIterations = 30;
    }

    public void UpdateState()
    {
        // Update grabbing animation
        anim.SetBool("isGrabing", player.isGrabingActive);

        // If grabbing is no longer active, release the object
        if (!player.isGrabingActive && fixedJoint != null)
        {
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

        // Update carrying animation
        anim.SetBool("isCarrying", fixedJoint != null);
    }

    bool TryCarryObject(Collision collision)
    {
        // Check that we are not in ragdoll mode
        if (!player.isActiveRagdoll)
            return false;

        // Check that grabbing is active
        if (!player.isGrabingActive)
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
}
