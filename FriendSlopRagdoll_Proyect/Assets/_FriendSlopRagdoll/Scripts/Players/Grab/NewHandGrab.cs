using UnityEngine;

public class NewHandGrab : MonoBehaviour
{
    [SerializeField] Animator anim;

    //A fixed joint that is created on the fly
    FixedJoint fixedJoint;

    //Our own rigidbody
    Rigidbody rb;

    //References
    NewPlayerController player;

    void Awake()
    {
        player = transform.root.GetComponent<NewPlayerController>();
        rb = GetComponent<Rigidbody>();
    }

    bool TryCarryObject(Collision collision)
    {
        //Check that we are not in active ragdoll mode
        if(!player.isActiveRagdoll)
            return false;
        
        //Check if we are already carrying something
        if(fixedJoint != null)
            return false;

        //Avoid trying to grab yourself
        if (collision.transform.root == player.transform) 
            return false;

        //Get the ohter rigidbody if there is one
        if(!collision.collider.TryGetComponent(out Rigidbody otherObjectRigidbody))
            return false;

        //Add a fixed joint
        fixedJoint = transform.gameObject.AddComponent<FixedJoint>();

        //Connect the joint to the other objects rigidbody
        fixedJoint.connectedBody = otherObjectRigidbody;

        //We'll take care of the anchor point on our own
        fixedJoint.autoConfigureConnectedAnchor = false;

        //Transform the collision point from world to local space
        fixedJoint.connectedAnchor = collision.transform.InverseTransformPoint(collision.GetContact(0).point);

        //Set animator to carrying
        anim.SetBool("isCarrying", true);
        return true;
    }

    void OnCollisionEnter(Collision collision)
    {
        //Attempt to carry the other object
        TryCarryObject(collision);
    }
}
