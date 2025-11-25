using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum EnemyColor { White, Black }
    public enum AttackType { Melee, Ranged }

    [Header("敌人类型设置")]
    public EnemyColor enemyColor;
    public AttackType attackType;

    [Header("基础属性")]
    [SerializeField] protected float maxHealth = 50f;
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float moveAcceleration = 8f;
    [SerializeField] protected float rotationSpeed = 360f;
    
    [Header("检测设置")]
    [SerializeField] protected float playerDetectionRange = 5f;
    [SerializeField] protected float enemyDetectionRange = 5f;
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected LayerMask whiteEnemyLayer;
    [SerializeField] protected LayerMask blackEnemyLayer;
    
    [Header("远程攻击设置")]
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected float rangedAttackRange = 8f;
    [SerializeField] protected float rangedAttackCooldown = 2f;
    [SerializeField] protected float bulletSpeed = 10f;
    [SerializeField] protected float minRangedDistance = 5f;
    [SerializeField] protected float maxRangedDistance = 7f;

    [Header("近战攻击设置")]
    [SerializeField] protected float meleeAttackRange = 1.5f;
    [SerializeField] protected float meleeAttackCooldown = 1.5f;
    [SerializeField] protected float meleeChaseSpeed = 4f;
    [SerializeField] protected float attackAnimationDuration = 0.3f;

    [Header("巡逻设置")]
    [SerializeField] protected bool enablePatrol = true;
    [SerializeField] protected float patrolRadius = 5f;
    [SerializeField] protected float patrolSpeed = 2f;
    [SerializeField] protected float waypointReachDistance = 0.5f;
    
    protected float currentHealth;
    protected Transform player;
    protected PlayerControl playerControl;
    protected Rigidbody2D rb;
    protected bool isDead = false;
    protected Transform currentTarget;
    protected float lastAttackTime;
    protected bool isAttacking = false;
    
    protected Vector3 patrolCenter;
    protected Vector3 currentPatrolTarget;
    protected bool hasPatrolTarget = false;
    protected bool wasInCombat = false;   

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        patrolCenter = transform.position;
        lastAttackTime = -999f;
        
        if (firePoint == null) firePoint = transform;
        
        FindPlayer();
    }
    
    protected virtual void Update()
    {
        if (isDead) return;
        
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            FindPlayer();
        }
        
        UpdateTarget();
        
        if (currentTarget != null)
        {
            if (!wasInCombat)
            {
                wasInCombat = true;
                hasPatrolTarget = false;
            }
            
            EngageTarget();
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
                StopMoving();
            }
        }
    }

    protected virtual void UpdateTarget()
    {
        if (player != null && player.gameObject.activeInHierarchy)
        {
            float distToPlayer = Vector2.Distance(transform.position, player.position);
            if (distToPlayer <= playerDetectionRange)
            {
                if (playerControl != null)
                {
                    bool isHostile = false;
                    if (enemyColor == EnemyColor.White && playerControl.isBlack) isHostile = true;
                    else if (enemyColor == EnemyColor.Black && playerControl.isWhite) isHostile = true;

                    if (isHostile)
                    {
                        currentTarget = player;
                        return;
                    }
                }
                else
                {
                    currentTarget = player;
                    return;
                }
            }
        }

        LayerMask targetLayer = (enemyColor == EnemyColor.White) ? blackEnemyLayer : whiteEnemyLayer;
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, enemyDetectionRange, targetLayer);
        
        Transform nearest = null;
        float minDistance = float.MaxValue;

        foreach (var enemyCollider in enemies)
        {
            if (enemyCollider.gameObject == gameObject) continue;
            if (!enemyCollider.gameObject.activeInHierarchy) continue;

            Enemy enemyScript = enemyCollider.GetComponent<Enemy>();
            if (enemyScript != null && enemyScript.IsDead()) continue;

            float dist = Vector2.Distance(transform.position, enemyCollider.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = enemyCollider.transform;
            }
        }

        currentTarget = nearest;
    }
    
    protected virtual void EngageTarget()
    {
        if (currentTarget == null) return;

        float distance = Vector2.Distance(transform.position, currentTarget.position);
        LookAtTargetSmooth();

        if (attackType == AttackType.Ranged)
        {
            HandleRangedCombat(distance);
        }
        else
        {
            HandleMeleeCombat(distance);
        }
    }

    protected void HandleRangedCombat(float distance)
    {
        if (distance > maxRangedDistance)
        {
            MoveTowards(currentTarget.position, moveSpeed);
        }
        else if (distance < minRangedDistance)
        {
            Vector2 direction = (transform.position - currentTarget.position).normalized;
            Vector2 targetPos = transform.position + (Vector3)direction;
            MoveTowards(targetPos, moveSpeed);
        }
        else
        {
            StopMoving();
        }

        if (distance <= rangedAttackRange && Time.time >= lastAttackTime + rangedAttackCooldown)
        {
            RangedAttack();
        }
    }

    protected void HandleMeleeCombat(float distance)
    {
        if (isAttacking) return;

        if (distance <= meleeAttackRange)
        {
            StopMoving();
            if (Time.time >= lastAttackTime + meleeAttackCooldown)
            {
                MeleeAttack();
            }
        }
        else
        {
            MoveTowards(currentTarget.position, meleeChaseSpeed);
        }
    }

    protected virtual void RangedAttack()
    {
        if (bulletPrefab == null || firePoint == null) return;

        lastAttackTime = Time.time;
        
        Vector2 direction = (currentTarget.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rotation);
        if (bullet != null)
        {
            bullet.SetActive(true);
            EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
            if (bulletScript != null)
            {
                EnemyBullet.BulletType type = (enemyColor == EnemyColor.White) ? EnemyBullet.BulletType.White : EnemyBullet.BulletType.Black;
                LayerMask targetLayer = (enemyColor == EnemyColor.White) ? blackEnemyLayer : whiteEnemyLayer;
                
                bulletScript.Initialize(direction, bulletSpeed, damage, type, gameObject, damage, targetLayer);
            }
        }
    }

    protected virtual void MeleeAttack()
    {
        lastAttackTime = Time.time;
        isAttacking = true;
        
        if (currentTarget != null)
        {
            if (currentTarget.CompareTag("Player"))
            {
                if (enemyColor == EnemyColor.Black && LogicScript.Instance != null)
                {
                    LogicScript.Instance.HitByBlackEnemy();
                }
            }
            else
            {
                Enemy targetEnemy = currentTarget.GetComponent<Enemy>();
                if (targetEnemy != null)
                {
                    targetEnemy.TakeDamage(damage);
                }
            }
        }

        Invoke(nameof(EndAttack), attackAnimationDuration);
    }

    protected void EndAttack()
    {
        isAttacking = false;
    }

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

    protected void StopMoving()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.MoveTowards(rb.velocity, Vector2.zero, moveAcceleration * Time.deltaTime);
        }
    }

    protected void LookAtTargetSmooth()
    {
        if (currentTarget == null) return;
        
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        if (direction == Vector2.zero) return;
        
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float currentAngle = transform.eulerAngles.z;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, 0, newAngle);
    }
    
    protected virtual void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerControl = playerObject.GetComponent<PlayerControl>();
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

    public bool IsDead()
    {
        return isDead;
    }
    
    protected virtual void OnDamaged()
    {
    }
    
    protected virtual void Die()
    {
        isDead = true;
        if (rb != null) rb.velocity = Vector2.zero;
        OnDeath();
        Destroy(gameObject, 0.5f);
    }
    
    protected virtual void OnDeath()
    {
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyDetectionRange);
        
        if (attackType == AttackType.Ranged)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, rangedAttackRange);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, maxRangedDistance);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, meleeAttackRange);
        }

        if (enablePatrol)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(Application.isPlaying ? patrolCenter : transform.position, patrolRadius);
        }
    }
}
