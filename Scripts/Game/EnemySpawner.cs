using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro; // For lists

/// <summary>
/// Manages the spawning and removal of enemies in waves.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    /// <summary>
    /// The prefab used to instantiate new enemies.
    /// </summary>
    public GameObject enemyPrefab;

    /// <summary>
    /// UI element to display the current wave number.
    /// </summary>
    public TextMeshProUGUI waveText; // UI Text to display current wave

    /// <summary>
    /// Time interval between enemy spawns within a wave.
    /// </summary>
    public float timeBetweenSpawns = 2f;

    /// <summary>
    /// Number of enemies to spawn in the first wave.
    /// </summary>
    public int enemiesPerWave = 5;
    /// <summary>
    /// Current number of enemies to spawn in the ongoing wave.
    /// </summary>
    private int currEnemiesPerWave;

    /// <summary>
    /// Time interval between waves.
    /// </summary>
    public float waveCooldown = 10f; // Time between waves
    /// <summary>
    /// Current cooldown time before the next wave starts.
    /// </summary>
    private float currWaveCooldown;

    /// <summary>
    /// The current wave number.
    /// </summary>
    public int currentWave = 0;

    /// <summary>
    /// The spawn point where enemies appear.
    /// </summary>
    private Transform spawnPoint; // Where enemies appear

    /// <summary>
    /// Number of enemies spawned in the current wave.
    /// </summary>
    private int enemiesSpawnedInWave = 0;
    /// <summary>
    /// Indicates whether a wave is currently being spawned.
    /// </summary>
    private bool spawningWave = false;

    /// <summary>
    /// List of active enemies currently in the game.
    /// </summary>
    public List<Enemy> activeEnemies = new List<Enemy>(); // Track active enemies

    /// <summary>
    /// Initializes the spawner and starts the first wave.
    /// </summary>
    void Start()
    {
        spawnPoint = transform; // Set spawn point to this object's transform
        Reset(); // Initialize the spawner
    }

    /// <summary>
    /// Resets the spawner to its initial state and starts the first wave.
    /// </summary>
    public void Reset()
    {
        currentWave = 0; // Reset current wave
        currEnemiesPerWave = enemiesPerWave; // Initialize current enemies per wave
        currWaveCooldown = waveCooldown; // Initialize current wave cooldown
        StartNextWave();
    }

    /// <summary>
    /// Updates the spawner's state, including checking for wave completion.
    /// </summary>
    void Update()
    {
        if (GameManager.instance.isGameOver)
        {
            return;
        }
        // Check if all enemies in the current wave are defeated
        if (spawningWave && enemiesSpawnedInWave >= currEnemiesPerWave)//&& activeEnemies.Count == 0)
        {
            spawningWave = false;
            Debug.Log("Wave " + currentWave + " complete! Starting next wave in " + currWaveCooldown + " seconds.");
            Invoke("StartNextWave", currWaveCooldown); // Wait before next wave
        }
    }

    /// <summary>
    /// Starts the next wave of enemies.
    /// </summary>
    void StartNextWave()
    {
        if (GameManager.instance.isGameOver)
        {
            if (activeEnemies.Count > 0)
            {
                RemoveAllEnemies(); // Clear remaining enemies if game is over
            }
            return; // Don't start a new wave if the game is over
        }
        currentWave++;
        enemiesSpawnedInWave = 0;
        spawningWave = true;
        currEnemiesPerWave += 2; // Increase enemies per wave for difficulty
        currWaveCooldown *= 0.95f; // Decrease cooldown for next wave
        Debug.Log("Starting Wave " + currentWave);
        if (waveText != null)
        {
            waveText.text = "Wave: " + currentWave; // Update UI text
        }
        StartCoroutine(SpawnWave());
    }

    /// <summary>
    /// Spawns enemies for the current wave over time.
    /// </summary>
    /// <returns>An enumerator for coroutine execution.</returns>
    IEnumerator SpawnWave()
    {
        if (GameManager.instance.isGameOver)
        {
            yield break; // Exit if the game is over
        }
        for (int i = 0; i < currEnemiesPerWave; i++)
        {
            if (GameManager.instance.isGameOver)
            {
                yield break; // Exit if the game is over
            }
            GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            newEnemy.transform.localScale = new Vector3(10f, 10f, 10f);
            Enemy enemyComponent = newEnemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                activeEnemies.Add(enemyComponent);
            }
            enemiesSpawnedInWave++; // Increment here
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    /// <summary>
    /// Removes a specific enemy from the game and destroys its GameObject.
    /// </summary>
    /// <param name="enemy">The enemy to remove.</param>
    public void RemoveEnemy(Enemy enemy)
    {
        activeEnemies.Remove(enemy);
        Destroy(enemy.gameObject);
    }

    /// <summary>
    /// Removes all active enemies from the game and clears the list.
    /// </summary>
    public void RemoveAllEnemies()
    {
        foreach (Enemy enemy in activeEnemies)
        {
            try
            {
                Destroy(enemy.gameObject);
            }
            catch (MissingReferenceException)
            {
                Debug.LogWarning("Enemy already destroyed.");
            }
        }
        activeEnemies.Clear();
    }
}