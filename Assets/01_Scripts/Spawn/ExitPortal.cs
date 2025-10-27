using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitPortal : MonoBehaviour
{
    private LevelManager levelManager;

    void Start()
    {
        levelManager = FindObjectOfType<LevelManager>();

        if (levelManager == null)
            Debug.LogWarning("ExitPortal: No se encontr� LevelManager en la escena.");

    }

    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player") && levelManager != null)

        {
            levelManager.UseExit();
        }
    }
}
