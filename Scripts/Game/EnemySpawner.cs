using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro; // For lists

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    public TextMeshProUGUI waveText; // UI Text to display current wave

    public float timeBetweenSpawns = 2f;
    public int enemiesPerWave = 5;
    private int currEnemiesPerWave;
    public float waveCooldown = 10f; // Time between waves
    private float currWaveCooldown;

    public int currentWave = 0;

    private Transform spawnPoint; // Where enemies appear

    private int enemiesSpawnedInWave = 0;
    private bool spawningWave = false;

    public List<Enemy> activeEnemies = new List<Enemy>(); // Track active enemies

    void Start()
    {
        spawnPoint = transform; // Set spawn point to this object's transform
        Reset(); // Initialize the spawner
    }

    public void Reset()
    {
        currentWave = 0; // Reset current wave
        currEnemiesPerWave = enemiesPerWave; // Initialize current enemies per wave
        currWaveCooldown = waveCooldown; // Initialize current wave cooldown
        StartNextWave();
    }

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

    public void RemoveEnemy(Enemy enemy)
    {
        activeEnemies.Remove(enemy);
        Destroy(enemy.gameObject);
    }

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