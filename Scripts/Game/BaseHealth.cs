using UnityEngine;
using UnityEngine.UI; // For UI text display
using TMPro;
using System;


public class BaseHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    // public TextMeshProUGUI healthText; // Assign a UI Text element in the Inspector

    public HealthBarUI healthBarUI; // Reference to the HealthBarUI script

    void Start()
    {
        Reset();
    }

    void Reset()
    {
        currentHealth = maxHealth;
        // UpdateHealthUI();
        healthBarUI.SetMaxHealth(maxHealth);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        // UpdateHealthUI();
        healthBarUI.SetHealth(currentHealth);
        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    // void UpdateHealthUI()
    // {
    //     if (healthText != null)
    //     {
    //         healthText.text = "Base HP: " + currentHealth;
    //     }
    // }

    void GameOver()
    {
        Reset(); // Reset the base health for the next game
        GameManager.instance.GameOver(); // Call GameOver method in GameManager
    }
}
