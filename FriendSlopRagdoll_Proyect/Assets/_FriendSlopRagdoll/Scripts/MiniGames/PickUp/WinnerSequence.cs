using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine; // Asegúrate de tener el namespace de Cinemachine

public class WinnerSequence : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineVirtualCamera winnerVCam; // Arrastra aquí tu cámara de victoria
    [SerializeField] private float zoomFOV = 30f; // Campo de visión para el zoom (más bajo = más zoom)

    [Header("Slow Motion")]
    [SerializeField] private float slowMotionTimeScale = 0.2f;
    [SerializeField] private float slowMotionDuration = 3f; // Cuánto dura la cámara lenta antes de cambiar escena

    [Header("Escena")]
    [SerializeField] private string nextSceneName = "Menu"; // Nombre de la escena a cargar

    private float originalFixedDeltaTime;

    void Awake()
    {
        // Guardamos el valor original para restaurarlo (buena práctica)
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    public void StartWinnerSequence(GameObject player, GameObject pickup)
    {
        StartCoroutine(WinnerRoutine(player, pickup));
    }

    private IEnumerator WinnerRoutine(GameObject player, GameObject pickup)
    {
        // 1. Configurar la cámara de Cinemachine para que enfoque al jugador y al pickup
        if (winnerVCam != null)
        {
            winnerVCam.Follow = player.transform;
            winnerVCam.LookAt = pickup.transform;

            // Ajustamos el FOV para el zoom (si usas una lente Perspective)
            var lens = winnerVCam.m_Lens;
            lens.FieldOfView = zoomFOV;
            winnerVCam.m_Lens = lens;

            // Subimos la prioridad para que esta cámara tome el control
            winnerVCam.Priority = 100;
        }

        // 2. Aplicar Cámara Lenta
        Time.timeScale = slowMotionTimeScale;
        // Ajustamos el FixedDeltaTime para que la física no explote con el timeScale bajo
        Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;

        // 3. Esperar el tiempo de "dramatismo" (usando tiempo real para que no se alargue demasiado)
        yield return new WaitForSecondsRealtime(slowMotionDuration);

        // 4. Restaurar el tiempo (importante antes de cambiar de escena)
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        // 5. Cambiar de escena
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("No se ha definido el nombre de la siguiente escena.");
        }
    }
}