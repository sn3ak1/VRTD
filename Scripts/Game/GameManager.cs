using UnityEngine;
using UnityEngine.UI; // For UI text display
using System.Collections.Generic; // For lists
using System.Linq; // For .ToArray()
using UnityEngine.AI;
using TMPro;
using System;


// Required for Oculus Integration
#if UNITY_EDITOR
using UnityEditor;
#endif
// using OVRInput; // For Oculus input

/// <summary>
/// Manages the overall game state, including game over logic, cooldowns, and interactions with other game systems.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the GameManager.
    /// </summary>
    public static GameManager instance;

    /// <summary>
    /// Cooldown time for drawing gestures.
    /// </summary>
    public int drawingCooldown = 5;

    /// <summary>
    /// Reference to the cooldown bar UI.
    /// </summary>
    public HealthBarUI cooldownBar;

    /// <summary>
    /// Reference to the DrawingSelector script.
    /// </summary>
    public DrawingSelector drawingSelector;

    /// <summary>
    /// Reference to the base target.
    /// </summary>
    public Transform targetBase;

    /// <summary>
    /// Reference to the guard target, can be set in the Inspector.
    /// </summary>
    public Transform targetGuard;

    /// <summary>
    /// Reference to the GuardSpawner script.
    /// </summary>
    public GuardSpawner guardSpawner;

    /// <summary>
    /// Reference to the EnemySpawner script.
    /// </summary>
    public EnemySpawner enemySpawner;

    /// <summary>
    /// Main game canvas.
    /// </summary>
    public Canvas canvasMain;

    /// <summary>
    /// Game over canvas.
    /// </summary>
    public Canvas canvasGameOver;

    /// <summary>
    /// Text to display the score on game over.
    /// </summary>
    public TextMeshProUGUI gameOverScoreText; // Text to display score on game over

    /// <summary>
    /// Flag to check if the game is over.
    /// </summary>
    public bool isGameOver = false; // Flag to check if the game is over

    /// <summary>
    /// Current cooldown time for drawing gestures.
    /// </summary>
    [NonSerialized]
    public float currDrawingCooldown = 0;

    /// <summary>
    /// Initializes the singleton instance of the GameManager.
    /// </summary>
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Sets up the cooldown bar at the start of the game.
    /// </summary>
    void Start()
    {
        cooldownBar.SetMaxHealth(drawingCooldown);
    }

    /// <summary>
    /// Updates the game state, including handling input and managing cooldowns.
    /// </summary>
    void Update()
    {
        if (isGameOver)
        {
            HandleInput();
        }
        if (currDrawingCooldown > 0)
        {
            currDrawingCooldown -= Time.deltaTime;
            cooldownBar.SetHealth(drawingCooldown - currDrawingCooldown);
        }
    }

    /// <summary>
    /// Checks if the player can draw gestures based on the game state and cooldown.
    /// </summary>
    public bool CanDraw
    {
        get
        {
            return isGameOver == false && currDrawingCooldown <= 0;
        }
    }

    /// <summary>
    /// Handles input during the game over state to reset the game.
    /// </summary>
    void HandleInput()
    {
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch)) // A button
        {
            isGameOver = false; // Reset game over state
            enemySpawner.Reset(); // Reset enemy spawner
            canvasMain.enabled = true; // Show main canvas
            canvasGameOver.enabled = false; // Hide game over canvas
        }
    }

    /// <summary>
    /// Triggers the game over state, updates the UI, and removes all active entities.
    /// </summary>
    public void GameOver()
    {
        if (isGameOver) return; // Prevent multiple game over calls
        isGameOver = true;
        gameOverScoreText.text = enemySpawner.currentWave.ToString(); // Display score
        canvasMain.enabled = false; // Hide main canvas
        canvasGameOver.enabled = true; // Show game over canvas
        guardSpawner.RemoveAllGuards(); // Remove all guards
        enemySpawner.RemoveAllEnemies(); // Remove all enemies
    }

    /// <summary>
    /// Handles gesture input, validates it, and spawns guards based on the result.
    /// </summary>
    /// <param name="gesture">The gesture input to validate.</param>
    public void HandleGestureInput(string gesture)
    {
        // currDrawingCooldown = drawingCooldown; // done in GestureClassifier.cs
        bool isValidGesture = gesture == drawingSelector.CurrentDrawing;
        if (isValidGesture)
        {
            for (int i = 0; i < 3; i++)
            {
                guardSpawner.PlaceGuard(targetBase.position);
            }
        }
        else
        {
            guardSpawner.PlaceGuard(targetBase.position);
        }
        drawingSelector.NextDrawing(isValidGesture);
    }
}