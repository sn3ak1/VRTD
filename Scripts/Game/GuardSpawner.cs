using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardSpawner : MonoBehaviour
{
    public GameObject guardPrefab;
    public List<Guard> activeGuards = new List<Guard>(); // Track all active guards
    // Start is called before the first frame update
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

    public void RemoveGuard(Guard guard)
    {
        activeGuards.Remove(guard);
        Destroy(guard.gameObject);
    }

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
