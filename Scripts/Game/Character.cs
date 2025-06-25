using UnityEngine;
using UnityEngine.AI; // For NavMeshAgent
using System.Collections.Generic; // For IEnumerable

public abstract class Character : MonoBehaviour
{
    public int health;
    public int damage;
    public float attackRange;
    public float attackCooldown;
    protected float lastAttackTime;

    protected HealthBar healthBar;
    protected NavMeshAgent agent;
    private Animator _animator;

    protected Transform targetDefault; // Default target, can be overridden by derived classes
    protected Character target; // Current target, can be set by derived classes

    protected virtual void Awake()
    {
        //get animator from child object
        _animator = GetComponentInChildren<Animator>();
        if (_animator == null)
        {
            Debug.LogError("Animator not found in children of " + gameObject.name);
        }
        agent = GetComponent<NavMeshAgent>();
        healthBar = GetComponentInChildren<HealthBar>();
        if (healthBar != null)
        {
            healthBar.maxHealth = health;
        }
    }

    protected virtual void Update()
    {
        if (health <= 0) return; // Prevent further actions if dead
        if (target == null || !target.gameObject.activeInHierarchy || target.health <= 0)
        {
            target = FindNearestTarget();
        }
        if (target != null)
        {
            if (IsInRange(target.transform))
            {
                //rotate towards target
                Vector3 direction = (target.transform.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                AttackTarget(target);
            }
            else
            {
                MoveToTarget(target.transform);
            }
        }
        else if (targetDefault != null && !IsInRange(targetDefault))
        {
            MoveToTarget(targetDefault);
        }
        else
        {
            _animator.SetBool("run", false); // Stop running if no target
        }
    }

    public virtual void TakeDamage(int dmg)
    {
        if (health <= 0) return; // Prevent taking damage if already dead
        health -= dmg;
        if (healthBar != null)
        {
            healthBar.TakeDamage(dmg);
        }
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        _animator.SetTrigger("death");
        // Disable the NavMeshAgent to stop movement
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        // Disable the character's collider to prevent further interactions
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        Invoke(nameof(DieAction), 5f);
    }

    protected abstract void DieAction();

    protected virtual void MoveToTarget(Transform target)
    {
        if (health <= 0) return; // Prevent movement if dead
        if (agent != null && target != null)
        {
            agent.SetDestination(target.position);
            if (_animator != null)
            {
                _animator.SetBool("run", true);
            }
        }
    }

    protected virtual bool IsInRange(Transform target)
    {
        return Vector3.Distance(transform.position, target.position) <= attackRange;
    }

    protected virtual void AttackTarget(Character target)
    {
        if (health <= 0 || target.health <= 0) return; // Prevent attacking if dead
        if (Time.time - lastAttackTime > attackCooldown && target != null)
        {
            target.TakeDamage(damage);
            lastAttackTime = Time.time;
            _animator.SetTrigger("attack_1");
        }
    }

    protected abstract Character FindNearestTarget();

    protected Character FindNearestTarget(IEnumerable<Character> targets)
    {
        if (health <= 0) return null; // Prevent finding targets if dead
        Character nearestTarget = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Character target in targets)
        {
            if (target != null && target.health > 0)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                if (distanceToTarget < shortestDistance)
                {
                    shortestDistance = distanceToTarget;
                    nearestTarget = target;
                }
            }
        }

        return nearestTarget;
    }
}
