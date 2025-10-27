using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Configuración de la bala")]
    [Tooltip("Tiempo antes de destruir la bala automáticamente")]
    public float lifeTime = 2f;

    [Tooltip("Daño que inflige la bala al enemigo")]
    public int damage = 50; // puedes ajustarlo en el inspector

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("🎯 La bala tocó: " + other.name);

        // Evita destruirse si colisiona con el jugador
        if (other.CompareTag("Player")) return;

        // Si golpea a un enemigo
        if (other.CompareTag("Enemy"))
        {
            EnemySlowPowerful enemy = other.GetComponent<EnemySlowPowerful>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"💥 Bala impactó a {other.name} e hizo {damage} de daño.");
            }
        }

        Destroy(gameObject);
    }
}
