using UnityEngine;
using UnityEngine.AI; // For NavMeshAgent
using System.Collections.Generic; // For lists

public class Guard : Character
{
    protected override void DieAction()
    {
        GameManager.instance.guardSpawner.RemoveGuard(this);
    }

    protected override void Awake()
    {
        base.Awake();
        GameManager.instance.guardSpawner.activeGuards.Add(this);
        if (targetDefault == null)
        {
            targetDefault = GameManager.instance.targetGuard;
        }
    }

    protected override Character FindNearestTarget()
    {
        return base.FindNearestTarget(GameManager.instance.enemySpawner.activeEnemies);
    }
}

