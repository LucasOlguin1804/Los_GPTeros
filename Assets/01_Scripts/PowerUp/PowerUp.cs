using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { Shield } // 👈 solo el escudo de momento
    public PowerUpType powerUpType;
    public float duration = 5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();

            if (player != null && powerUpType == PowerUpType.Shield)
            {
                player.ActivateShield(duration);
            }

            Destroy(gameObject);
        }
    }
}


