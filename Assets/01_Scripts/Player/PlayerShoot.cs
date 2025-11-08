using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
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

    [Header("Sonidos de disparo")]
    public AudioClip revolverClip;
    public AudioClip rifleClip;
    public AudioClip shotgunClip;
    [Range(0f, 1f)] public float volume = 0.75f;
    [Range(-0.2f, 0.2f)] public float pitchRandomness = 0.1f;

    private AudioSource audioSource;
    private Vector2 lookDir;
    private float minDistance = 0.2f;
    private bool isShooting = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f; // 0 = 2D (para UI/juegos 2D)
    }

    void Update()
    {
        if (PauseMenu.IsPaused || Time.timeScale == 0f)
            return;

        if (Time.unscaledTime < PauseMenu.SuppressInputUntilUnscaled)
            return;

        AimTowardsMouse();

        // 🔫 Pistola
        if (!isAutoMode && !isShotgunMode && Input.GetMouseButtonDown(0))
        {
            if (Time.time >= lastPistolShot + pistolFireRate)
            {
                Shoot(revolverClip);
                lastPistolShot = Time.time;
            }
        }

        // 🔥 Automático
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

    void Shoot(AudioClip clip)
    {
        if (bulletPrefab == null || firePoint == null || lookDir == Vector2.zero) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null) b.SetDamage(pistolDamage);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = lookDir * bulletSpeed;
        Destroy(bullet, 2f);

        PlaySound(clip);
    }

    private IEnumerator AutoShoot()
    {
        isShooting = true;
        while (Input.GetMouseButton(0) && autoBullets > 0 && !PauseMenu.IsPaused)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Bullet b = bullet.GetComponent<Bullet>();
            if (b != null) b.SetDamage(autoDamage);

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.velocity = lookDir * bulletSpeed;
            Destroy(bullet, 2f);

            autoBullets--;
            PlaySound(rifleClip);

            yield return new WaitForSeconds(autoFireRate);
        }

        if (autoBullets <= 0) isAutoMode = false;
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

        PlaySound(shotgunClip);

        if (shotgunShots <= 0) isShotgunMode = false;
        yield return null;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;

        // Randomizar pitch para evitar monotonía
        audioSource.pitch = 1f + Random.Range(-pitchRandomness, pitchRandomness);
        audioSource.PlayOneShot(clip, volume);
    }

    // PowerUps
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

