using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    private float health, maxHealth, maxWidth;
    private RectTransform bar;
    // Start is called before the first frame update
    void Awake()
    {
        bar = GetComponent<RectTransform>();
        if (bar == null)
        {
            Debug.LogError("HealthBarUI: RectTransform component is missing on this GameObject.");
        }

        maxWidth = bar.sizeDelta.x;
    }

    public void SetHealth(float newHealth)
    {
        health = Mathf.Clamp(newHealth, 0, maxHealth);
        UpdateBar();
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = Mathf.Max(newMaxHealth, 0);
        health = maxHealth;
        UpdateBar();
    }

    private void UpdateBar()
    {
        if (maxHealth <= 0)
        {
            bar.sizeDelta = new Vector2(0, bar.sizeDelta.y);
            return;
        }

        float healthRatio = health / maxHealth;
        float newWidth = maxWidth * healthRatio;
        bar.sizeDelta = new Vector2(newWidth, bar.sizeDelta.y);
    }
}
