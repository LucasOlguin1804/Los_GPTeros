using UnityEngine;

public class BossBulletEnemy : MonoBehaviour
{
    [Header("Configuración de la bala del Boss")]
    public float speed = 8f;
    public int damage = 10;
    public float lifeTime = 5f;

    private Rigidbody2D rb;
    private Vector2 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// Establece la dirección de movimiento de la bala.
    /// </summary>
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;

        if (rb != null)
            rb.velocity = moveDirection * speed;
    }

    void FixedUpdate()
    {
        if (rb == null)
            transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            PlayerController player = col.GetComponent<PlayerController>();
            if (player != null)
                player.TakeDamage(damage);

            Destroy(gameObject);
        }

        if (col.CompareTag("Ground") || col.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
