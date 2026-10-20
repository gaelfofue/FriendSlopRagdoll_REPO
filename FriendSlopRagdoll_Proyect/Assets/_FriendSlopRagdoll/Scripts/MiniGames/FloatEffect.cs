using UnityEngine;

public class FloatEffect : MonoBehaviour
{
    [Header("Configuración de Flotación")]
    [SerializeField] private float floatSpeed = 1.5f; // Qué tan rápido sube y baja
    [SerializeField] private float floatHeight = 0.25f; // Qué tanto se aleja de su posición original

    private Vector3 startPosition;

    void Start()
    {
        // Guardamos la posición inicial para saber alrededor de qué punto flotar
        startPosition = transform.position;
    }

    void Update()
    {
        // Calculamos la nueva posición en Y usando una onda senoidal
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        // Aplicamos solo el cambio en Y para no afectar el X y Z (por si el objeto se mueve)
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}