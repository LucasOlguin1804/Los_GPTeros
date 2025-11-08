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

    [Header("Detección (pies / pared / cabeza)")]
    public Transform groundCheck;               // pies
    public float groundCheckDistance = 0.5f;
    public Transform headCheck;                 // cabeza / pecho
    public float headCheckDistance = 0.3f;
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

    // Estado
    private bool isChasing = false;
    private float chaseLostTime;
    private float chaseCooldown = 1.5f; // tiempo antes de volver a patrullar
    private float turnCooldown = 0.25f; // ⏱ tiempo mínimo entre giros
    private float lastTurnTime = 0f;


    // 🔒 Anti-temblor
    private bool lastPlayerActive = true;
    private Coroutine attackCR;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        // Fricción 0 para resbalar (como tu Player)
        var mat = new PhysicsMaterial2D { friction = 0f, bounciness = 0f };
        rb.sharedMaterial = mat;
        rb.freezeRotation = true;

        currentHealth = maxHealth;
        startPosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        bool playerActive = player != null && player.gameObject.activeInHierarchy;

        // ⛔ Si el player desaparece (respawn), hace un reset limpio UNA sola vez
        if (!playerActive)
        {
            if (lastPlayerActive) // transición de activo -> inactivo
                ResetAfterPlayerHidden();

            Patrol();
            lastPlayerActive = false;
            return;
        }

        // (re)cacheo si fuera null
        if (player == null)
        {
            var pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
            Patrol();
            lastPlayerActive = player != null && player.gameObject.activeInHierarchy;
            return;
        }

        lastPlayerActive = true;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Histéresis
        float chaseEnterRange = detectionRange;
        float chaseExitRange = detectionRange + 1f;

        if (isChasing)
        {
            if (distanceToPlayer > chaseExitRange)
            {
                chaseLostTime += Time.deltaTime;
                if (chaseLostTime >= chaseCooldown)
                {
                    isChasing = false;
                    chaseLostTime = 0f;
                    startPosition = transform.position; // nuevo centro de patrulla
                }
            }
            else chaseLostTime = 0f;
        }
        else
        {
            if (distanceToPlayer < chaseEnterRange) isChasing = true;
        }

        if (isChasing) ChasePlayer(); else Patrol();
    }

    void Patrol()
    {
        float moveDir = movingRight ? 1f : -1f;
        rb.velocity = new Vector2(moveDir * moveSpeed, rb.velocity.y);

        // --- Detectores de suelo, pared y cabeza ---
        Vector2 checkPos = groundCheck ? (Vector2)groundCheck.position :
            new Vector2(transform.position.x + moveDir * 0.5f, transform.position.y - 0.1f);

        bool groundFar = Physics2D.Raycast(checkPos, Vector2.down, groundCheckDistance, groundLayer);
        bool groundNear = Physics2D.Raycast(checkPos, Vector2.down, groundCheckDistance * 0.6f, groundLayer);

        Vector2 wallCheckPos = new Vector2(transform.position.x + moveDir * 0.35f, transform.position.y - 0.15f);
        bool isWallAhead = Physics2D.Raycast(wallCheckPos, Vector2.right * moveDir, 0.25f, groundLayer);

        Vector2 headPos = headCheck ? (Vector2)headCheck.position :
            new Vector2(transform.position.x, transform.position.y + 0.6f);
        bool isHeadBlocked = Physics2D.Raycast(headPos, Vector2.up, headCheckDistance, groundLayer);

        // --- Debug ---
        Debug.DrawRay(checkPos, Vector2.down * groundCheckDistance, groundFar ? Color.green : Color.red);
        Debug.DrawRay(wallCheckPos, Vector2.right * moveDir * 0.25f, isWallAhead ? Color.magenta : Color.cyan);
        Debug.DrawRay(headPos, Vector2.up * headCheckDistance, isHeadBlocked ? Color.yellow : Color.blue);

        // --- Condición de giro (con cooldown y confirmación doble) ---
        bool edgeDetected = !groundFar && !groundNear;  // confirma que realmente NO hay suelo

        if ((edgeDetected || isWallAhead ||
            (movingRight && transform.position.x > startPosition.x + patrolRange) ||
            (!movingRight && transform.position.x < startPosition.x - patrolRange))
            && Time.time > lastTurnTime + turnCooldown)
        {
            movingRight = !movingRight;
            lastTurnTime = Time.time;
        }

        // --- Anti-pegado en techo ---
        if (isHeadBlocked)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Min(rb.velocity.y, -1f));
            rb.AddForce(Vector2.down * 6f, ForceMode2D.Force);
        }
    }
    void ChasePlayer()
    {
        if (player == null) return;

        float directionX = Mathf.Sign(player.position.x - transform.position.x);
        rb.velocity = new Vector2(directionX * moveSpeed, rb.velocity.y);

        if (directionX > 0 && !movingRight) movingRight = true;
        else if (directionX < 0 && movingRight) movingRight = false;

        float heightDifference = player.position.y - transform.position.y;
        if (heightDifference > 1f && heightDifference < maxJumpHeightDifference &&
            Time.time > lastJumpTime + jumpCooldown)
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
        rb.velocity = new Vector2(rb.velocity.x, 0f);

        Vector2 jumpVector = new Vector2(directionX * moveSpeed * 0.8f, jumpForce);
        rb.AddForce(jumpVector, ForceMode2D.Impulse);
        Debug.Log($"🦘 {gameObject.name} saltó hacia el jugador.");
    }

    void TryAttack()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;

            if (player != null && player.gameObject.activeInHierarchy)
            {
                var ph = player.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(damage, transform);
                Debug.Log($"💥 {gameObject.name} atacó al jugador causando {damage} de daño.");

                // arrancamos feedback controlado
                if (attackCR != null) StopCoroutine(attackCR);
                attackCR = StartCoroutine(AttackFeedback());
            }
        }
    }

    private IEnumerator AttackFeedback()
    {
        if (sr == null) yield break;

        sr.color = Color.yellow;

        if (rb != null && player != null && player.gameObject.activeInHierarchy)
        {
            Vector2 recoilDir = (transform.position - player.position).normalized;
            rb.AddForce(recoilDir * recoilForce, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(attackFlashTime);

        if (this == null || sr == null) yield break;

        sr.color = Color.white;
        yield return new WaitForSeconds(recoilDuration);

        if (rb != null) rb.velocity = new Vector2(0f, rb.velocity.y);
        attackCR = null; // 🔚
    }

    // 🔧 Se llama automáticamente cuando el player desaparece (respawn)
    private void ResetAfterPlayerHidden()
    {
        // Paramos ataque en curso y limpiamos color
        if (attackCR != null) { StopCoroutine(attackCR); attackCR = null; }
        if (sr != null) sr.color = Color.white;

        // Salir de chase y resetear patrulla
        isChasing = false;
        chaseLostTime = 0f;
        startPosition = transform.position;

        // Elegir el lado “libre” (evita vibrar contra pared)
        movingRight = ChooseFreeSide();

        // Limpiar empujes residuales
        rb.velocity = new Vector2(0f, rb.velocity.y);

        // Pequeño cooldown para no atacar inmediatamente al reaparecer
        lastAttackTime = Time.time + 0.2f;
    }

    private bool ChooseFreeSide()
    {
        // Raycasts cortos a ambos lados y elegimos el que NO tenga pared
        Vector2 leftPos = new Vector2(transform.position.x - 0.35f, transform.position.y - 0.15f);
        Vector2 rightPos = new Vector2(transform.position.x + 0.35f, transform.position.y - 0.15f);
        bool wallLeft = Physics2D.Raycast(leftPos, Vector2.left, 0.25f, groundLayer);
        bool wallRight = Physics2D.Raycast(rightPos, Vector2.right, 0.25f, groundLayer);

        if (wallLeft && !wallRight) return true;   // ir a la derecha
        if (wallRight && !wallLeft) return false;  // ir a la izquierda
        return movingRight; // si ambos libres/ocupados, conserva
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"🩸 {gameObject.name} recibió {amount} de daño. Vida restante: {currentHealth}");

        if (sr != null) StartCoroutine(FlashRed());

        if (currentHealth <= 0) Die();
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
        if (attackCR != null) { StopCoroutine(attackCR); attackCR = null; }
        if (sr != null) sr.color = Color.white;

        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        var levelManager = FindObjectOfType<LevelManager>();
        if (levelManager != null) levelManager.EnemyDefeated();

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        }
        if (headCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(headCheck.position, headCheck.position + Vector3.up * headCheckDistance);
        }
    }
}
