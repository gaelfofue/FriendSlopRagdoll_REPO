using UnityEngine;

//Escucha a los 2 brazos y activa/desactiva habilidades segun cuantos quedan
public class PlayerAbilityUnlocker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] LimbDetacher leftArm;
    [SerializeField] LimbDetacher rightArm;

    [Tooltip("Componente del dive (futuro). Se activa cuando ambos brazos estan desprendidos.")]
    [SerializeField] MonoBehaviour diveAbility;

    public bool DiveUnlocked { get; private set; }

    void OnEnable()
    {
        if (leftArm != null) leftArm.OnDetached += HandleArmDetached;
        if (rightArm != null) rightArm.OnDetached += HandleArmDetached;
        Refresh();
    }

    void OnDisable()
    {
        if (leftArm != null) leftArm.OnDetached -= HandleArmDetached;
        if (rightArm != null) rightArm.OnDetached -= HandleArmDetached;
    }

    void HandleArmDetached(LimbDetacher _) => Refresh();

    void Refresh()
    {
        bool bothDetached = (leftArm == null || leftArm.IsDetached)
                         && (rightArm == null || rightArm.IsDetached);

        DiveUnlocked = bothDetached;

        if (diveAbility != null)
            diveAbility.enabled = bothDetached;

        Debug.Log($"[PlayerAbilityUnlocker] Dive unlocked: {bothDetached}");
    }
}
