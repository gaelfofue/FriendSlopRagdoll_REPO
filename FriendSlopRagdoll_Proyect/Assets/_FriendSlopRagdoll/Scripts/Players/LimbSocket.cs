using UnityEngine;

//Trigger en el hombro del torso detecta braszos desprendidos y los reconecta tras mantener un contacto un rato

public class LimbSocket : MonoBehaviour
{
    [SerializeField] Rigidbody torsoRigidbody;
    [SerializeField] float reattachHoldTime = 0.5f;

    float contactTimer;
    LimbDetacher current;

    void OnTriggerStay(Collider other)
    {
        if (!other.TryGetComponent(out LimbDetacher limb) || !limb.IsDetached) return;

        current = limb;
        contactTimer += Time.deltaTime;

        if (contactTimer >= reattachHoldTime)
        {
            limb.Reattach(torsoRigidbody);
            contactTimer = 0f;
            current = null;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.TryGetComponent(out LimbDetacher limb) && limb == current)
        {
            contactTimer = 0f;
            current = null;
        }
    }
}
