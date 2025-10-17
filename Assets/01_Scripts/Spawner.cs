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
    public Transform topPoint;
    public Transform bottomPoint;
    public List<GameObject> enemyPrefabs;
    public GameObject bossPrefab;
    public Text waveText;
    public Text gameOverText;
    public static Spawner instance;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        wave++;
        waveText.text = "Wave " + wave;
        StartCoroutine(SpawnWave());
    }

    void Update()
    {
    }

    public void EnemyKilled()
    {
        enemiesCount++;
        if(enemiesCount == enemiesToNextWave)
        {
            enemiesCount = 0;
            wave++;
            waveText.text = "Wave " + wave;
            if (wave == bossWave)
            {
                Instantiate(bossPrefab, transform.position, Quaternion.Euler(0, 0, 90));
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
            Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Count)], position, Quaternion.Euler(0, 0, 90));
            yield return new WaitForSeconds(timeBtwSpawn);
        }
    }
}
