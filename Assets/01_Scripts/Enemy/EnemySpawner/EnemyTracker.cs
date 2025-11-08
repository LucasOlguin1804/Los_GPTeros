using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTracker : MonoBehaviour
{
    [HideInInspector] public EnemySpawnerBase parentSpawner;

    void OnDestroy()
    {
        if (parentSpawner != null && Application.isPlaying)
            parentSpawner.OnEnemyKilled();
    }
}


