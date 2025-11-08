using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WaveManager : MonoBehaviour
{
    [Header("Configuración de oleadas")]
    public EnemySpawnerBase[] spawners;
    public int totalWaves = 3;
    public float waveDelay = 2f;

    [Header("UI")]
    public Text waveText;

    [Header("Nivel / Portal")]
    [Tooltip("Referencia automática al LevelManager que controla el portal de salida.")]
    public LevelManager levelManager;

    private int currentWave = 0;

    void Start()
    {
        if (spawners == null || spawners.Length == 0)
        {
            Debug.LogWarning("⚠️ No hay spawners asignados al WaveManager.");
            return;
        }

        // Buscar automáticamente el LevelManager si no fue asignado
        if (levelManager == null)
            levelManager = FindObjectOfType<LevelManager>();

        StartCoroutine(HandleWaves());
    }

    IEnumerator HandleWaves()
    {
        for (currentWave = 1; currentWave <= totalWaves; currentWave++)
        {
            // 🟡 Mostrar texto "WAVE X"
            if (waveText != null)
                StartCoroutine(ShowWaveText($"WAVE {currentWave}"));

            yield return new WaitForSeconds(1f);

            // 🟢 Spawnea una oleada de cada tipo de enemigo
            foreach (var spawner in spawners)
            {
                if (spawner != null)
                    spawner.SpawnWave(currentWave);
            }

            // 🔴 Esperar a que mueran TODOS los enemigos
            yield return new WaitUntil(() => !AnyEnemiesAlive());

            Debug.Log($"✅ Wave {currentWave} completada.");

            yield return new WaitForSeconds(waveDelay);
        }

        // 🏁 Cuando termina la última wave...
        if (levelManager != null && levelManager.exitSpawn != null)
        {
            levelManager.exitSpawn.SetActive(true);
            Debug.Log("🚪 Todas las waves completadas. Portal activado.");
        }
        else
        {
            // Si no hay LevelManager, mostramos el texto
            if (waveText != null)
                StartCoroutine(ShowWaveText("🏆 ALL WAVES CLEARED!"));
        }
    }

    private bool AnyEnemiesAlive()
    {
        // Busca si hay enemigos activos en la escena
        return GameObject.FindGameObjectsWithTag("Enemy").Length > 0;
    }

    private IEnumerator ShowWaveText(string text)
    {
        waveText.gameObject.SetActive(true);
        waveText.text = text;
        Color c = waveText.color;
        c.a = 1f;
        waveText.color = c;

        yield return new WaitForSeconds(2f);

        float fade = 0.5f;
        float elapsed = 0f;
        while (elapsed < fade)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, elapsed / fade);
            waveText.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        waveText.gameObject.SetActive(false);
    }
}
