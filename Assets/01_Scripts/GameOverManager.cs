using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("UI")]
    public GameObject gameOverUI;

    [Header("Configuración")]
    public int maxLives = 3;
    private int currentLives;
    private bool gameOverShown = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // ❗ Quita esta línea si prefieres que se destruya entre escenas
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        currentLives = maxLives;
        if (gameOverUI != null)
            gameOverUI.SetActive(false);
    }

    public void PlayerDied()
    {
        currentLives--;
        if (currentLives <= 0 && !gameOverShown)
            ShowGameOver();
    }

    void ShowGameOver()
    {
        Time.timeScale = 0f;
        gameOverShown = true;
        if (gameOverUI != null)
            gameOverUI.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        Destroy(gameObject); // ✅ evita duplicación del canvas
        SceneManager.LoadScene("Wave_1");
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;
        Destroy(gameObject);
        SceneManager.LoadScene("SampleScene");
    }
}
