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

    public void LoadGame(string name)
    {
        SceneManager.LoadScene("D" + name);
    }

    public void LoadMainMenu()
    {
        // 0 = Main menu
        SceneManager.LoadScene(0);
    }
}