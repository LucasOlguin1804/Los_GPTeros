using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 2f;

    void Start()
    {
        Destroy(gameObject, lifeTime); // destrucción automática por seguridad
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Evita destruirse si colisiona con el mismo jugador
        if (other.CompareTag("Player")) return;

        // Destruye la bala al tocar cualquier otra cosa
        Destroy(gameObject);
    }
}

