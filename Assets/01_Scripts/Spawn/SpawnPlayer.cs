using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnPlayer : MonoBehaviour
{
    public static SpawnPlayer instance;

    [Header("Player Settings")]
    public GameObject playerPrefab;
    private GameObject currentPlayer;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            // 🔒 Nunca seas hijo de nada de la escena
            transform.SetParent(null);

            // 🔧 Por si alguien dejó hijos por error en el editor, suéltalos
            transform.DetachChildren();

            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 👀 Si en runtime alguien intenta colgar algo del GameManager, lo soltamos
    void OnTransformChildrenChanged()
    {
        foreach (Transform child in transform)
        {
            Debug.Log($"[SpawnPlayer] Se colgó un hijo inesperado: {child.name}. Lo descolgamos para evitar persistencia.");
            child.SetParent(null);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SpawnPlayerAtPoint();
    }

    private void SpawnPlayerAtPoint()
    {
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");
        if (spawnPoint == null)
        {
            Debug.LogWarning("⚠️ No se encontró un objeto con tag 'SpawnPoint' en esta escena.");
            return;
        }

        currentPlayer = GameObject.FindGameObjectWithTag("Player");

        if (currentPlayer == null)
            currentPlayer = Instantiate(playerPrefab, spawnPoint.transform.position, Quaternion.identity);
        else
            currentPlayer.transform.position = spawnPoint.transform.position;

        // Defensa extra: el Player NUNCA debe ser hijo del GameManager
        if (currentPlayer.transform.parent == transform)
            currentPlayer.transform.SetParent(null);
    }
}
