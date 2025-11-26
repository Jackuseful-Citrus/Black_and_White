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
    [SerializeField] protected float attackHeightTolerance = 0.8f; // 攻击时允许的高度差

    [Header("平台检测设置")]
    [SerializeField] protected bool canFallOffLedge = false;       // 是否允许掉下平台
    [SerializeField] protected float detectHeightTolerance = 1.0f;
    [SerializeField] protected LayerMask groundMask;               // 地面/平台的 Layer
    [SerializeField] protected float groundCheckExtraHeight = 0.1f;// 射线额外长度
    [SerializeField] protected float ledgeCheckDistance = 0.2f;    // 向前探出的水平距离

    [Header("巡逻设置")]
    [SerializeField] protected bool enablePatrol = true;
    [SerializeField] protected float patrolRadius = 5f;
    [SerializeField] protected float patrolSpeed = 2f;
    [SerializeField] protected float waypointReachDistance = 0.5f;
    
    protected float currentHealth;
    protected Transform player;
    protected PlayerControl playerControl;
    protected Rigidbody2D rb;
    protected Collider2D bodyCollider;
    protected bool isDead = false;
    protected Transform currentTarget;
    protected float lastAttackTime;
    protected bool isAttacking = false;
    
    protected Vector3 patrolCenter;
    protected Vector3 currentPatrolTarget;
    protected bool hasPatrolTarget = false;
    protected bool wasInCombat = false;   
    protected int patrolDirection = 1; // 1: Right, -1: Left

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
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
        
        // 对于近战敌人，增加高度差检查
        if (attackType == AttackType.Melee)
        {
            float absDy = Mathf.Abs(currentTarget.position.y - transform.position.y);
            if (absDy > detectHeightTolerance)
            {
                if (enablePatrol)
                {
                    Patrol();
                }
                else
                {
                    StopMoving();
                }
                return;
            }
        }

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

        float absDx = Mathf.Abs(currentTarget.position.x - transform.position.x);
        float absDy = Mathf.Abs(currentTarget.position.y - transform.position.y);

        // 攻击判定：水平距离在范围内且高度差在允许范围内
        if (absDx <= meleeAttackRange && absDy <= attackHeightTolerance)
        {
            StopMoving();
            if (Time.time >= lastAttackTime + meleeAttackCooldown)
            {
                MeleeAttack();
            }
        }
        else
        {
            // 追击逻辑：只在水平方向移动，且不走出平台
            float dx = currentTarget.position.x - transform.position.x;
            float moveDir = Mathf.Sign(dx);

            if (!HasGroundAhead(moveDir))
            {
                StopMoving();
                return;
            }

            if (rb != null)
            {
                rb.velocity = new Vector2(moveDir * meleeChaseSpeed, rb.velocity.y);
            }
            else
            {
                transform.Translate(Vector3.right * moveDir * meleeChaseSpeed * Time.deltaTime);
            }
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
        if (attackType == AttackType.Melee)
        {
            // 平台巡逻逻辑
            float leftLimit = patrolCenter.x - patrolRadius;
            float rightLimit = patrolCenter.x + patrolRadius;
            float x = transform.position.x;

            if (x >= rightLimit) patrolDirection = -1;
            else if (x <= leftLimit) patrolDirection = 1;

            if (!HasGroundAhead(patrolDirection))
            {
                patrolDirection *= -1;
            }

            if (rb != null)
            {
                rb.velocity = new Vector2(patrolDirection * patrolSpeed, rb.velocity.y);
            }

            if (patrolDirection != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Sign(patrolDirection) * Mathf.Abs(scale.x);
                transform.localScale = scale;

                transform.rotation = Quaternion.identity;
            }
        }
        else
        {
            if (!hasPatrolTarget || Vector3.Distance(transform.position, currentPatrolTarget) < waypointReachDistance)
            {
                GenerateNewPatrolTarget();
            }
            MoveTowards(currentPatrolTarget, patrolSpeed);
        }
    }

    protected bool HasGroundAhead(float moveDirX)
    {
        if (canFallOffLedge) return true;
        if (bodyCollider == null || Mathf.Abs(moveDirX) < 0.01f) return true;

        Bounds bounds = bodyCollider.bounds;
        float x = moveDirX > 0 ? bounds.max.x : bounds.min.x;
        Vector2 origin = new Vector2(x + moveDirX * ledgeCheckDistance, bounds.min.y + 0.05f);
        float rayLength = groundCheckExtraHeight + 0.05f;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, groundMask);
        return hit.collider != null;
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

        if (attackType == AttackType.Melee)
        {
            if (Mathf.Abs(direction.x) > 0.01f)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Sign(direction.x) * Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
            transform.rotation = Quaternion.identity;
        }
        else
        {
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float currentAngle = transform.eulerAngles.z;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }
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

            if (bodyCollider != null)
            {
                Bounds bounds = bodyCollider.bounds;
                float moveDirX = patrolDirection;
                float x = moveDirX > 0 ? bounds.max.x : bounds.min.x;
                Vector2 origin = new Vector2(x + moveDirX * ledgeCheckDistance, bounds.min.y + 0.05f);
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(origin, origin + Vector2.down * (groundCheckExtraHeight + 0.05f));
            }
        }

        if (enablePatrol)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(Application.isPlaying ? patrolCenter : transform.position, patrolRadius);
        }
    }
}
