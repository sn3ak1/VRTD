using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    private Vector3 initialScale; // Store the initial scale of the health bar fill

    private Transform healthBarFillTransform; // Drag your "HealthBarFill" Cube's Transform here

    void Awake()
    {
        healthBarFillTransform = transform;
        initialScale = healthBarFillTransform.localScale;
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        float fillRatio = currentHealth / maxHealth;
        // Scale the X-axis (assuming your bar extends along X)
        healthBarFillTransform.localScale = new Vector3(initialScale.x * fillRatio, initialScale.y, initialScale.z);
    }

    // Example for testing
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