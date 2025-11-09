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
    public int maxHealth = 50;
    private int currentHealth;
    public int contactDamage = 10;
    public float attackRange = 8f;
    public float fireRate = 2f;
    private float nextFireTime = 0f;
    private float nextMeleeTime = 0f;

    [Header("Proyectil")]
<<<<<<< HEAD
    public GameObject projectilePrefab; // Prefab con el script EBFlying
=======
    public GameObject projectilePrefab;
>>>>>>> e0c2d32b0053e9c6544b6af5afa6f349f918fe5c
    public Transform firePoint;
    public float projectileSpeed = 5f;
    public int projectileDamage = 8;

    [Header("Efectos de daño")]
    public float flashTime = 0.15f;
    public float recoilForce = 2f;
    public float recoilDuration = 0.2f;

    [Header("Muerte")]
    public GameObject deathEffect;
    public AudioClip deathSound;
    private AudioSource audioSource;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Coroutine recoilCR;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        movingLeft = startMovingLeft;
        currentHealth = maxHealth;

        // Audio setup
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        Debug.Log($"🛩️ {gameObject.name} listo. Vida: {currentHealth}");
    }

    void Update()
    {
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

    // 🌊 Movimiento senoidal de vuelo
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

    // ⚔️ Ataque cuerpo a cuerpo
    void ContactAttack()
    {
        if (player == null) return;
        if (Time.time < nextMeleeTime) return;

        nextMeleeTime = Time.time + fireRate;
<<<<<<< HEAD
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
            pc.TakeDamage(contactDamage);
=======
        player.GetComponent<PlayerController>().TakeDamage(contactDamage);
        Debug.Log($"💥 {gameObject.name} dañó al jugador ({contactDamage})");
>>>>>>> e0c2d32b0053e9c6544b6af5afa6f349f918fe5c
    }

    // 🔫 Disparo hacia el jugador (usa EBFlying)
    void FireAtPlayer()
    {
        nextFireTime = Time.time + fireRate;

        if (projectilePrefab == null || firePoint == null || player == null) return;

        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

<<<<<<< HEAD
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

        Debug.LogWarning("⚠️ El prefab del proyectil no tiene el script EBFlying asignado.");
=======
        // Si usa BossBulletEnemy (controlador con dirección)
        BossBulletEnemy bulletScript = bullet.GetComponent<BossBulletEnemy>();
        if (bulletScript != null)
        {
            Vector2 dir = (player.position - firePoint.position).normalized;
            bulletScript.SetDirection(dir);
            bulletScript.damage = 5;
            return;
        }

        // Fallback: si es una bala simple
        Vector2 direction = (player.position - firePoint.position).normalized;
        Rigidbody2D rbBullet = bullet.GetComponent<Rigidbody2D>();
        if (rbBullet != null)
            rbBullet.velocity = direction * projectileSpeed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
>>>>>>> e0c2d32b0053e9c6544b6af5afa6f349f918fe5c
    }

    // 🩸 Recibir daño
    public void TakeDamage(int amount)
    {
<<<<<<< HEAD
        health -= dmg;
        if (health <= 0)
            Die();
=======
        currentHealth -= amount;
        Debug.Log($"🩸 {gameObject.name} recibió {amount} de daño. Vida restante: {currentHealth}");

        if (recoilCR != null) StopCoroutine(recoilCR);
        recoilCR = StartCoroutine(DamageFeedback());

        if (currentHealth <= 0)
        {
            Die();
        }
>>>>>>> e0c2d32b0053e9c6544b6af5afa6f349f918fe5c
    }

    // 💥 Feedback de daño con flash + retroceso
    private IEnumerator DamageFeedback()
{
    if (sr != null) sr.color = Color.red;

    if (rb != null)
    {
        Vector2 recoilDir = (player != null)
            ? ((Vector2)(transform.position - player.position)).normalized
            : Vector2.up;

        rb.velocity = Vector2.zero;
        rb.AddForce(recoilDir * recoilForce, ForceMode2D.Impulse);
    }

    yield return new WaitForSeconds(flashTime);

    if (sr != null) sr.color = Color.white;
    yield return new WaitForSeconds(recoilDuration);
    recoilCR = null;
}


    // 💀 Muerte del enemigo
    void Die()
    {
        Debug.Log($"💀 {gameObject.name} ha muerto.");

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

<<<<<<< HEAD
        // ❌ elimina Spawner y LevelManager.EnemyDefeated()
        Destroy(gameObject);
    }



=======
        if (audioSource != null && deathSound != null)
            audioSource.PlayOneShot(deathSound);

        if (Spawner.instance != null)
            Spawner.instance.EnemyKilled();

        Destroy(gameObject);
    }

    // 🎯 Colisiones con balas
>>>>>>> e0c2d32b0053e9c6544b6af5afa6f349f918fe5c
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage);
            }

            Destroy(other.gameObject);
        }
    }
}
