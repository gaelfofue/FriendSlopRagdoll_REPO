using Cinemachine;
using UnityEngine;

public class DetectCollisions : MonoBehaviour
{
    NewPlayerController player;
    Rigidbody rb;

    ContactPoint[] contactPoints = new ContactPoint[5];

    void Awake()
    {
        player = GetComponentInParent<NewPlayerController>();
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!player.isActiveRagdoll)
            return;

        if (!collision.collider.CompareTag("CauseDamage"))
            return;

        //Avoid having the player hurt themselves
        if (collision.collider.transform.root == player.transform)
            return;

        int numberOfContacts = collision.GetContacts(contactPoints);

        for (int i = 0; i < numberOfContacts; i++)
        {
            ContactPoint contactPoint = contactPoints[i];

            //Get the contact impusle
            Vector3 contactImpulse = contactPoint.impulse / Time.fixedDeltaTime;

            //Check that the force mas great enough to cause a knockout
            if (contactImpulse.magnitude < 15)
                continue;

            player.OnPlayerBodyPartHit();

            Vector3 forceDirection = (contactImpulse + Vector3.up) * 0.5f;

            //Limit the force so it doesnt get too big
            forceDirection = Vector3.ClampMagnitude(forceDirection, 30);

            Debug.DrawRay(rb.position, forceDirection*40, Color.red, 4);

            //Increase the effect of the hit
            rb.AddForce(forceDirection,ForceMode.Impulse);
        }
    }
}
