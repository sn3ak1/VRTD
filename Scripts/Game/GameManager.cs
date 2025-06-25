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

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Singleton pattern
    public int drawingCooldown = 5; // Cooldown time for drawing gestures

    public HealthBarUI cooldownBar; // Reference to the cooldown bar UI
    public DrawingSelector drawingSelector; // Reference to the DrawingSelector script

    public Transform targetBase; // Reference to the base
    public Transform targetGuard; // Reference to the guard, can be set in Inspector

    public GuardSpawner guardSpawner; // Reference to the GuardSpawner script
    public EnemySpawner enemySpawner; // Reference to the EnemySpawner script

    public Canvas canvasMain;
    public Canvas canvasGameOver;

    public TextMeshProUGUI gameOverScoreText; // Text to display score on game over

    public bool isGameOver = false; // Flag to check if the game is over

    [NonSerialized]
    public float currDrawingCooldown = 0;

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

    void Start()
    {
        cooldownBar.SetMaxHealth(drawingCooldown);
    }

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

    public bool CanDraw
    {
        get
        {
            return isGameOver == false && currDrawingCooldown <= 0;
        }
    }

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