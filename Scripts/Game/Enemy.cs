using UnityEngine;

public class Enemy : Character
{
    private BaseHealth _baseHealth;

    protected override void DieAction()
    {
        GameManager.instance.enemySpawner.RemoveEnemy(this);
    }

    protected override void Awake()
    {
        base.Awake();
        if (targetDefault == null)
        {
            targetDefault = GameManager.instance.targetBase;
        }
        _baseHealth = targetDefault.GetComponent<BaseHealth>();
    }

    protected override void Update()
    {
        base.Update();

        if (IsInRange(targetDefault))
        {
            AttackBase();
        }
    }

    void AttackBase()
    {
        if (_baseHealth != null)
        {
            _baseHealth.TakeDamage(damage);
        }
        DieAction();
    }

    protected override Character FindNearestTarget()
    {
        return base.FindNearestTarget(GameManager.instance.guardSpawner.activeGuards);
    }
}
