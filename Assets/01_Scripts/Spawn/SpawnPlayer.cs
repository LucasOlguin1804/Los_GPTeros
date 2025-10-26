using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnPlayer : MonoBehaviour
{
    public static SpawnPlayer instance;

    [Header("Player Settings")]
    public GameObject playerPrefab;   // Prefab del jugador
    private GameObject currentPlayer; // Referencia al jugador actual

    private void Awake()
    {
        // Singleton: solo una instancia del GameManager
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SpawnPlayerAtPoint();
    }

    private void SpawnPlayerAtPoint()
    {
        // Buscar SpawnPoint en la escena
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");
        if (spawnPoint == null)
        {
            Debug.LogWarning("⚠️ No se encontró un objeto con tag 'SpawnPoint' en esta escena.");
            return;
        }

        // Verificar si ya hay un jugador en escena
        currentPlayer = GameObject.FindGameObjectWithTag("Player");

        if (currentPlayer == null)
        {
            // Si no existe, instanciar uno nuevo
            currentPlayer = Instantiate(playerPrefab, spawnPoint.transform.position, Quaternion.identity);
        }
        else
        {
            // Si ya existe, simplemente moverlo al punto de spawn
            currentPlayer.transform.position = spawnPoint.transform.position;
        }
    }
}
