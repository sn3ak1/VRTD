using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Manages the health of the base in the game.
/// </summary>
public class BaseHealth : MonoBehaviour
{
    /// <summary>
    /// The maximum health of the base.
    /// </summary>
    public int maxHealth = 100;

    /// <summary>
    /// The current health of the base.
    /// </summary>
    private int currentHealth;

    /// <summary>
    /// Reference to the HealthBarUI component to update the health bar.
    /// </summary>
    public HealthBarUI healthBarUI;

    /// <summary>
    /// Initializes the base health at the start of the game.
    /// </summary>
    void Start()
    {
        Reset();
    }

    /// <summary>
    /// Resets the base health to the maximum value and updates the health bar.
    /// </summary>
    void Reset()
    {
        currentHealth = maxHealth;
        healthBarUI.SetMaxHealth(maxHealth);
    }

    /// <summary>
    /// Reduces the base's health by the specified damage amount and updates the health bar.
    /// Triggers the GameOver method if health drops to zero or below.
    /// </summary>
    /// <param name="damage">The amount of damage to apply to the base.</param>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBarUI.SetHealth(currentHealth);
        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    /// <summary>
    /// Handles the game over logic when the base's health reaches zero.
    /// Resets the base health and notifies the GameManager.
    /// </summary>
    void GameOver()
    {
        Reset(); // Reset the base health for the next game
        GameManager.instance.GameOver(); // Call GameOver method in GameManager
    }
}
