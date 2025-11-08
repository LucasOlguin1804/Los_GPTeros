using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Configuración del nivel")]
    [Tooltip("Nombre exacto del siguiente nivel (escena)")]
    public string nextLevelName = "Level3";

    [Header("Spawns")]
    public GameObject entrySpawn; // Punto de entrada del jugador
    public GameObject exitSpawn;  // Portal de salida (activado por WaveManager)

    // Referencia opcional al fader si existe
    private SceneTransition sceneTransition;

    void Start()
    {
        // Buscar transición de escena si existe
        sceneTransition = FindObjectOfType<SceneTransition>();

        // 🚪 El portal siempre empieza apagado
        if (exitSpawn != null)
            exitSpawn.SetActive(false);
    }

    // 🚪 Cuando el jugador toca el portal
    public void UseExit()
    {
        if (exitSpawn != null && exitSpawn.activeSelf)
        {
            Debug.Log($"🚪 Cargando siguiente nivel: {nextLevelName}");
            if (sceneTransition != null)
                sceneTransition.LoadScene(nextLevelName);
            else
                SceneManager.LoadScene(nextLevelName);
        }
        else
        {
            Debug.Log("❌ No puedes salir todavía. Falta eliminar todas las oleadas.");
        }
    }
}
