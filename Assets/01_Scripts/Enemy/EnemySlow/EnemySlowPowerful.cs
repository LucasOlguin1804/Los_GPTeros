using System.Collections;
using UnityEngine;

public class EnemySlowPowerful : MonoBehaviour
{
    [Header("Atributos del enemigo")]
    public int maxHealth = 100;
    public int damage = 30;
    public float moveSpeed = 1.5f;
    public float attackCooldown = 1.5f;
    public float detectionRange = 5f;

    [Header("Efectos visuales")]
    public float attackFlashTime = 0.1f;
    public float recoilDuration = 0.2f;
    public float recoilForce = 2f;

    [Header("Detección (pies / pared / cabeza)")]
    public Transform groundCheck;
    public float groundCheckDistance = 0.5f;
    public Transform headCheck;
    public float headCheckDistance = 0.3f;
    public LayerMask groundLayer;

    [Header("Salto entre plataformas")]
    public float jumpForce = 7f;
    public float maxJumpHeightDifference = 3f;
    public float jumpCooldown = 1.5f;

    private int currentHealth;
    private bool movingRight = true;
    private bool isChasing = false;
    private bool lastPlayerActive = true;

    private float lastAttackTime;
    private float lastJumpTime;
    private float chaseLostTime;
    private float chaseCooldown = 1.5f;
    private float turnCooldown = 0.25f;
    private float lastTurnTime = 0f;

    // 🔹 Nuevos límites dinámicos
    private float leftLimit;
    private float rightLimit;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;
    private Coroutine attackCR;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        rb.sharedMaterial = new PhysicsMaterial2D { friction = 0f, bounciness = 0f };
        rb.freezeRotation = true;

        currentHealth = maxHealth;

        // 🧭 Detecta los bordes de la plataforma inicial
        CalculatePatrolLimits();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        bool playerActive = player != null && player.gameObject.activeInHierarchy;

        // Si el jugador desaparece (respawn)
        if (!playerActive)
        {
            if (lastPlayerActive)
                ResetAfterPlayerHidden();

            Patrol();
            lastPlayerActive = false;
            return;
        }

        // Reasignar jugador si se perdió referencia
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
                    // 🔁 recalcula los límites de la nueva plataforma donde se detuvo
                    CalculatePatrolLimits();
                }
            }
            else chaseLostTime = 0f;
        }
        else if (distanceToPlayer < chaseEnterRange)
            isChasing = true;

        if (isChasing) ChasePlayer(); else Patrol();
    }

    // 🔹 Patrulla en los límites reales detectados
    void Patrol()
    {
        float moveDir = movingRight ? 1f : -1f;
        rb.velocity = new Vector2(moveDir * moveSpeed, rb.velocity.y);

        Vector2 checkPos = groundCheck ? (Vector2)groundCheck.position :
            new Vector2(transform.position.x + moveDir * 0.5f, transform.position.y - 0.1f);

        bool groundFar = Physics2D.Raycast(checkPos, Vector2.down, groundCheckDistance, groundLayer);
        bool groundNear = Physics2D.Raycast(checkPos, Vector2.down, groundCheckDistance * 0.6f, groundLayer);

        Vector2 wallCheckPos = new Vector2(transform.position.x + moveDir * 0.35f, transform.position.y - 0.15f);
        bool isWallAhead = Physics2D.Raycast(wallCheckPos, Vector2.right * moveDir, 0.25f, groundLayer);

        Vector2 headPos = headCheck ? (Vector2)headCheck.position :
            new Vector2(transform.position.x, transform.position.y + 0.6f);
        bool isHeadBlocked = Physics2D.Raycast(headPos, Vector2.up, headCheckDistance, groundLayer);

        // Debug visual
        Debug.DrawRay(checkPos, Vector2.down * groundCheckDistance, groundFar ? Color.green : Color.red);
        Debug.DrawRay(wallCheckPos, Vector2.right * moveDir * 0.25f, isWallAhead ? Color.magenta : Color.cyan);
        Debug.DrawRay(headPos, Vector2.up * headCheckDistance, isHeadBlocked ? Color.yellow : Color.blue);

        bool edgeDetected = !groundFar && !groundNear;

        if ((edgeDetected || isWallAhead ||
            (movingRight && transform.position.x > rightLimit) ||
            (!movingRight && transform.position.x < leftLimit))
            && Time.time > lastTurnTime + turnCooldown)
        {
            movingRight = !movingRight;
            lastTurnTime = Time.time;
        }

        // Anti-pegado
        if (isHeadBlocked)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Min(rb.velocity.y, -1f));
            rb.AddForce(Vector2.down * 6f, ForceMode2D.Force);
        }
    }

    // 🔹 Recalcula los límites reales de la plataforma actual
    private void CalculatePatrolLimits()
    {
        float checkDistance = 10f;
        Vector2 originLeft = transform.position + Vector3.left * 0.2f;
        Vector2 originRight = transform.position + Vector3.right * 0.2f;

        RaycastHit2D leftHit = Physics2D.Raycast(originLeft, Vector2.left, checkDistance, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(originRight, Vector2.right, checkDistance, groundLayer);

        if (leftHit.collider != null)
            leftLimit = leftHit.point.x + 0.4f;
        else
            leftLimit = transform.position.x - 3f;

        if (rightHit.collider != null)
            rightLimit = rightHit.point.x - 0.4f;
        else
            rightLimit = transform.position.x + 3f;

        Debug.Log($"📏 {gameObject.name} nuevos límites de patrulla: izquierda={leftLimit}, derecha={rightLimit}");
    }

    // 🔹 Lógica de persecución y ataque cuerpo a cuerpo
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
            JumpTowardsPlayer();

        if (Vector2.Distance(transform.position, player.position) < 1.2f)
            TryAttack();
    }

    void JumpTowardsPlayer()
    {
        if (rb == null || player == null) return;
        lastJumpTime = Time.time;

        float directionX = Mathf.Sign(player.position.x - transform.position.x);
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(new Vector2(directionX * moveSpeed * 0.8f, jumpForce), ForceMode2D.Impulse);
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
        sr.color = Color.white;
        yield return new WaitForSeconds(recoilDuration);

        if (rb != null) rb.velocity = new Vector2(0f, rb.velocity.y);
        attackCR = null;
    }

    // 🔹 Reinicio limpio tras respawn del jugador
    private void ResetAfterPlayerHidden()
    {
        if (attackCR != null) { StopCoroutine(attackCR); attackCR = null; }
        if (sr != null) sr.color = Color.white;

        isChasing = false;
        chaseLostTime = 0f;
        CalculatePatrolLimits();
        movingRight = ChooseFreeSide();
        rb.velocity = new Vector2(0f, rb.velocity.y);
        lastAttackTime = Time.time + 0.2f;
    }

    private bool ChooseFreeSide()
    {
        Vector2 leftPos = new Vector2(transform.position.x - 0.35f, transform.position.y - 0.15f);
        Vector2 rightPos = new Vector2(transform.position.x + 0.35f, transform.position.y - 0.15f);
        bool wallLeft = Physics2D.Raycast(leftPos, Vector2.left, 0.25f, groundLayer);
        bool wallRight = Physics2D.Raycast(rightPos, Vector2.right, 0.25f, groundLayer);

        if (wallLeft && !wallRight) return true;
        if (wallRight && !wallLeft) return false;
        return movingRight;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
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
        if (attackCR != null) { StopCoroutine(attackCR); attackCR = null; }
        if (sr != null) sr.color = Color.white;

        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        // ❌ eliminar LevelManager.EnemyDefeated()
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
