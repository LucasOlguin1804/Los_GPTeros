using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { Speed, DoubleJump, Shield }
    public PowerUpType powerUpType;

    [Header("Duración del efecto (segundos)")]
    public float duration = 5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();

            if (player != null)
            {
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
            }

            Destroy(gameObject); // Elimina el power-up al recogerlo
        }
    }
}
