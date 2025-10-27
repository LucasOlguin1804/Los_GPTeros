using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    // Configura la barra inicial
    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;

        if (fill != null && gradient != null)
            fill.color = gradient.Evaluate(1f);
    }

    // Actualiza la barra
    public void SetHealth(int health)
    {
        slider.value = health;

        if (fill != null && gradient != null)
            fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}
