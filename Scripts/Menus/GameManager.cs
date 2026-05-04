// Manages sceen transitions and other menu stuff

using UnityEngine;
using UnityEngine.SceneManagement;

// Persistent singleton that manages scene transitions
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public const string MAIN_MENU_SCENE = "MainMenu";
    public const string BATTLE_SCENE = "Battle";

    void Awake()
    {
        // Singleton pattern is persist across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(BATTLE_SCENE);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(MAIN_MENU_SCENE);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}