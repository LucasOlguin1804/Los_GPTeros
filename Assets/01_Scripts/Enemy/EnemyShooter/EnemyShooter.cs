using System.Collections;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Atributos del enemigo")]
    public int maxHealth = 80;
    public int damage = 20;
    public float moveSpeed = 1.8f;
    public float detectionRange = 7f;

    [Header("Disparo")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 8f;
    public float attackCooldown = 1.5f;

    [Header("Detección (pies / pared / cabeza)")]
    public Transform groundCheck;
    public float groundCheckDistance = 0.5f;
    public Transform headCheck;
    public float headCheckDistance = 0.3f;
    public LayerMask groundLayer;

    [Header("Salto entre plataformas")]
    public float jumpForce = 6f;
    public float maxJumpHeightDifference = 3f;
    public float jumpCooldown = 1.5f;

    [Header("Efectos visuales")]
    public float attackFlashTime = 0.1f;
    public float recoilForce = 2f;

    private int currentHealth;
    private bool movingRight = true;
    private bool isChasing = false;
    private float lastAttackTime;
    private float lastJumpTime;
    private float lastTurnTime;
    private float turnCooldown = 0.25f;

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
        rb.freezeRotation = true;
        rb.sharedMaterial = new PhysicsMaterial2D { friction = 0, bounciness = 0 };

        currentHealth = maxHealth;

        CalculatePatrolLimits();

        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null)
            player = pObj.transform;
    }

    void Update()
    {
        // 🛑 Detener toda lógica si el juego está pausado
        if (PauseMenu.IsPaused)
        {
            rb.velocity = Vector2.zero;  // detiene el movimiento físico
            return;
        }

        if (player == null)
        {
            var pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null)
                player = pObj.transform;
            Patrol();
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (isChasing)
        {
            if (distance > detectionRange + 1f)
            {
                isChasing = false;
                CalculatePatrolLimits();
            }
        }
        else if (distance < detectionRange)
            isChasing = true;

        if (isChasing)
            ChaseAndShoot();
        else
            Patrol();
    }


    // 🔹 Patrullaje con detección de bordes reales
    void Patrol()
    {
        float moveDir = movingRight ? 1f : -1f;
        rb.velocity = new Vector2(moveDir * moveSpeed, rb.velocity.y);

        Vector2 checkPos = groundCheck ? (Vector2)groundCheck.position :
            new Vector2(transform.position.x + moveDir * 0.5f, transform.position.y - 0.1f);

        bool groundFar = Physics2D.Raycast(checkPos, Vector2.down, groundCheckDistance, groundLayer);
        bool groundNear = Physics2D.Raycast(checkPos, Vector2.down, groundCheckDistance * 0.6f, groundLayer);

        Vector2 wallPos = new Vector2(transform.position.x + moveDir * 0.35f, transform.position.y - 0.15f);
        bool isWallAhead = Physics2D.Raycast(wallPos, Vector2.right * moveDir, 0.25f, groundLayer);

        Debug.DrawRay(checkPos, Vector2.down * groundCheckDistance, groundFar ? Color.green : Color.red);
        Debug.DrawRay(wallPos, Vector2.right * moveDir * 0.25f, isWallAhead ? Color.magenta : Color.cyan);

        bool edgeDetected = !groundFar && !groundNear;

        if ((edgeDetected || isWallAhead ||
            (movingRight && transform.position.x > rightLimit) ||
            (!movingRight && transform.position.x < leftLimit))
            && Time.time > lastTurnTime + turnCooldown)
        {
            movingRight = !movingRight;
            lastTurnTime = Time.time;
        }

        // 🔁 Actualiza la dirección visual según movimiento
        UpdateVisualFacing(movingRight);
    }

    // 🔹 Persecución con disparo
    void ChaseAndShoot()
    {
        if (player == null) return;

        float directionX = Mathf.Sign(player.position.x - transform.position.x);
        rb.velocity = new Vector2(directionX * moveSpeed, rb.velocity.y);

        // 🧭 Girar visualmente hacia el jugador
        bool shouldFaceRight = player.position.x > transform.position.x;
        UpdateVisualFacing(shouldFaceRight);

        if (directionX > 0 && !movingRight) movingRight = true;
        else if (directionX < 0 && movingRight) movingRight = false;

        float heightDiff = player.position.y - transform.position.y;
        if (heightDiff > 1f && heightDiff < maxJumpHeightDifference && Time.time > lastJumpTime + jumpCooldown)
            JumpTowardsPlayer();

        if (Vector2.Distance(transform.position, player.position) < detectionRange - 1f)
            TryShoot();
    }

    // 🔁 Voltea sprite y firePoint sin tocar la lógica
    void UpdateVisualFacing(bool faceRight)
    {
        if (sr != null)
            sr.flipX = !faceRight;

        if (firePoint != null)
        {
            Vector3 fp = firePoint.localPosition;
            fp.x = Mathf.Abs(fp.x) * (faceRight ? 1 : -1);
            firePoint.localPosition = fp;
        }
    }

    void TryShoot()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;
        if (firePoint == null || bulletPrefab == null) return;

        Vector2 dir = (player.position - firePoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        EnemyBullet eb = bullet.GetComponent<EnemyBullet>();
        if (eb != null)
        {
            eb.direction = dir;
            eb.damage = damage;
            eb.speed = bulletSpeed;
        }

        StartCoroutine(AttackFlash());
    }

    private IEnumerator AttackFlash()
    {
        if (sr == null) yield break;
        sr.color = Color.yellow;
        yield return new WaitForSeconds(attackFlashTime);
        sr.color = Color.white;
    }

    void JumpTowardsPlayer()
    {
        if (rb == null) return;
        lastJumpTime = Time.time;

        float dir = Mathf.Sign(player.position.x - transform.position.x);
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(new Vector2(dir * moveSpeed, jumpForce), ForceMode2D.Impulse);
    }

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

        Debug.Log($"📏 Nuevos límites: izquierda={leftLimit}, derecha={rightLimit}");
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
        Debug.Log($"💀 {gameObject.name} ha muerto.");
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        // ❌ antes notificaba al LevelManager, ahora ya no.
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
