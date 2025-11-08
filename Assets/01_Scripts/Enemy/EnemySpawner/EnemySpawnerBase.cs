using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemySpawnerBase : MonoBehaviour
{
    [Header("Configuración de oleadas")]
    public GameObject enemyPrefab;
    public int enemiesPerWave = 2;
    public float spawnInterval = 1.5f;

    [Header("Rango de aparición aleatorio")]
    public float spawnRangeX = 5f;
    public float spawnRangeY = 2f;

    [Header("Efectos visuales")]
    public float appearDuration = 0.5f;
    public Vector3 appearScale = new Vector3(1.3f, 1.3f, 1f);

    [Header("Referencia Global")]
    public WaveManager waveManager; // 👈 El WaveManager que coordina todo

    [Header("Debug")]
    public bool showDebugArea = true;

    private int enemiesAlive = 0;

    // 🔹 Este método lo llama el WaveManager para generar una wave en este spawner
    public int SpawnWave(int waveNumber)
    {
        StartCoroutine(SpawnWaveRoutine(waveNumber));
        return enemiesPerWave;
    }

    private IEnumerator SpawnWaveRoutine(int waveNumber)
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // 🔹 Instancia un enemigo y lo registra con el WaveManager
    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        Vector3 spawnPos = Vector3.zero;
        bool foundSpot = false;
        int attempts = 0;
        int maxAttempts = 10;

        while (!foundSpot && attempts < maxAttempts)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-spawnRangeX, spawnRangeX),
                Random.Range(-spawnRangeY, spawnRangeY),
                0f
            );

            spawnPos = transform.position + randomOffset;

            Collider2D hit = Physics2D.OverlapCircle(spawnPos, 0.4f, LayerMask.GetMask("Ground", "Platform", "Obstacles"));
            if (hit == null)
                foundSpot = true;
            else
                spawnPos.y = hit.bounds.max.y + 0.5f;

            attempts++;
        }

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        newEnemy.transform.position = new Vector3(newEnemy.transform.position.x, newEnemy.transform.position.y, 0f);
        newEnemy.SetActive(true);

        Rigidbody2D rb = newEnemy.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = true;

        StartCoroutine(AppearanceEffect(newEnemy));
        Debug.Log($"👾 Spawned {enemyPrefab.name} en {spawnPos} después de {attempts} intentos.");
    }

    // 🔹 Calcula una posición libre para spawnear
    Vector3 GetValidSpawnPosition()
    {
        Vector3 spawnPos = transform.position;
        bool foundSpot = false;
        int attempts = 0;
        int maxAttempts = 10;

        while (!foundSpot && attempts < maxAttempts)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-spawnRangeX, spawnRangeX),
                Random.Range(-spawnRangeY, spawnRangeY),
                0f
            );

            spawnPos = transform.position + randomOffset;

            Collider2D hit = Physics2D.OverlapCircle(spawnPos, 0.4f, LayerMask.GetMask("Ground", "Platform", "Obstacles"));
            if (hit == null)
                foundSpot = true;
            else
                spawnPos.y = hit.bounds.max.y + 0.5f;

            attempts++;
        }

        return spawnPos;
    }

    // 🔹 Efecto visual al aparecer el enemigo
    private IEnumerator AppearanceEffect(GameObject enemy)
    {
        SpriteRenderer sr = enemy.GetComponentInChildren<SpriteRenderer>();
        if (sr == null) yield break;

        Color c = sr.color;
        c.a = 0f;
        sr.color = c;

        Vector3 originalScale = enemy.transform.localScale;
        enemy.transform.localScale = appearScale;

        float elapsed = 0f;
        while (elapsed < appearDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / appearDuration;

            sr.color = new Color(c.r, c.g, c.b, Mathf.Lerp(0f, 1f, t));
            enemy.transform.localScale = Vector3.Lerp(appearScale, originalScale, t);

            yield return null;
        }

        sr.color = new Color(c.r, c.g, c.b, 1f);
        enemy.transform.localScale = originalScale;
    }

    // 🔹 Llamado automáticamente por EnemyTracker cuando el enemigo muere
    public void OnEnemyKilled()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        Debug.Log($"☠️ Enemigo eliminado. Restantes: {enemiesAlive}");
    }

    // 🔹 Área visual del rango de spawn
    void OnDrawGizmosSelected()
    {
        if (!showDebugArea) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawCube(transform.position, new Vector3(spawnRangeX * 2f, spawnRangeY * 2f, 0f));

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnRangeX * 2f, spawnRangeY * 2f, 0f));
    }
}
