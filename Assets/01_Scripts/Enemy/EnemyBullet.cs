using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyBullet : MonoBehaviour
{
    [Header("Propiedades de la bala enemiga")]
    public int damage = 10;
    public float speed = 6f;
    public float lifeTime = 3f;

    [HideInInspector] public Vector2 direction; // Asignada por EnemyShooter

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.velocity = direction.normalized * speed;

        // Rotación visual hacia la dirección del disparo
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        // Evita golpear a otros enemigos
        if (col.CompareTag("Enemy")) return;

        // Daño al jugador
        if (col.CompareTag("Player"))
        {
            PlayerHealth ph = col.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(damage, transform);

            Destroy(gameObject);
            return;
        }

        // Se destruye si choca con entorno
        if (col.CompareTag("Ground") || col.CompareTag("Wall") || col.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
