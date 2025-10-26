using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySlowPowerful : MonoBehaviour
{
    [Header("Atributos del enemigo")]
    public int maxHealth = 100;
    public int damage = 30;
    public float moveSpeed = 1.5f;
    public float patrolRange = 3f;
    public float attackCooldown = 1.5f;
    public float detectionRange = 5f;

    [Header("Efectos visuales")]
    public float attackFlashTime = 0.1f;
    public float recoilDuration = 0.2f;
    public float recoilForce = 2f;

    [Header("Detección de borde y pared")]
    public Transform groundCheck;
    public float groundCheckDistance = 0.5f;
    public LayerMask groundLayer;

    [Header("Salto entre plataformas")]
    public float jumpForce = 7f;
    public float maxJumpHeightDifference = 3f;
    public float jumpCooldown = 1.5f;

    private int currentHealth;
    private Vector3 startPosition;
    private bool movingRight = true;
    private float lastAttackTime;
    private float lastJumpTime;
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    // 👇 NUEVO: control de estado
    private bool isChasing = false;
    private float chaseLostTime;
    private float chaseCooldown = 1.5f; // tiempo antes de volver a patrullar

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        currentHealth = maxHealth;
        startPosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 🔹 Rango con margen de histéresis
        float chaseEnterRange = detectionRange;
        float chaseExitRange = detectionRange + 1f;

        // 🧠 Mantiene estado entre frames (evita temblores)
        if (isChasing)
        {
            if (distanceToPlayer > chaseExitRange)
            {
                chaseLostTime += Time.deltaTime;
                if (chaseLostTime >= chaseCooldown)
                {
                    isChasing = false;
                    chaseLostTime = 0;
                    startPosition = transform.position; // nuevo punto de patrulla
                }
            }
            else
            {
                chaseLostTime = 0; // sigue persiguiendo
            }
        }
        else
        {
            if (distanceToPlayer < chaseEnterRange)
            {
                isChasing = true;
            }
        }

        // 🔁 Ejecuta el comportamiento actual
        if (isChasing)
            ChasePlayer();
        else
            Patrol();
    }

    void Patrol()
    {
        float moveDir = movingRight ? 1 : -1;
        rb.velocity = new Vector2(moveDir * moveSpeed, rb.velocity.y);

        // 🔍 Detección del suelo frente al enemigo
        Vector2 checkPos = new Vector2(transform.position.x + (moveDir * 0.5f), transform.position.y - 0.1f);
        bool isGroundAhead = Physics2D.Raycast(checkPos, Vector2.down, groundCheckDistance, groundLayer);

        // 🔍 Detección de pared
        Vector2 wallCheckPos = new Vector2(transform.position.x + (moveDir * 0.3f), transform.position.y - 0.3f);
        bool isWallAhead = Physics2D.Raycast(wallCheckPos, Vector2.right * moveDir, 0.3f, groundLayer);

        Debug.DrawRay(checkPos, Vector2.down * groundCheckDistance, isGroundAhead ? Color.green : Color.red);
        Debug.DrawRay(wallCheckPos, Vector2.right * moveDir * 0.3f, isWallAhead ? Color.magenta : Color.cyan);

        if (!isGroundAhead || isWallAhead ||
            (movingRight && transform.position.x > startPosition.x + patrolRange) ||
            (!movingRight && transform.position.x < startPosition.x - patrolRange))
        {
            movingRight = !movingRight;
        }
    }

    void ChasePlayer()
    {
        if (player == null) return;

        float directionX = Mathf.Sign(player.position.x - transform.position.x);
        rb.velocity = new Vector2(directionX * moveSpeed, rb.velocity.y);

        if (directionX > 0 && !movingRight)
            movingRight = true;
        else if (directionX < 0 && movingRight)
            movingRight = false;

        float heightDifference = player.position.y - transform.position.y;
        if (heightDifference > 1f && heightDifference < maxJumpHeightDifference && Time.time > lastJumpTime + jumpCooldown)
        {
            JumpTowardsPlayer();
        }

        if (Vector2.Distance(transform.position, player.position) < 1.2f)
            TryAttack();
    }

    void JumpTowardsPlayer()
    {
        if (rb == null || player == null) return;
        lastJumpTime = Time.time;

        float directionX = Mathf.Sign(player.position.x - transform.position.x);
        rb.velocity = new Vector2(rb.velocity.x, 0);

        Vector2 jumpVector = new Vector2(directionX * moveSpeed * 0.8f, jumpForce);
        rb.AddForce(jumpVector, ForceMode2D.Impulse);
        Debug.Log($"🦘 {gameObject.name} saltó hacia el jugador.");
    }

    void TryAttack()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            Debug.Log($"💥 {gameObject.name} atacó al jugador causando {damage} de daño.");

            if (player != null)
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null)
                    ph.TakeDamage(damage, transform);
            }

            StartCoroutine(AttackFeedback());
        }
    }

    private IEnumerator AttackFeedback()
    {
        if (sr == null || player == null) yield break;
        if (!player.gameObject.activeInHierarchy) yield break;

        sr.color = Color.yellow;

        if (rb != null)
        {
            Vector2 recoilDir = (transform.position - player.position).normalized;
            rb.AddForce(recoilDir * recoilForce, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(attackFlashTime);

        if (this == null || sr == null) yield break;

        sr.color = Color.white;
        yield return new WaitForSeconds(recoilDuration);

        if (rb != null)
            rb.velocity = Vector2.zero;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"🩸 {gameObject.name} recibió {amount} de daño. Vida restante: {currentHealth}");

        if (sr != null)
            StartCoroutine(FlashRed());

        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator FlashRed()
    {
        if (sr == null) yield break;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        sr.color = Color.white;
    }

    private void Die()
    {
        Debug.Log($"💀 {gameObject.name} ha muerto.");
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        LevelManager levelManager = FindObjectOfType<LevelManager>();
        if (levelManager != null)
            levelManager.EnemyDefeated();

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        }
    }
}
