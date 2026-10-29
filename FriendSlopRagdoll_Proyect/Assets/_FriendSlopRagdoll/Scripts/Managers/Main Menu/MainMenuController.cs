
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Menu Screens")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject gameModeCanvas;
    [SerializeField] private GameObject onlineMenuCanvas;

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

        if (currentCanvas != null)
        {
            currentCanvas.SetActive(false);
        }

        mainMenuCanvas.SetActive(targetCanvas == mainMenuCanvas);
        gameModeCanvas.SetActive(targetCanvas == gameModeCanvas);
        onlineMenuCanvas.SetActive(targetCanvas == onlineMenuCanvas);

        currentCanvas = targetCanvas;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}

