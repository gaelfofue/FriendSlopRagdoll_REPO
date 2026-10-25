
using UnityEngine;

public class MinigameOption : MonoBehaviour
{
    [Header("Minigame")]
    [SerializeField] private string minigameName;
    [SerializeField] private string sceneName;

    public string MinigameName => minigameName;
    public string SceneName => sceneName;
}

