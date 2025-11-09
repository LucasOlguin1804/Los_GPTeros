using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Detección de Suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Coyote Time")]
    public float coyoteTime = 0.1f;
    private float coyoteCounter;

    [Header("Salud")]
    public int maxHealth = 5;
    public int currentHealth;
    public float invulnerabilityDuration = 1f;
    private bool isInvulnerable = false;

    [Header("Escudo")]
    public GameObject shieldVisual;
    private bool isShieldActive = false;

    [Header("Power-Ups")]
    [Tooltip("Saltos EXTRA totales que otorga el power-up (2 = dos saltos extra).")]
    public int maxExtraJumps = 2;
    public float speedMultiplier = 1.8f;

    [Header("Disparo")]
    public GameObject bulletPrefab;      // Prefab de la bala
    public Transform firePoint;          // Punto de salida
    public float bulletSpeed = 15f;
    public float fireRate = 0.25f;       // 0.25s = 4 disparos por segundo
    private float nextFireTime = 0f;

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool facingRight = true;

    private Vector3 originalShieldPos;
    private Vector3 originalShieldScale;

    // Coroutines activas
    private Coroutine speedBoostCR;
    private Coroutine doubleJumpCR;
    private Coroutine shieldCR;

    // Estado del power-up de salto
    private bool doubleJumpActive = false;
    private int extraJumpsRemaining = 0;
    private float doubleJumpExpireTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.sharedMaterial = new PhysicsMaterial2D { friction = 0, bounciness = 0 };
        currentHealth = maxHealth;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
            originalShieldPos = shieldVisual.transform.localPosition;
            originalShieldScale = shieldVisual.transform.localScale;
        }
    }

    void Update()
    {
        HandleMovement();
        HandleFacing();
        HandleJump();
        HandleShooting();

        // Mantener escudo centrado
        if (shieldVisual != null)
        {
            shieldVisual.transform.localPosition = originalShieldPos;
            Vector3 parentScale = transform.localScale;
            shieldVisual.transform.localScale = new Vector3(
                originalShieldScale.x / Mathf.Abs(parentScale.x),
                originalShieldScale.y / Mathf.Abs(parentScale.y),
                originalShieldScale.z
            );
        }

        // Expiración del power-up de salto
        if (doubleJumpActive && Time.time >= doubleJumpExpireTime)
            EndDoubleJumpPowerUp("⏱️ Tiempo agotado");
    }

    // ---------------- MOVIMIENTO ----------------
    void HandleMovement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    void HandleFacing()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (mousePos.x > transform.position.x && !facingRight) Flip();
        else if (mousePos.x < transform.position.x && facingRight) Flip();
    }

    // ---------------- SALTO ----------------
    void HandleJump()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        if (Input.GetButtonDown("Jump"))
        {
            if (coyoteCounter > 0f)
            {
                Jump();
                isGrounded = false;
                coyoteCounter = 0f;
            }
            else if (doubleJumpActive && extraJumpsRemaining > 0)
            {
                Jump();
                extraJumpsRemaining--;
                Debug.Log($"🌀 Salto extra usado. Restan: {extraJumpsRemaining}");
                if (extraJumpsRemaining <= 0)
                    EndDoubleJumpPowerUp("✅ Power-Up de salto agotado por usos");
            }
        }
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // ---------------- DISPARO ----------------
    void HandleShooting()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            ShootTowardMouse();
        }
    }

    void ShootTowardMouse()
    {
        if (bulletPrefab == null || firePoint == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - firePoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Rigidbody2D rbBullet = bullet.GetComponent<Rigidbody2D>();
        if (rbBullet != null)
            rbBullet.velocity = direction * bulletSpeed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(bullet, 3f); // destruir después de 3s
    }

    // ---------------- DAÑO ----------------
    public void TakeDamage(int damage)
    {
        if (isShieldActive)
        {
            Debug.Log("🛡️ Escudo bloqueó el daño");
            return;
        }
        if (isInvulnerable) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(InvulnerabilityFlash());
    }

    private IEnumerator InvulnerabilityFlash()
    {
        isInvulnerable = true;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float elapsed = 0f;

        while (elapsed < invulnerabilityDuration)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        sr.enabled = true;
        isInvulnerable = false;
    }

    private void Die()
    {
        Debug.Log("💀 Jugador ha muerto.");
    }

    // ---------------- ESCUDO ----------------
    public void ActivateShield(float duration)
    {
        if (shieldCR != null) StopCoroutine(shieldCR);
        shieldCR = StartCoroutine(ShieldEffect(duration));
    }

    private IEnumerator ShieldEffect(float duration)
    {
        isShieldActive = true;
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(true);
            shieldVisual.transform.localPosition = originalShieldPos;
            shieldVisual.transform.localScale = originalShieldScale;
        }

        SpriteRenderer sr = shieldVisual.GetComponent<SpriteRenderer>();
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float scalePulse = 1f + Mathf.Sin(Time.time * 3f) * 0.05f;
            shieldVisual.transform.localScale = originalShieldScale * scalePulse;
            if (sr != null)
            {
                float alpha = Mathf.PingPong(Time.time * 1.5f, 0.4f) + 0.6f;
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (sr != null)
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);

        shieldVisual.transform.localScale = originalShieldScale;
        shieldVisual.SetActive(false);
        isShieldActive = false;
        shieldCR = null;
        Debug.Log("❌ Escudo desactivado");
    }

    public bool IsShieldActive()
    {
        return isShieldActive;
    }

    // ---------------- VELOCIDAD ----------------
    public void ActivateSpeedBoost(float duration)
    {
        if (speedBoostCR != null) StopCoroutine(speedBoostCR);
        speedBoostCR = StartCoroutine(SpeedBoostRoutine(duration));
    }

    private IEnumerator SpeedBoostRoutine(float duration)
    {
        float originalSpeed = moveSpeed;
        moveSpeed *= speedMultiplier;
        Debug.Log($"⚡ Aumento de velocidad activado ({moveSpeed}) por {duration}s");

        yield return new WaitForSeconds(duration);

        moveSpeed = originalSpeed;
        speedBoostCR = null;
        Debug.Log("❌ Aumento de velocidad finalizado");
    }

    // ---------------- DOBLE SALTO ----------------
    public void ActivateDoubleJump(float duration)
    {
        if (doubleJumpCR != null) StopCoroutine(doubleJumpCR);
        doubleJumpCR = StartCoroutine(DoubleJumpRoutine(duration));
    }

    private IEnumerator DoubleJumpRoutine(float duration)
    {
        doubleJumpActive = true;
        extraJumpsRemaining = maxExtraJumps;
        doubleJumpExpireTime = Time.time + duration;
        Debug.Log($"🌀 Power-Up salto: {extraJumpsRemaining} saltos extra o {duration}s.");

        while (doubleJumpActive)
            yield return null;

        doubleJumpCR = null;
    }

    private void EndDoubleJumpPowerUp(string reason)
    {
        if (!doubleJumpActive) return;
        doubleJumpActive = false;
        extraJumpsRemaining = 0;
        Debug.Log($"❌ Power-Up de salto finalizado ({reason})");
    }
}
