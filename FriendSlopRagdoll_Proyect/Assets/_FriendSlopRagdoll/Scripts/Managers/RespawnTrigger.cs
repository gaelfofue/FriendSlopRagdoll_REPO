
using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerRespawnController respawn =
            other.GetComponentInParent<PlayerRespawnController>();

        if (respawn != null)
        {
            respawn.Respawn();
        }
    }
}