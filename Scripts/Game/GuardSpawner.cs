using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the spawning and removal of guards in the game.
/// </summary>
public class GuardSpawner : MonoBehaviour
{
    /// <summary>
    /// The prefab used to instantiate new guards.
    /// </summary>
    public GameObject guardPrefab;

    /// <summary>
    /// List of active guards currently in the game.
    /// </summary>
    public List<Guard> activeGuards = new List<Guard>(); // Track all active guards

    /// <summary>
    /// Places a new guard at the specified position.
    /// </summary>
    /// <param name="placementPoint">The position where the guard will be placed.</param>
    public void PlaceGuard(Vector3 placementPoint)
    {
        if (GameManager.instance.isGameOver)
        {
            Debug.LogWarning("Cannot place guard, game is over.");
            return;
        }
        var guard = Instantiate(guardPrefab, placementPoint, Quaternion.identity);
        guard.transform.localScale = new Vector3(15f, 15f, 15f);
        Guard newGuard = guard.GetComponent<Guard>();
        activeGuards.Add(newGuard);
    }

    /// <summary>
    /// Removes a specific guard from the game and destroys its GameObject.
    /// </summary>
    /// <param name="guard">The guard to remove.</param>
    public void RemoveGuard(Guard guard)
    {
        activeGuards.Remove(guard);
        Destroy(guard.gameObject);
    }

    /// <summary>
    /// Removes all active guards from the game and clears the list.
    /// </summary>
    public void RemoveAllGuards()
    {
        foreach (var guard in activeGuards)
        {
            try
            {
                Destroy(guard.gameObject);
            }
            catch (Exception e)
            {
                // Catching all exceptions to handle cases where the guard might already be destroyed
                Debug.LogWarning($"GuardSpawner: Exception while destroying guard: {e.Message}");
            }
        }
        activeGuards.Clear();
    }
}
