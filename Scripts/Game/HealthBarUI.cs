using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the UI representation of a health bar, including updating its size based on health changes.
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    /// <summary>
    /// The current health value represented by the health bar.
    /// </summary>
    private float health;

    /// <summary>
    /// The maximum health value represented by the health bar.
    /// </summary>
    private float maxHealth;

    /// <summary>
    /// The maximum width of the health bar, used to calculate scaling.
    /// </summary>
    private float maxWidth;

    /// <summary>
    /// The RectTransform of the health bar, which is resized to represent health.
    /// </summary>
    private RectTransform bar;

    /// <summary>
    /// Initializes the health bar, setting its maximum width and ensuring the RectTransform is assigned.
    /// </summary>
    void Awake()
    {
        bar = GetComponent<RectTransform>();
        if (bar == null)
        {
            Debug.LogError("HealthBarUI: RectTransform component is missing on this GameObject.");
        }

        maxWidth = bar.sizeDelta.x;
    }

    /// <summary>
    /// Sets the current health value and updates the health bar's size.
    /// </summary>
    /// <param name="newHealth">The new health value to set.</param>
    public void SetHealth(float newHealth)
    {
        health = Mathf.Clamp(newHealth, 0, maxHealth);
        UpdateBar();
    }

    /// <summary>
    /// Sets the maximum health value and updates the health bar to reflect the new maximum.
    /// </summary>
    /// <param name="newMaxHealth">The new maximum health value to set.</param>
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = Mathf.Max(newMaxHealth, 0);
        health = maxHealth;
        UpdateBar();
    }

    /// <summary>
    /// Updates the health bar's size based on the current health and maximum health values.
    /// </summary>
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
