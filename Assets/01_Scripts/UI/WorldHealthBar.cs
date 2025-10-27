using UnityEngine;
using UnityEngine.UI;

public class WorldHealthBar : MonoBehaviour
{
    public Image fill;
    private float maxHealth;

    public void SetMaxHealth(float health)
    {
        maxHealth = health;
        fill.fillAmount = 1f;
    }

    public void SetHealth(float health)
    {
        fill.fillAmount = health / maxHealth;
    }
}
