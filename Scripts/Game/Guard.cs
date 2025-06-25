using UnityEngine;
using UnityEngine.AI; // For NavMeshAgent
using System.Collections.Generic; // For lists

/// <summary>
/// Represents a guard character in the game that targets enemies and protects the base.
/// </summary>
public class Guard : Character
{
    /// <summary>
    /// Removes the guard from the game when it dies.
    /// </summary>
    protected override void DieAction()
    {
        GameManager.instance.guardSpawner.RemoveGuard(this);
    }

    /// <summary>
    /// Initializes the guard and adds it to the active guards list.
    /// Sets the default target for the guard.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        GameManager.instance.guardSpawner.activeGuards.Add(this);
        if (targetDefault == null)
        {
            targetDefault = GameManager.instance.targetGuard;
        }
    }

    /// <summary>
    /// Finds the nearest enemy to target from the active enemies in the game.
    /// </summary>
    /// <returns>The nearest enemy character.</returns>
    protected override Character FindNearestTarget()
    {
        return base.FindNearestTarget(GameManager.instance.enemySpawner.activeEnemies);
    }
}

