using UnityEngine;

public class BossShooter : MonoBehaviour
{
    public GameObject projectilePrefab;  // Prefab de la bala
    public Transform shootPoint;         // Lugar desde donde dispara
    public float shootForce = 10f;       // Velocidad de disparo
    public float fireRate = 2f;          // Cada cuánto dispara (segundos)

    private float nextFireTime = 0f;

    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * shootForce; // dispara hacia la derecha
    }
}
