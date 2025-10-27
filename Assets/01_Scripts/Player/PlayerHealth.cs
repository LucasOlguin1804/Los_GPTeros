using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Atributos del jugador")]
    public int maxHealth = 100;
    public float respawnDelay = 2f;

    [Header("Invulnerabilidad al reaparecer")]
    public float invulnerableTime = 2f;
    public float blinkInterval = 0.2f;

    [Header("Knockback al recibir daño")]
    [Tooltip("Fuerza del retroceso horizontal")]
    public float knockbackForce = 7f;

    [Tooltip("Impulso vertical adicional al recibir daño")]
    public float verticalKnockForce = 4f;

    [Tooltip("Duración del retroceso")]
    public float knockbackDuration = 0.25f;

    private int currentHealth;
    private Vector3 spawnPosition;
    private bool isDead = false;
    private bool isInvulnerable = false;
    private Rigidbody2D rb;
    private SpriteRenderer[] spriteRenderers;

    void Awake()
    {
        GameObject spawn = GameObject.FindGameObjectWithTag("SpawnPoint");
        spawnPosition = spawn != null ? spawn.transform.position : transform.position;

        rb = GetComponent<Rigidbody2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount, Transform attacker = null)
    {
        if (isDead || isInvulnerable) return;

        currentHealth -= amount;
        Debug.Log($"❤️‍🔥 Jugador recibió {amount} de daño. Vida restante: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(FlashAllSpritesRed());

        if (attacker != null)
        {
            Vector2 knockDir = (transform.position - attacker.position).normalized;
            StartCoroutine(ApplyKnockback(knockDir));
        }
    }

    private IEnumerator ApplyKnockback(Vector2 direction)
    {
        if (rb == null) yield break;

        rb.velocity = Vector2.zero;

        // 🧭 Agrega un impulso hacia atrás y ligeramente hacia arriba
        Vector2 force = new Vector2(direction.x * knockbackForce, verticalKnockForce);
        rb.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        rb.velocity = Vector2.zero;
    }

    private IEnumerator FlashAllSpritesRed()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0) yield break;

        foreach (var sr in spriteRenderers)
            if (sr != null) sr.color = Color.red;

        yield return new WaitForSeconds(0.15f);

        foreach (var sr in spriteRenderers)
            if (sr != null) sr.color = Color.white;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("💀 Jugador ha muerto. Reiniciando posición...");
        gameObject.SetActive(false);
        Invoke(nameof(Respawn), respawnDelay);
    }

    private void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;

        transform.position = spawnPosition;
        gameObject.SetActive(true);

        Debug.Log("🔁 Jugador reapareció en el punto de spawn.");
        StartCoroutine(InvulnerabilityEffect());
    }

    private IEnumerator InvulnerabilityEffect()
    {
        isInvulnerable = true;
        float elapsed = 0f;

        Debug.Log("🛡️ Jugador invulnerable temporalmente...");

        while (elapsed < invulnerableTime)
        {
            foreach (var sr in spriteRenderers)
                if (sr != null) sr.enabled = !sr.enabled;

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        foreach (var sr in spriteRenderers)
            if (sr != null) sr.enabled = true;

        isInvulnerable = false;
        Debug.Log("✅ Invulnerabilidad terminada, jugador vulnerable otra vez.");
    }
}
