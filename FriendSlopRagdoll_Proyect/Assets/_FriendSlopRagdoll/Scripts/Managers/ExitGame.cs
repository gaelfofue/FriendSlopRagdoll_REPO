using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void SalirDelJuego()
    {
        // Cierra la aplicación cuando el juego ya está compilado (.exe, .apk, etc.)
        Application.Quit();

        // Detiene el modo Play si estás probando dentro del editor de Unity
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}