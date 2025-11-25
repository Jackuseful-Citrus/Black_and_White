using Unity.VisualScripting;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [Header("基础属性")]
    [SerializeField] protected float maxHealth = 50f;
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float moveAcceleration = 8f;
    
    [Header("检测设置")]
    [SerializeField] protected float detectionRange = 5f;
    [SerializeField] protected LayerMask playerLayer;
    
    [Header("巡逻设置")]
    [SerializeField] protected bool enablePatrol = true;
    [SerializeField] protected float patrolRadius = 5f;
    [SerializeField] protected float patrolSpeed = 2f;
    [SerializeField] protected float waypointReachDistance = 0.5f;
    
    protected float currentHealth;
    protected Transform player;
    protected Rigidbody2D rb;
    protected bool isDead = false;
    
    protected Vector3 patrolCenter;
    protected Vector3 currentPatrolTarget;
    protected bool hasPatrolTarget = false;
    protected bool wasInCombat = false;   

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        patrolCenter = transform.position;
        FindPlayer();
    }
    
    protected virtual void Update()
    {
        if (isDead) return;
        
        if (player == null)
        {
            FindPlayer();
            return;
        }
        
        if (player.gameObject == null || !player.gameObject.activeInHierarchy)
        {
            player = null;
            FindPlayer();
            return;
        }
        
        Vector2 enemyPos2D = new Vector2(transform.position.x, transform.position.y);
        Vector2 playerPos2D = new Vector2(player.position.x, player.position.y);
        float distanceToPlayer = Vector2.Distance(enemyPos2D, playerPos2D);
        
        if (distanceToPlayer <= detectionRange)
        {
            if (!wasInCombat)
            {
                wasInCombat = true;
                hasPatrolTarget = false;
            }
            
            EnemyBehavior();
        }
        else
        {
            if (wasInCombat)
            {
                wasInCombat = false;
            }
            
            if (enablePatrol)
            {
                Patrol();
            }
            else if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
        }
    }
    
    protected abstract void EnemyBehavior();
    
    protected virtual void Patrol()
    {
        if (!hasPatrolTarget || Vector3.Distance(transform.position, currentPatrolTarget) < waypointReachDistance)
        {
            GenerateNewPatrolTarget();
        }
        
        MoveTowards(currentPatrolTarget, patrolSpeed);
    }
    
    protected virtual void GenerateNewPatrolTarget()
    {
        Vector2 randomDirection = Random.insideUnitCircle * patrolRadius;
        currentPatrolTarget = patrolCenter + new Vector3(randomDirection.x, randomDirection.y, 0);
        hasPatrolTarget = true;
    }
    
    protected virtual void MoveTowards(Vector3 target, float speed)
    {
        Vector2 direction = (target - transform.position).normalized;
        Vector2 targetVelocity = direction * speed;
        
        if (rb != null)
        {
            rb.velocity = Vector2.MoveTowards(rb.velocity, targetVelocity, moveAcceleration * Time.deltaTime);
        }
        else
        {
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }
    }
    
    protected virtual void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }
    
    public virtual void TakeDamage(float damageAmount)
    {
        if (isDead) return;
        
        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);
        
        OnDamaged();
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    protected virtual void OnDamaged()
    {
    }
    
    protected virtual void Die()
    {
        isDead = true;
        
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        
        OnDeath();
        Destroy(gameObject, 0.5f);
    }
    
    protected virtual void OnDeath()
    {
    }
    
    protected Vector2 GetDirectionToPlayer()
    {
        if (player == null)
        {
            return Vector2.zero;
        }
        
        Vector2 enemyPos2D = new Vector2(transform.position.x, transform.position.y);
        Vector2 playerPos2D = new Vector2(player.position.x, player.position.y);
        
        return (playerPos2D - enemyPos2D).normalized;
    }
    
    protected float GetDistanceToPlayer()
    {
        if (player == null)
        {
            return float.MaxValue;
        }
        
        Vector2 enemyPos2D = new Vector2(transform.position.x, transform.position.y);
        Vector2 playerPos2D = new Vector2(player.position.x, player.position.y);
        
        return Vector2.Distance(enemyPos2D, playerPos2D);
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        if (enablePatrol)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Application.isPlaying ? patrolCenter : transform.position, patrolRadius);
        }
    }
}
