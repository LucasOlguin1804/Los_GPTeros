using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{
    int wave = 0;
    public int bossWave = 3;
    public int enemiesToNextWave = 5;
    int enemiesCount = 0;
    public float timeBtwSpawn = 1.5f;
    float timer = 0;

    [Header("Puntos de aparición")]
    public Transform topPoint;
    public Transform bottomPoint;

    [Header("Prefabs")]
    public List<GameObject> enemyPrefabs;
    public GameObject bossPrefab;

    [Header("UI")]
    public Text waveText;
    public Text gameOverText;

    public static Spawner instance;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 🔍 Verificaciones de seguridad
        if (waveText == null)
            Debug.LogError("❌ Falta asignar 'waveText' en el Spawner");
        if (topPoint == null || bottomPoint == null)
            Debug.LogError("❌ Faltan los puntos de spawn (topPoint/bottomPoint)");
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
            Debug.LogError("❌ No hay enemigos en la lista 'enemyPrefabs'");

        wave++;
        if (waveText != null)
            waveText.text = "Wave " + wave;

        StartCoroutine(SpawnWave());
    }

    public void EnemyKilled()
    {
        enemiesCount++;
        if (enemiesCount == enemiesToNextWave)
        {
            enemiesCount = 0;
            wave++;
            waveText.text = "Wave " + wave;

            if (wave == bossWave)
            {
                GameObject boss = Instantiate(bossPrefab, transform.position, Quaternion.identity);
                
                // 👁️ Asegurar que el boss se vea correctamente
                SpriteRenderer bossRenderer = boss.GetComponent<SpriteRenderer>();
                if (bossRenderer != null)
                    bossRenderer.sortingOrder = 1;
            }
            else
            {
                StartCoroutine(SpawnWave());
            }
        }
    }

    public void GameOver(bool playerWin)
    {
        if (playerWin)
        {
            gameOverText.text = "You win!";
            gameOverText.color = Color.green;
        }
        else
        {
            gameOverText.text = "Game Over!";
            gameOverText.color = Color.red;
        }

        gameOverText.gameObject.SetActive(true);
        StartCoroutine(ReloadScene());
    }

    IEnumerator ReloadScene()
    {
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene("Game");
    }

    IEnumerator SpawnWave()
    {
        for (int i = 0; i < enemiesToNextWave; i++)
        {
            float x = transform.position.x;
            float y = Random.Range(bottomPoint.position.y, topPoint.position.y);
            Vector2 position = new Vector2(x, y);

            GameObject enemy = Instantiate(
                enemyPrefabs[Random.Range(0, enemyPrefabs.Count)],
                position,
                Quaternion.identity
            );

            // 🧩 Asignar "Order in Layer = 1" automáticamente
            SpriteRenderer renderer = enemy.GetComponent<SpriteRenderer>();
            if (renderer != null)
                renderer.sortingOrder = 1;

            yield return new WaitForSeconds(timeBtwSpawn);
        }
    }
}
