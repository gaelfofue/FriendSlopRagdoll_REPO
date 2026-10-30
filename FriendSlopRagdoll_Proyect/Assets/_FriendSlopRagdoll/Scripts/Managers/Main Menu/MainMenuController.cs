
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Menu Screens")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject gameModeCanvas;
    [SerializeField] private GameObject onlineMenuCanvas;
    [SerializeField] private GameObject settingsCanvas;

    private GameObject currentCanvas;

    private void Awake()
    {
        ShowScreen(mainMenuCanvas);
    }

    public void ShowMainMenu()
    {
        ShowScreen(mainMenuCanvas);
    }

    public void ShowGameMode()
    {
        ShowScreen(gameModeCanvas);
    }

    public void ShowOnlineMenu()
    {
        ShowScreen(onlineMenuCanvas);
    }

    public void ShowSettings()
    {
        Debug.Log("OPTIONS PULSADO: ShowSettings se ha ejecutado.");

        if (settingsCanvas == null)
        {
            Debug.LogError("ERROR: Settings Canvas no está asignado.", this);
            return;
        }

        Debug.Log("Canvas de ajustes: " + settingsCanvas.name);
        ShowScreen(settingsCanvas);
    }

    public void LoadLocalLobby()
    {
        SceneManager.LoadScene("SCN_Lobby");
    }

    private void ShowScreen(GameObject targetCanvas)
    {
        if (targetCanvas == null)
        {
            Debug.LogError(
                "MenuController: falta asignar uno de los Canvas en el Inspector.",
                this
            );
            return;
        }

        if (mainMenuCanvas != null)
            mainMenuCanvas.SetActive(targetCanvas == mainMenuCanvas);

        if (gameModeCanvas != null)
            gameModeCanvas.SetActive(targetCanvas == gameModeCanvas);

        if (onlineMenuCanvas != null)
            onlineMenuCanvas.SetActive(targetCanvas == onlineMenuCanvas);

        if (settingsCanvas != null)
            settingsCanvas.SetActive(targetCanvas == settingsCanvas);

        currentCanvas = targetCanvas;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}



