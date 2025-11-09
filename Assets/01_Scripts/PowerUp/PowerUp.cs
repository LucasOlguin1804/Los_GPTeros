using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { Speed, DoubleJump, Shield }
    public PowerUpType powerUpType;

    [Header("Duración del efecto (segundos)")]
    public float duration = 5f;

    [Header("Efecto visual")]
    public float rotationSpeed = 90f;

    void Update()
    {
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        // Evita interferencias con balas o ataques
        if (collision.CompareTag("Bullet") || collision.CompareTag("EnemyBullet")) return;

        PlayerController player = collision.GetComponent<PlayerController>();
        if (player == null) return;

        switch (powerUpType)
        {
            case PowerUpType.Speed:
                player.ActivateSpeedBoost(duration);
                break;
            case PowerUpType.DoubleJump:
                player.ActivateDoubleJump(duration);
                break;
            case PowerUpType.Shield:
                player.ActivateShield(duration);
                break;
        }

        Destroy(gameObject);
    }
}
