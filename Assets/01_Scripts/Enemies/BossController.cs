using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossController : MonoBehaviour
{
    [Header("Vida")]
    public BossHealthBar healthBar;
    public int maxHealth = 250;
    private int currentHealth;
    private bool phaseTwo = false;

    [Header("Movimiento (2D)")]
    public float moveSpeed = 2f;
    public float floatAmplitude = 0.5f;
    public float verticalFloatSpeed = 2f;
    private Vector2 startPosition;
    private bool canMove = false;

    [Header("Límites del movimiento")]
    public float leftLimit = -8f;
    public float rightLimit = 8f;
    public float topLimit = 4f;
    public float bottomLimit = -3f;

    [Header("Detección del jugador")]
    public float detectionRange = 8f;
    private bool playerDetected = false;

    [Header("Ataques")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 1.5f;
    public float projectileSpeed = 6f;
    public float meleeRange = 1.5f;
    public int meleeDamage = 2;

    [Header("Componentes")]
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;

    private bool isAttacking = false;
    private float nextFireTime = 0f;
    private bool movingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        currentHealth = maxHealth;
        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);

        startPosition = transform.position;
        rb.gravityScale = 0;

        StartCoroutine(EntranceAnimation());
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Detectar jugador
        if (!playerDetected && distanceToPlayer <= detectionRange)
        {
            playerDetected = true;
            Debug.Log("👁️ Boss detectó al jugador y comienza la persecución.");
        }

        // Movimiento
        if (canMove)
        {
            if (playerDetected)
                FollowPlayer();
            else
                PatrolMovement();
        }

        ClampPosition();

        // Cambio de fase
        if (!phaseTwo && currentHealth <= maxHealth / 2)
        {
            phaseTwo = true;
            StartCoroutine(TransformToPhaseTwo());
        }

        // Ataques
        if (playerDetected && !isAttacking)
        {
            if (distanceToPlayer < meleeRange)
                StartCoroutine(MeleeAttack());
            else if (Time.time >= nextFireTime)
                StartCoroutine(RangedAttack());
        }
    }

    // 🔁 Movimiento de patrulla entre límites
    void PatrolMovement()
    {
        float y = startPosition.y + Mathf.Sin(Time.time * verticalFloatSpeed) * floatAmplitude;
        Vector2 newPos = transform.position;

        // Movimiento horizontal
        if (movingRight)
        {
            newPos.x += moveSpeed * Time.deltaTime;
            if (newPos.x >= rightLimit)
                movingRight = false;
        }
        else
        {
            newPos.x -= moveSpeed * Time.deltaTime;
            if (newPos.x <= leftLimit)
                movingRight = true;
        }

        transform.position = new Vector2(newPos.x, y);
    }

    // 👁️ Seguir al jugador
    void FollowPlayer()
    {
        float targetX = Mathf.MoveTowards(transform.position.x, player.position.x, moveSpeed * Time.deltaTime);
        float y = startPosition.y + Mathf.Sin(Time.time * verticalFloatSpeed) * floatAmplitude;
        transform.position = new Vector2(targetX, y);

        // Girar hacia el jugador
        sr.flipX = player.position.x < transform.position.x;
    }

    // 🔒 Mantener dentro de los límites
    void ClampPosition()
    {
        float clampedX = Mathf.Clamp(transform.position.x, leftLimit, rightLimit);
        float clampedY = Mathf.Clamp(transform.position.y, bottomLimit, topLimit);
        transform.position = new Vector2(clampedX, clampedY);
    }

    // 🎬 Entrada desde arriba
    IEnumerator EntranceAnimation()
    {
        canMove = false;
        Vector2 startAbove = startPosition + Vector2.up * 6f;
        transform.position = startAbove;
        sr.color = new Color(1, 1, 1, 0);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 2f;
            transform.position = Vector2.Lerp(startAbove, startPosition, t);
            sr.color = new Color(1, 1, 1, t);
            yield return null;
        }

        sr.color = Color.white;
        canMove = true;
    }

    // 🔫 Ataques a distancia
    IEnumerator RangedAttack()
    {
        isAttacking = true;

        if (!phaseTwo)
        {
            FireAtPlayer();
        }
        else
        {
            // Fase 2: ráfaga triple
            for (int i = 0; i < 3; i++)
            {
                FireAtPlayer();
                yield return new WaitForSeconds(0.25f);
            }
        }

        nextFireTime = Time.time + fireRate;
        isAttacking = false;
    }

    // 👇 Disparo hacia el jugador
    void FireAtPlayer()
    {
        if (projectilePrefab == null || firePoint == null || player == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Vector2 direction = (player.position - firePoint.position).normalized;

        Rigidbody2D rbProj = proj.GetComponent<Rigidbody2D>();
        if (rbProj != null)
            rbProj.velocity = direction * projectileSpeed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // 🦾 Ataque cuerpo a cuerpo
    IEnumerator MeleeAttack()
    {
        isAttacking = true;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.3f);

        if (Vector2.Distance(transform.position, player.position) <= meleeRange + 0.2f)
            player.GetComponent<PlayerController>().TakeDamage(meleeDamage);

        sr.color = Color.white;
        isAttacking = false;
    }

    // 💀 Transformación de fase
    IEnumerator TransformToPhaseTwo()
    {
        Debug.Log("💀 El Arquitecto entra en FASE 2 ⚡");
        sr.color = Color.magenta;
        fireRate *= 0.6f;
        moveSpeed += 1.5f;
        meleeDamage += 2;
        yield return new WaitForSeconds(1f);
        sr.color = Color.white;
    }

    // 🩸 Daño recibido
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        StartCoroutine(DamageFlash());

        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    IEnumerator DamageFlash()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }

    void Die()
    {
        Debug.Log("🔥 Boss derrotado.");
        Destroy(gameObject);
        SceneManager.LoadScene("EndingScene");
    }

    // 🔵 Gizmos
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            new Vector3((leftLimit + rightLimit) / 2, (bottomLimit + topLimit) / 2, 0),
            new Vector3(rightLimit - leftLimit, topLimit - bottomLimit, 0)
        );
    }
}
