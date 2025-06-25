using UnityEngine;

/// <summary>
/// Represents an enemy character in the game that targets the base and interacts with guards.
/// </summary>
public class Enemy : Character
{
    /// <summary>
    /// Reference to the base's health component for attacking the base.
    /// </summary>
    private BaseHealth _baseHealth;

    /// <summary>
    /// Removes the enemy from the game when it dies.
    /// </summary>
    protected override void DieAction()
    {
        GameManager.instance.enemySpawner.RemoveEnemy(this);
    }

    /// <summary>
    /// Initializes the enemy's default target and references the base's health component.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (targetDefault == null)
        {
            targetDefault = GameManager.instance.targetBase;
        }
        _baseHealth = targetDefault.GetComponent<BaseHealth>();
    }

    /// <summary>
    /// Updates the enemy's behavior, including attacking the base if in range.
    /// </summary>
    protected override void Update()
    {
        base.Update();

        if (IsInRange(targetDefault))
        {
            AttackBase();
        }
    }

    /// <summary>
    /// Attacks the base by reducing its health and triggers the enemy's death logic.
    /// </summary>
    void AttackBase()
    {
        if (_baseHealth != null)
        {
            _baseHealth.TakeDamage(damage);
        }
        DieAction();
    }

    /// <summary>
    /// Finds the nearest guard to target from the active guards in the game.
    /// </summary>
    /// <returns>The nearest guard character.</returns>
    protected override Character FindNearestTarget()
    {
        return base.FindNearestTarget(GameManager.instance.guardSpawner.activeGuards);
    }
}
