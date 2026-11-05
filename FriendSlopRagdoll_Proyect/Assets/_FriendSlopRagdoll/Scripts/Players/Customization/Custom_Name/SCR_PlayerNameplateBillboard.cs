using UnityEngine;

public class PlayerNameplateBillboard : MonoBehaviour
{
    private Camera mainCamera;

    private void LateUpdate()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector3 direction =
            transform.position - mainCamera.transform.position;

        if (direction.sqrMagnitude < 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }
}
