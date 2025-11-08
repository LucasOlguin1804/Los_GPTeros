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
    public GameObject shieldVisual;          // Asignar en el inspector
    public Vector3 shieldOffset = new Vector3(0f, 0.3f, 0f); // 👈 Ajuste manual desde el Inspector
    private bool isShieldActive = false;

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        rb.sharedMaterial = new PhysicsMaterial2D
        {
            friction = 0,
            bounciness = 0
        };

        currentHealth = maxHealth;

        if (shieldVisual != null)
            shieldVisual.SetActive(false);
    }

    void Update()
    {
        HandleMovement();
        HandleFacing();
        HandleJump();
    }

    void HandleMovement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    void HandleFacing()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (mousePos.x > transform.position.x && !facingRight)
            Flip();
        else if (mousePos.x < transform.position.x && facingRight)
            Flip();
    }

    void HandleJump()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        if (Input.GetButtonDown("Jump") && coyoteCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public int GetDirection()
    {
        return facingRight ? 1 : -1;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    // 🩸 SISTEMA DE DAÑO / VIDA
    public void TakeDamage(int damage)
    {
        if (isShieldActive)
        {
            Debug.Log("🛡️ Escudo bloqueó el daño");
            return;
        }

        if (isInvulnerable) return;

        currentHealth -= damage;
        Debug.Log("Jugador recibe daño: " + damage + " | Vida restante: " + currentHealth);

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

    // 🛡️ SISTEMA DE ESCUDO
    public void ActivateShield(float duration)
    {
        if (shieldVisual == null)
        {
            shieldVisual = transform.Find("ShieldVisual")?.gameObject;
            if (shieldVisual == null)
            {
                Debug.LogWarning("⚠️ No se encontró ShieldVisual en el Player.");
                return;
            }
        }

        StopCoroutine(nameof(ShieldRoutine));
        StartCoroutine(ShieldRoutine(duration));
    }

    private IEnumerator ShieldRoutine(float duration)
    {
        isShieldActive = true;
        shieldVisual.SetActive(true);

        // ✅ Mantiene tu offset configurado desde el Inspector
        shieldVisual.transform.localPosition = shieldOffset;

        SpriteRenderer sr = shieldVisual.GetComponent<SpriteRenderer>();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (sr != null)
            {
                float alpha = Mathf.PingPong(Time.time * 2f, 0.5f) + 0.5f;
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (sr != null)
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);

        shieldVisual.SetActive(false);
        isShieldActive = false;
        Debug.Log("❌ Escudo desactivado después de " + duration + "s");
    }

    public bool IsShieldActive()
    {
        return isShieldActive;
    }
}
