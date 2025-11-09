using System.Collections;
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
    public float knockbackForce = 7f;
    public float verticalKnockForce = 4f;
    public float knockbackDuration = 0.25f;

    [Header("Escudo temporal (PowerUp)")]
    public bool hasShield = false;
    public float shieldScale = 1.5f;
    public Color shieldColor = new Color(0f, 1f, 1f, 0.35f);

    private int currentHealth;
    private Vector3 spawnPosition;
    private bool isDead = false;
    private bool isInvulnerable = false;
    private Rigidbody2D rb;
    private SpriteRenderer[] spriteRenderers;
    private GameObject shieldVisual;

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

        // 🛡️ Chequear si hay escudo activo en PlayerController
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null && pc.IsShieldActive())
        {
            Debug.Log("🛡️ Escudo bloqueó el daño en PlayerHealth");
            return;
        }

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
        rb.AddForce(new Vector2(direction.x * knockbackForce, verticalKnockForce), ForceMode2D.Impulse);
        yield return new WaitForSeconds(knockbackDuration);
        rb.velocity = Vector2.zero;
    }

    private IEnumerator FlashAllSpritesRed()
    {
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

        // ⚡ Registrar muerte global
        if (GameOverManager.Instance != null)
            GameOverManager.Instance.PlayerDied();

        Invoke(nameof(Respawn), respawnDelay);
    }


    private void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        transform.position = spawnPosition;
        gameObject.SetActive(true);
        StartCoroutine(InvulnerabilityEffect());
    }

    private IEnumerator InvulnerabilityEffect()
    {
        isInvulnerable = true;
        float elapsed = 0f;

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
    }

    // 🛡️ Activador automático del escudo
    public void ActivateShield(float duration)
    {
        if (hasShield) return;
        hasShield = true;

        // Crear el círculo visual automáticamente
        shieldVisual = new GameObject("ShieldVisual");
        shieldVisual.transform.SetParent(transform);
        shieldVisual.transform.localPosition = Vector3.zero;
        shieldVisual.transform.localScale = Vector3.one * shieldScale;

        var sr = shieldVisual.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd"); // círculo simple integrado
        sr.color = shieldColor;
        sr.sortingOrder = 5;

        StartCoroutine(ShieldDuration(duration));
    }

    private IEnumerator ShieldDuration(float duration)
    {
        Debug.Log("🛡️ Escudo activado");
        yield return new WaitForSeconds(duration);

        hasShield = false;
        if (shieldVisual != null) Destroy(shieldVisual);
        Debug.Log("❌ Escudo desactivado");
    }
}

