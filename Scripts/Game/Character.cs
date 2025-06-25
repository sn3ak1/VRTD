using UnityEngine;
using UnityEngine.AI; // For NavMeshAgent
using System.Collections.Generic; // For IEnumerable

/// <summary>
/// Represents a character in the game with health, damage, and movement capabilities.
/// </summary>
public abstract class Character : MonoBehaviour
{
    /// <summary>
    /// The current health of the character.
    /// </summary>
    public int health;

    /// <summary>
    /// The damage dealt by the character during an attack.
    /// </summary>
    public int damage;

    /// <summary>
    /// The range within which the character can attack.
    /// </summary>
    public float attackRange;

    /// <summary>
    /// The cooldown time between consecutive attacks.
    /// </summary>
    public float attackCooldown;

    /// <summary>
    /// The time of the last attack performed by the character.
    /// </summary>
    protected float lastAttackTime;

    /// <summary>
    /// Reference to the character's health bar.
    /// </summary>
    protected HealthBar healthBar;

    /// <summary>
    /// Reference to the NavMeshAgent for movement.
    /// </summary>
    protected NavMeshAgent agent;

    /// <summary>
    /// Reference to the Animator for controlling animations.
    /// </summary>
    private Animator _animator;

    /// <summary>
    /// The default target for the character, can be overridden by derived classes.
    /// </summary>
    protected Transform targetDefault;

    /// <summary>
    /// The current target of the character, can be set by derived classes.
    /// </summary>
    protected Character target;

    /// <summary>
    /// Initializes the character's components such as Animator, NavMeshAgent, and HealthBar.
    /// </summary>
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

    /// <summary>
    /// Updates the character's behavior, including targeting and attacking.
    /// </summary>
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

    /// <summary>
    /// Reduces the character's health by the specified damage amount.
    /// Triggers death logic if health drops to zero or below.
    /// </summary>
    /// <param name="dmg">The amount of damage to apply to the character.</param>
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

    /// <summary>
    /// Handles the character's death logic, including disabling movement and interactions.
    /// </summary>
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

    /// <summary>
    /// Executes additional actions upon the character's death, to be implemented by derived classes.
    /// </summary>
    protected abstract void DieAction();

    /// <summary>
    /// Moves the character towards the specified target.
    /// </summary>
    /// <param name="target">The target to move towards.</param>
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

    /// <summary>
    /// Checks if the specified target is within attack range.
    /// </summary>
    /// <param name="target">The target to check.</param>
    /// <returns>True if the target is within range, otherwise false.</returns>
    protected virtual bool IsInRange(Transform target)
    {
        return Vector3.Distance(transform.position, target.position) <= attackRange;
    }

    /// <summary>
    /// Attacks the specified target if conditions are met.
    /// </summary>
    /// <param name="target">The target to attack.</param>
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

    /// <summary>
    /// Finds the nearest target for the character, to be implemented by derived classes.
    /// </summary>
    /// <returns>The nearest target character.</returns>
    protected abstract Character FindNearestTarget();

    /// <summary>
    /// Finds the nearest target from a list of potential targets.
    /// </summary>
    /// <param name="targets">The list of potential targets.</param>
    /// <returns>The nearest target character.</returns>
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
