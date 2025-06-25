using UnityEngine;

/// <summary>
/// Manages the visual representation of a health bar, including updating its fill based on health changes.
/// </summary>
public class HealthBar : MonoBehaviour
{
    /// <summary>
    /// The maximum health value represented by the health bar.
    /// </summary>
    public float maxHealth = 100f;

    /// <summary>
    /// The current health value represented by the health bar.
    /// </summary>
    private float currentHealth;

    /// <summary>
    /// The initial scale of the health bar fill, used to calculate scaling.
    /// </summary>
    private Vector3 initialScale; // Store the initial scale of the health bar fill

    /// <summary>
    /// The transform of the health bar fill, which is scaled to represent health.
    /// </summary>
    private Transform healthBarFillTransform; // Drag your "HealthBarFill" Cube's Transform here

    /// <summary>
    /// Initializes the health bar, setting its initial scale and health values.
    /// </summary>
    void Awake()
    {
        healthBarFillTransform = transform;
        initialScale = healthBarFillTransform.localScale;
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    /// <summary>
    /// Reduces the current health by the specified amount and updates the health bar.
    /// </summary>
    /// <param name="amount">The amount of health to reduce.</param>
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
    }

    /// <summary>
    /// Increases the current health by the specified amount and updates the health bar.
    /// </summary>
    /// <param name="amount">The amount of health to restore.</param>
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
    }

    /// <summary>
    /// Updates the health bar's fill scale based on the current health value.
    /// </summary>
    void UpdateHealthBar()
    {
        float fillRatio = currentHealth / maxHealth;
        // Scale the X-axis (assuming your bar extends along X)
        healthBarFillTransform.localScale = new Vector3(initialScale.x * fillRatio, initialScale.y, initialScale.z);
    }

    /// <summary>
    /// Example method for testing health bar functionality using keyboard input.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            TakeDamage(10);
        }
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            Heal(10);
        }
    }
}