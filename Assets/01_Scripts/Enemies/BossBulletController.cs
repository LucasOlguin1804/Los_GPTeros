using UnityEngine;

public class BossBulletController : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 1;
    public float lifeTime = 5f;

    private Vector2 direction;

    void Start()
    {
        // Destruye la bala después de un tiempo
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // Daño al jugador
        if (col.CompareTag("Player"))
        {
            col.GetComponent<PlayerController>().TakeDamage(damage);
            Destroy(gameObject);
        }

        // Si choca con el suelo o una pared
        if (col.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
