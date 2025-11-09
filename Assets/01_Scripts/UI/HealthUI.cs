using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerHealth playerHealth;   // Arrastra aquí tu objeto Player con PlayerHealth
    public Image healthBarFill;         // Arrastra aquí la imagen verde (HealthBar_Fill)
    public Text healthText;             // Opcional (si quieres mostrar 100/100)

    [Header("Animación visual")]
    public float smoothSpeed = 10f;     // Qué tan suave baja/sube la barra
    private float currentFill = 1f;

    void Update()
    {
        if (playerHealth == null || healthBarFill == null)
            return;

        // 🔹 Obtener valores actuales
        int current = Mathf.Clamp(playerHealth.GetCurrentHealth(), 0, playerHealth.maxHealth);
        float targetFill = (float)current / playerHealth.maxHealth;

        // 🔹 Transición suave de la barra
        currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * smoothSpeed);
        healthBarFill.fillAmount = currentFill;

        // 🔹 (Opcional) Mostrar texto con vida numérica
        if (healthText != null)
        {
            healthText.text = $"{current} / {playerHealth.maxHealth}";
        }
    }
}

