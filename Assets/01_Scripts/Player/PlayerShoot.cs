using System.Collections;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Disparo básico (pistola)")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;
    public float pistolFireRate = 0.4f;
    private float lastPistolShot;

    [Header("Disparo automático (metralleta - PowerUp)")]
    public bool isAutoMode = false;
    public float autoFireRate = 0.1f;
    public int autoBullets = 0;

    [Header("Disparo escopeta (PowerUp)")]
    public bool isShotgunMode = false;
    public int pelletsPerShot = 5;
    public float spreadAngle = 15f;
    public int shotgunShots = 0;
    public float shotgunFireRate = 1.5f;
    private float lastShotgunShot;

    [Header("Daño por arma")]
    public int pistolDamage = 40;
    public int autoDamage = 20;
    public int shotgunDamage = 25;

    private Vector2 lookDir;
    private float minDistance = 0.2f;
    private bool isShooting = false;

    void Update()
    {
        // 🛑 Bloquear disparos si el juego está pausado o se acaba de reanudar
        if (PauseMenu.IsPaused) return;
        if (Time.unscaledTime < PauseMenu.SuppressInputUntilUnscaled) return;

        AimTowardsMouse();

        // 🔫 Pistola normal
        if (!isAutoMode && !isShotgunMode && Input.GetMouseButtonDown(0))
        {
            if (Time.time >= lastPistolShot + pistolFireRate)
            {
                Shoot();
                lastPistolShot = Time.time;
            }
        }

        // 🔥 Automático (metralleta)
        if (isAutoMode)
        {
            if (Input.GetMouseButtonDown(0) && !isShooting)
                StartCoroutine(AutoShoot());
        }

        // 💥 Escopeta
        if (isShotgunMode)
        {
            if (Input.GetMouseButtonDown(0) && Time.time >= lastShotgunShot + shotgunFireRate)
            {
                StartCoroutine(ShotgunShoot());
                lastShotgunShot = Time.time;
            }
        }
    }

    void AimTowardsMouse()
    {
        if (firePoint == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 rawDir = mousePos - firePoint.position;
        if (rawDir.magnitude < minDistance) return;

        lookDir = rawDir.normalized;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || lookDir == Vector2.zero) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null) b.SetDamage(pistolDamage);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = lookDir * bulletSpeed;
        Destroy(bullet, 2f);
    }

    private IEnumerator AutoShoot()
    {
        isShooting = true;

        while (Input.GetMouseButton(0) && autoBullets > 0)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            Bullet b = bullet.GetComponent<Bullet>();
            if (b != null) b.SetDamage(autoDamage);

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.velocity = lookDir * bulletSpeed;
            Destroy(bullet, 2f);

            autoBullets--;
            yield return new WaitForSeconds(autoFireRate);
        }

        if (autoBullets <= 0)
            isAutoMode = false;

        isShooting = false;
    }

    private IEnumerator ShotgunShoot()
    {
        if (shotgunShots <= 0) yield break;
        shotgunShots--;

        for (int i = 0; i < pelletsPerShot; i++)
        {
            float angleOffset = ((i - (pelletsPerShot - 1) / 2f) * spreadAngle);
            Quaternion pelletRotation = firePoint.rotation * Quaternion.Euler(0f, 0f, angleOffset);

            Vector3 offset = firePoint.right * (i - (pelletsPerShot - 1) / 2f) * 0.05f;

            GameObject pellet = Instantiate(bulletPrefab, firePoint.position + offset, pelletRotation);

            Bullet b = pellet.GetComponent<Bullet>();
            if (b != null) b.SetDamage(shotgunDamage);

            Rigidbody2D rb = pellet.GetComponent<Rigidbody2D>();
            rb.velocity = pellet.transform.right * bulletSpeed;
            Destroy(pellet, 2f);
        }

        if (shotgunShots <= 0)
            isShotgunMode = false;

        yield return null;
    }

    // 🟢 PowerUps
    public void ActivateAutoFire(int bulletCount, float newFireRate)
    {
        isAutoMode = true;
        autoBullets = bulletCount;
        autoFireRate = newFireRate;
        isShotgunMode = false;
        Debug.Log($"🔥 Power-Up automático activado con {bulletCount} balas");
    }

    public void ActivateShotgun(int shots, int pellets, float spread, float rate)
    {
        isShotgunMode = true;
        shotgunShots = shots;
        pelletsPerShot = pellets;
        spreadAngle = spread;
        shotgunFireRate = rate;
        isAutoMode = false;
        Debug.Log($"💥 Power-Up escopeta activado: {shots} disparos, {pellets} balas por tiro, dispersión {spread}°");
    }
}
