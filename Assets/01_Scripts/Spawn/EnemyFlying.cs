using System.Collections;
using UnityEngine;

public class EnemyFlying : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 3f;
    public float horizontalRange = 8f;
    public float verticalAmplitude = 1.5f;
    public float verticalSpeed = 2f;
    public bool startMovingLeft = true;

    private Vector2 startPos;
    private Transform player;
    private bool movingLeft;

    [Header("Combate")]
    public int health = 30;
    public int contactDamage = 10;
    public float attackRange = 8f;
    public float fireRate = 2f;
    private float nextFireTime = 0f;
    private float nextMeleeTime = 0f;

    [Header("Proyectil")]
    public GameObject projectilePrefab; // Prefab con el script EBFlying
    public Transform firePoint;
    public float projectileSpeed = 5f;
    public int projectileDamage = 8;

    [Header("Efectos")]
    public GameObject deathEffect;

    void Start()
    {
        startPos = transform.position;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        movingLeft = startMovingLeft;
    }

    void Update()
    {
        if (PauseMenu.IsPaused) return;  // ← añadido

        MovePattern();

        if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);

            if (dist < 1.5f)
                ContactAttack();
            else if (dist <= attackRange && Time.time >= nextFireTime)
                FireAtPlayer();
        }
    }


    // 🕹 Movimiento tipo vuelo senoidal
    void MovePattern()
    {
        float y = startPos.y + Mathf.Sin(Time.time * verticalSpeed) * verticalAmplitude;
        Vector3 pos = transform.position;

        if (movingLeft)
        {
            pos.x -= moveSpeed * Time.deltaTime;
            if (pos.x < -horizontalRange) movingLeft = false;
        }
        else
        {
            pos.x += moveSpeed * Time.deltaTime;
            if (pos.x > horizontalRange) movingLeft = true;
        }

        transform.position = new Vector2(pos.x, y);
    }

    // ⚔ Daño por contacto cuerpo a cuerpo
    void ContactAttack()
    {
        if (player == null) return;
        if (Time.time < nextMeleeTime) return;

        nextMeleeTime = Time.time + fireRate;
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
            pc.TakeDamage(contactDamage);
    }

    // 🔫 Disparo hacia el jugador (usa EBFlying)
    void FireAtPlayer()
    {
        nextFireTime = Time.time + fireRate;

        if (projectilePrefab == null || firePoint == null || player == null) return;

        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // Usa el script EBFlying
        EBFlying eb = bullet.GetComponent<EBFlying>();
        if (eb != null)
        {
            Vector2 dir = (player.position - firePoint.position).normalized;
            eb.direction = dir;
            eb.damage = projectileDamage;
            eb.speed = projectileSpeed;
            return;
        }

        Debug.LogWarning("⚠ El prefab del proyectil no tiene el script EBFlying asignado.");
    }

    // 💥 Recibir daño
    public void TakeDamage(int dmg)
    {
        health -= dmg;
        if (health <= 0)
            Die();
    }

    void Die()
    {
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        // ❌ elimina Spawner y LevelManager.EnemyDefeated()
        Destroy(gameObject);
    }



    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            TakeDamage(20);
            Destroy(other.gameObject);
        }
    }
}