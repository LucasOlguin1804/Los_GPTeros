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
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 5f;

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

    void ContactAttack()
    {
        if (player == null) return;
        if (Time.time < nextMeleeTime) return;

        nextMeleeTime = Time.time + fireRate;
        player.GetComponent<PlayerController>().TakeDamage(contactDamage);
    }

    void FireAtPlayer()
    {
        nextFireTime = Time.time + fireRate;

        if (projectilePrefab == null || firePoint == null || player == null) return;

        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Vector2 dir = (player.position - firePoint.position).normalized;
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.velocity = dir * projectileSpeed;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        if (health <= 0) Die();
    }

    void Die()
    {
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        if (Spawner.instance != null)
            Spawner.instance.EnemyKilled();

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
