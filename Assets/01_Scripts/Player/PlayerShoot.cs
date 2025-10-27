using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bulletPrefab;   // Prefab de la bala
    public Transform firePoint;       // Punto de salida del disparo
    public float bulletSpeed = 10f;   // Velocidad de la bala

    private Vector2 lookDir;          // Direcci�n hacia el mouse
    private float minDistance = 0.2f; // Distancia m�nima para evitar disparos lentos

    void Update()
    {
        AimTowardsMouse();

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void AimTowardsMouse()
    {
        if (firePoint == null) return;

        // Posici�n del mouse en coordenadas del mundo
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Calcular vector desde el FirePoint al mouse
        Vector2 rawDir = mousePos - firePoint.position;
        float distance = rawDir.magnitude;

        // Evita valores demasiado peque�os que provocan disparos lentos
        if (distance < minDistance)
        {
            // Mant�n la �ltima direcci�n v�lida, no recalcules a�n
            return;
        }

        // Normalizar direcci�n
        lookDir = rawDir.normalized;

        // Calcular el �ngulo de rotaci�n del FirePoint
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // Si el mouse est� demasiado cerca, no dispares
        if (lookDir == Vector2.zero) return;

        // Crear la bala
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Aplicar velocidad constante
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = lookDir * bulletSpeed;

        // Destruir despu�s de un tiempo
        Destroy(bullet, 2f);
    }
}
