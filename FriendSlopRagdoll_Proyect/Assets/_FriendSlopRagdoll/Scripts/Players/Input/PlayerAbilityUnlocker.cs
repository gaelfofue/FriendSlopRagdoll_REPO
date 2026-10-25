using UnityEngine;

//Escucha a los 2 brazos y activa/desactiva habilidades segun cuantos quedan
public class PlayerAbilityUnlocker : MonoBehaviour
{
    [SerializeField] LimbDetacher leftArm;
    [SerializeField] LimbDetacher rightArm;
    [SerializeField] MonoBehaviour diveAbility;

    void OnEnable()
    {
        leftArm.OnDetached += Refresh; leftArm.OnReattached += Refresh;
        rightArm.OnDetached += Refresh; rightArm.OnReattached += Refresh;
        Refresh(null);
    }

    void OnDisable()
    {
        leftArm.OnDetached -= Refresh; leftArm.OnReattached -= Refresh;
        rightArm.OnDetached -= Refresh; rightArm.OnReattached -= Refresh;
    }

    void Refresh(LimbDetacher _)
    {
        diveAbility.enabled = leftArm.IsDetached && rightArm.IsDetached;
    }
}
