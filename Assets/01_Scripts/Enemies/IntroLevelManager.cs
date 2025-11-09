using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroLevelManager : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerController player;
    public CanvasGroup transitionPanel;   // Panel negro o de fondo
    public Text transitionText;           // Texto UI normal
    public string nextSceneName = "Level_01";

    [Header("Transición")]
    [Tooltip("Tiempo que espera tras la muerte antes de iniciar el fade.")]
    public float delayBeforeTransition = 2f;

    [Tooltip("Duración del fundido a negro.")]
    public float fadeDuration = 2.5f;

    [Tooltip("Tiempo que el panel permanece totalmente visible antes de cambiar de escena.")]
    public float holdDuration = 2.5f;

    [Header("Audio (opcional)")]
    [Tooltip("Clip de sonido o música para reproducir durante la transición.")]
    public AudioClip transitionSound;
    public float soundVolume = 0.8f;

    private bool transitionStarted = false;
    private AudioSource audioSource;

    void Start()
    {
        if (transitionPanel != null)
        {
            transitionPanel.alpha = 0f;
            transitionPanel.gameObject.SetActive(false);
        }

        // Crear un AudioSource si hay un clip asignado
        if (transitionSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = transitionSound;
            audioSource.volume = soundVolume;
            audioSource.loop = false;
        }
    }

    void Update()
    {
        // Detectar muerte del jugador
        if (!transitionStarted && player != null && player.currentHealth <= 0)
        {
            transitionStarted = true;
            StartCoroutine(PlayIntroTransition());
        }
    }

    private IEnumerator PlayIntroTransition()
    {
        // Espera breve antes del fade (por animaciones de muerte, etc.)
        yield return new WaitForSeconds(delayBeforeTransition);

        if (transitionPanel != null)
        {
            transitionPanel.gameObject.SetActive(true);

            // Texto narrativo
            if (transitionText != null)
                transitionText.text = "Dias atras...El enemigo recordará el verdadero peligro pero también la valentía, de enfrentar una epidemia que solo tendra que enfrentar";

            // Reproduce sonido (opcional)
            if (audioSource != null)
                audioSource.Play();

            // Fade in progresivo del panel
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                transitionPanel.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
                yield return null;
            }

            // Mantiene el texto visible antes de cambiar de escena
            yield return new WaitForSeconds(holdDuration);
        }

        // Cambia a la siguiente escena
        SceneManager.LoadScene(nextSceneName);
    }
}
