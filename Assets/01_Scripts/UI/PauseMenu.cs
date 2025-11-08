using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    // Bandera global de pausa y ventana de supresión de input
    public static bool IsPaused { get; private set; } = false;
    public static float SuppressInputUntilUnscaled = 0f; // usa Time.unscaledTime

    [Header("UI")]
    public GameObject pauseUI;

    private bool isPaused = false;

    void Start()
    {
        if (pauseUI) pauseUI.SetActive(false);
        Time.timeScale = 1.0f;
        isPaused = false;
        IsPaused = false;

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        if (pauseUI) pauseUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        IsPaused = true;

        // Corta posibles corrutinas de disparo en curso (seguridad)
        var shooter = FindObjectOfType<PlayerShoot>(includeInactive: true);
        if (shooter) shooter.StopAllCoroutines();

        // Consume el click/teclas actuales para que no “goteen”
        Input.ResetInputAxes();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        if (pauseUI) pauseUI.SetActive(false);

        // Ventana de 1s (tiempo no escalado) para ignorar clicks al reanudar
        SuppressInputUntilUnscaled = Time.unscaledTime + 1f;

        // Consume el click que cerró el menú
        Input.ResetInputAxes();

        Time.timeScale = 1f;
        isPaused = false;
        IsPaused = false;
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        IsPaused = false;
        SceneManager.LoadScene("SampleScene");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Juego cerrado");
    }

    void OnDisable()
    {
        if (isPaused)
        {
            Time.timeScale = 1f;
            isPaused = false;
            IsPaused = false;
        }
    }
}
