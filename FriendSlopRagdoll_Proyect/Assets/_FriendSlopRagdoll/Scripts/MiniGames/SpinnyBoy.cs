using UnityEngine;

public class SpinnyBoy : MonoBehaviour
{
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0, 100, 0);

    void Update()
    {
        // Multiplicar por Time.deltaTime hace que el giro sea suave e independiente de los FPS
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
