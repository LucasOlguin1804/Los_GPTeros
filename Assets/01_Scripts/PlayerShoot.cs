using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bulletPrefab;   // Prefab de la bala
    public Transform firePoint;       // Punto de salida del disparo
    public float bulletSpeed = 10f;   // Velocidad de la bala

    private Vector2 lookDir;          // Dirección hacia el mouse
    private float minDistance = 0.2f; // Distancia mínima para evitar disparos lentos

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

        // Posición del mouse en coordenadas del mundo
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Calcular vector desde el FirePoint al mouse
        Vector2 rawDir = mousePos - firePoint.position;
        float distance = rawDir.magnitude;

        // Evita valores demasiado pequeños que provocan disparos lentos
        if (distance < minDistance)
        {
            // Mantén la última dirección válida, no recalcules aún
            return;
        }

        // Normalizar dirección
        lookDir = rawDir.normalized;

        // Calcular el ángulo de rotación del FirePoint
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // Si el mouse está demasiado cerca, no dispares
        if (lookDir == Vector2.zero) return;

        // Crear la bala
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Aplicar velocidad constante
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = lookDir * bulletSpeed;

        // Destruir después de un tiempo
        Destroy(bullet, 2f);
    }
}
