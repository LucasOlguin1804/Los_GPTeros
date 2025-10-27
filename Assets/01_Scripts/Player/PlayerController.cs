using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Detección de Suelo")]
    public Transform groundCheck;          // Punto bajo del jugador
    public float groundCheckRadius = 0.2f; // Radio del círculo de detección
    public LayerMask groundLayer;          // Capa del suelo

    [Header("Coyote Time")]
    public float coyoteTime = 0.1f;
    private float coyoteCounter;

    [Header("Salud")]
    public int maxHealth = 5;
    public int currentHealth;
    public float invulnerabilityDuration = 1f;
    private bool isInvulnerable = false;

    // Componentes internos
    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        // Evita que el jugador se "pegue" por fricción
        rb.sharedMaterial = new PhysicsMaterial2D
        {
            friction = 0,
            bounciness = 0
        };

        // Inicializa salud
        currentHealth = maxHealth;
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
        // 🔍 Detección de suelo con OverlapCircle
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Coyote time
        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        // Salto
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

    // ========================
    // 🩸 SISTEMA DE DAÑO / VIDA
    // ========================
    public void TakeDamage(int damage)
    {
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
            sr.enabled = !sr.enabled; // parpadea
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        sr.enabled = true;
        isInvulnerable = false;
    }

    private void Die()
    {
        Debug.Log("💀 Jugador ha muerto.");
        // Aquí puedes reiniciar escena, mostrar pantalla de derrota o animación
        // usando UnityEngine.SceneManagement:
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
