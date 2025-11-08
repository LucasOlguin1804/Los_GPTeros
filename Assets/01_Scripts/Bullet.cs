using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Configuración de la bala")]
    public float lifeTime = 2f;
    public int damage = 50; // valor por defecto, se puede sobrescribir desde PlayerShoot

    void Start()
{
    // Ignorar colisiones entre balas
    Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Bullet"), LayerMask.NameToLayer("Bullet"), true);
    Destroy(gameObject, lifeTime);

    // ⚠️ Validar que tenga el tag correcto
    if (!CompareTag("Bullet"))
    {
        Debug.LogWarning($"⚠️ {name} no tiene el tag 'Bullet'. Asigna el tag en el prefab para evitar errores.");
    }
}


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;
        if (other.CompareTag("Bullet")) return;

        if (other.CompareTag("Enemy"))
        {
            bool hitSomething = false;

            // 🧠 Comprueba varios tipos de enemigo posibles
            EnemySlowPowerful enemySlow = other.GetComponent<EnemySlowPowerful>();
            EnemyShooter enemyShooter = other.GetComponent<EnemyShooter>();
            EnemyFlying enemyFlying = other.GetComponent<EnemyFlying>();

            if (enemySlow != null)
            {
                enemySlow.TakeDamage(damage);
                hitSomething = true;
            }
            else if (enemyShooter != null)
            {
                enemyShooter.TakeDamage(damage);
                hitSomething = true;
            }
            else if (enemyFlying != null)
            {
                enemyFlying.TakeDamage(damage);
                hitSomething = true;
            }

            if (hitSomething)
                Debug.Log($"💥 Bala impactó a {other.name} e hizo {damage} de daño.");
        }

        Destroy(gameObject);
    }

    // 👇 Permite que PlayerShoot asigne el daño dinámicamente
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
}
