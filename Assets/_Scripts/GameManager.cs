using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadGame()
    {
        // 1 = Level 1
        SceneManager.LoadScene(1);
    }

    public void LoadGame(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void LoadMainMenu()
    {
        // 0 = Main menu
        SceneManager.LoadScene(0);
    }
}