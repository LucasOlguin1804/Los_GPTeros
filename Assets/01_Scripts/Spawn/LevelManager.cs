using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Configuración del nivel")]
    [Tooltip("Cantidad de enemigos a eliminar antes de activar la salida. Si es 0, la salida estará activa desde el inicio.")]
    public int requiredEnemies = 5;

    [Tooltip("Nombre exacto del siguiente nivel (escena)")]
    public string nextLevelName = "Level3";

    [Header("Spawns")]
    public GameObject entrySpawn; // Punto de entrada del jugador
    public GameObject exitSpawn;  // Portal de salida (desactivado al inicio)

    private int defeatedEnemies = 0;

    // NUEVO: referencia opcional al fader si existe en la escena
    private SceneTransition sceneTransition;

    void Start()
    {
        // cachear SceneTransition si está presente
        sceneTransition = FindObjectOfType<SceneTransition>();

        // Si hay portal de salida, decidir si empieza activo o no
        if (exitSpawn != null)
        {
            bool shouldBeActive = (requiredEnemies <= 0);
            exitSpawn.SetActive(shouldBeActive);
        }
    }

    // Llamar cuando un enemigo muera
    public void EnemyDefeated()
    {
        defeatedEnemies++;

        if (defeatedEnemies >= requiredEnemies && exitSpawn != null)
        {
            exitSpawn.SetActive(true);
            Debug.Log("✅ Todos los enemigos eliminados, la salida está activada.");
        }
    }

    // Llamar cuando el jugador toque el portal
    public void UseExit()
    {
        if (exitSpawn != null && exitSpawn.activeSelf)
        {
            Debug.Log($"🚪 Cargando siguiente nivel: {nextLevelName}");

            // Si hay fader en la escena, usar transición; si no, cargar directo (comportamiento actual)
            if (sceneTransition != null)
                sceneTransition.LoadScene(nextLevelName);
            else
                SceneManager.LoadScene(nextLevelName);
        }
        else
        {
            Debug.Log("❌ No puedes salir todavía. Falta eliminar enemigos.");
        }
    }
}
