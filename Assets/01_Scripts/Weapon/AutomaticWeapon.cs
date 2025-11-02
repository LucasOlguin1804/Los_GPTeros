using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticWeapon : MonoBehaviour
{
    [Header("Configuración del Power-Up")]
    public int bulletCount = 20;      // cuántas balas automáticas otorga
    public float autoFireRate = 0.1f; // tiempo entre disparos (más bajo = más rápido)
    public float rotationSpeed = 60f; // para girar visualmente la cápsula

    private void Update()
    {
        // Rotación visual (decorativo)
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerShoot playerShoot = other.GetComponent<PlayerShoot>();
            if (playerShoot != null)
            {
                playerShoot.ActivateAutoFire(bulletCount, autoFireRate);
                Debug.Log($"⚡ Power-Up recogido: modo automático con {bulletCount} balas.");
            }

            Destroy(gameObject); // desaparece el ítem al recogerlo
        }
    }
}
