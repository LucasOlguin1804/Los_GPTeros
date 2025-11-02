using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunPowerUp : MonoBehaviour
{
    [Header("Configuración del Power-Up")]
    public int totalShots = 5;          // cuántos disparos de escopeta se pueden hacer
    public int pelletsPerShot = 5;      // cuántas balas salen por disparo
    public float spreadAngle = 15f;     // dispersión entre balas
    public float fireRate = 0.5f;       // tiempo entre disparos
    public float rotationSpeed = 60f;   // rotación visual

    void Update()
    {
        //forwar(rotacion en circulo)
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerShoot playerShoot = other.GetComponent<PlayerShoot>();
            if (playerShoot != null)
            {
                playerShoot.ActivateShotgun(totalShots, pelletsPerShot, spreadAngle, fireRate);
                Debug.Log($"💣 Power-Up de escopeta recogido ({totalShots} disparos)");
            }

            Destroy(gameObject);
        }
    }
}

