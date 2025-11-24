using UnityEngine;

public class WhiteEnemy : Enemy
{
    [Header("远程攻击设置")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float bulletSpeed = 10f;
    
    [Header("移动设置")]
    [SerializeField] private float minDistanceFromPlayer = 5f;
    [SerializeField] private float maxDistanceFromPlayer = 7f;
    [SerializeField] private float moveDeceleration = 10f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float combatTransitionTime = 0.3f;
    
    [Header("黑色敌人检测设置")]
    [SerializeField] private float blackEnemyDetectionRange = 6f;
    [SerializeField] private LayerMask blackEnemyLayer;
    [SerializeField] private float damageToBlackEnemy = 10f;
    
    private float lastAttackTime;
    private Vector3 lastRecordedPosition;
    private float combatStateStartTime;
    private bool isInCombatTransition;
    private Transform currentTarget; 
    private bool isTargetingBlackEnemy; 
    
    protected override void Start()
    {
        base.Start();
        lastAttackTime = -attackCooldown;
        lastRecordedPosition = transform.position;
        combatStateStartTime = -combatTransitionTime;
        isInCombatTransition = false;
        currentTarget = null;
        isTargetingBlackEnemy = false;
        
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }
    
    protected override void Update()
    {
        if (isDead) return;

        if (player == null)
        {
            FindPlayer();
        }
        else if (!player.gameObject.activeInHierarchy)
        {
            player = null;
        }

        UpdateCurrentTarget();
        
        bool hasTarget = currentTarget != null;

        bool wasPreviouslyInCombat = isInCombatTransition;
        
        if (hasTarget)
        {
            if (!wasInCombat)
            {
                wasInCombat = true;
                hasPatrolTarget = false;
                if (!isInCombatTransition)
                {
                    combatStateStartTime = Time.time;
                    isInCombatTransition = true;
                }
            }
            
            EnemyBehavior();
        }
        else
        {
            if (wasInCombat)
            {
                wasInCombat = false;
                isInCombatTransition = false;
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
        
        // 更新过渡状态
        if (wasPreviouslyInCombat && !wasInCombat)
        {
            isInCombatTransition = false;
        }
    }
    
    void LateUpdate()
    {
        lastRecordedPosition = transform.position;
    }
    
    protected override void EnemyBehavior()
    {
        if (isDead) return;
        
        if (currentTarget == null) return;
        
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        
        if (!wasInCombat || !isInCombatTransition)
        {
            if (!isInCombatTransition)
            {
                combatStateStartTime = Time.time;
                isInCombatTransition = true;
            }
        }
        
        float transitionProgress = Mathf.Clamp01((Time.time - combatStateStartTime) / combatTransitionTime);
        if (transitionProgress >= 1f)
        {
            isInCombatTransition = false;
        }
        
        LookAtTargetSmooth();
        
        if (distanceToTarget > maxDistanceFromPlayer)
        {
            MoveTowardsTarget(isInCombatTransition ? transitionProgress : 1f);
        }
        else if (distanceToTarget < minDistanceFromPlayer)
        {
            MoveAwayFromTarget(isInCombatTransition ? transitionProgress : 1f);
        }
        else
        {
            StopMoving();
        }
        
        if (distanceToTarget <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            if (bulletPrefab != null)
            {
                Attack();
            }
            else
            {
                lastAttackTime = Time.time;
            }
        }
    }
    
    /// <summary>
    /// 更新当前目标：优先选择玩家，如果没有玩家则选择黑色敌人
    /// </summary>
    private void UpdateCurrentTarget()
    {
        if (player != null && player.gameObject.activeInHierarchy)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= detectionRange)
            {
                // 玩家在范围内，优先攻击玩家
                currentTarget = player;
                isTargetingBlackEnemy = false;
                return;
            }
        }

        Transform nearestBlackEnemy = FindNearestBlackEnemy();
        if (nearestBlackEnemy != null)
        {
            if (currentTarget != nearestBlackEnemy)
            {
                float distance = Vector2.Distance(transform.position, nearestBlackEnemy.position);
                // Debug.Log($"[WhiteEnemy] 检测到黑色敌人: {nearestBlackEnemy.name}（距离: {distance:F2}）");
            }
            currentTarget = nearestBlackEnemy;
            isTargetingBlackEnemy = true;
        }
        else
        {
            // 没有找到任何目标
            currentTarget = null;
            isTargetingBlackEnemy = false;
        }
    }
    
    /// <summary>
    /// 寻找最近的黑色敌人
    /// </summary>
    private Transform FindNearestBlackEnemy()
    {
        Collider2D[] blackEnemies = Physics2D.OverlapCircleAll(transform.position, blackEnemyDetectionRange, blackEnemyLayer);
        
        // if (blackEnemies.Length > 0)
        // {
        //     Debug.Log($"[WhiteEnemy] 检测范围内发现 {blackEnemies.Length} 个黑色敌人层对象");
        // }
        
        Transform nearest = null;
        float minDistance = float.MaxValue;
        int validEnemyCount = 0;
        
        foreach (Collider2D enemyCollider in blackEnemies)
        {
            if (enemyCollider.gameObject == gameObject)
            {
                // Debug.Log("[WhiteEnemy]检测到自己");
                continue;
            }

            if (enemyCollider == null || !enemyCollider.gameObject.activeInHierarchy)
            {
                // Debug.Log("[WhiteEnemy]对象无效或未激活");
                continue;
            }
            
            validEnemyCount++;
            float distance = Vector2.Distance(transform.position, enemyCollider.transform.position);
            // Debug.Log($"[WhiteEnemy] 有效黑色敌人: {enemyCollider.name}（距离: {distance:F2}）");
            
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = enemyCollider.transform;
            }
        }
        
        // if (validEnemyCount == 0 && blackEnemies.Length > 0)
        // {
        //     Debug.LogWarning("[WhiteEnemy] 检测到黑色敌人层对象，但没有有效目标");
        // }
        
        return nearest;
    }
    
    private void MoveTowardsTarget(float transitionMultiplier = 1f)
    {
        if (currentTarget == null) return;
        
        Vector2 direction = GetDirectionToTarget();
        Vector2 targetVelocity = direction * moveSpeed;
        
        if (rb != null)
        {
            float effectiveAcceleration = moveAcceleration * transitionMultiplier;
            rb.velocity = Vector2.MoveTowards(rb.velocity, targetVelocity, effectiveAcceleration * Time.deltaTime);
        }
        else
        {
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
        }
    }
    
    private void MoveAwayFromTarget(float transitionMultiplier = 1f)
    {
        if (currentTarget == null) return;
        
        Vector2 direction = -GetDirectionToTarget();
        Vector2 targetVelocity = direction * moveSpeed;
        
        if (rb != null)
        {
            float effectiveAcceleration = moveAcceleration * transitionMultiplier;
            rb.velocity = Vector2.MoveTowards(rb.velocity, targetVelocity, effectiveAcceleration * Time.deltaTime);
        }
        else
        {
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
        }
    }
    
    private Vector2 GetDirectionToTarget()
    {
        if (currentTarget == null) return Vector2.zero;
        
        Vector2 enemyPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 targetPos = new Vector2(currentTarget.position.x, currentTarget.position.y);
        return (targetPos - enemyPos).normalized;
    }
    
    private void LookAtTargetSmooth()
    {
        if (currentTarget == null) return;
        
        Vector2 direction = GetDirectionToTarget();
        if (direction == Vector2.zero) return;
        
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float currentAngle = transform.eulerAngles.z;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, 0, newAngle);
    }
    
    private void StopMoving()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.MoveTowards(rb.velocity, Vector2.zero, moveDeceleration * Time.deltaTime);
            
            if (rb.velocity.magnitude < 0.01f)
            {
                rb.velocity = Vector2.zero;
            }
        }
    }
    
    private void Attack()
    {
        if (bulletPrefab == null || firePoint == null || currentTarget == null) return;
        
        if (Time.time < lastAttackTime + attackCooldown) return;
        
        lastAttackTime = Time.time;
        
        // string targetType = isTargetingBlackEnemy ? "黑色敌人" : "玩家";
        // Debug.Log($"[WhiteEnemy] 发射子弹! 目标: {targetType} ({currentTarget.name}) 时间: {Time.time}");
        
        Vector2 directionToTarget = ((Vector2)currentTarget.position - (Vector2)firePoint.position).normalized;
        float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        Quaternion bulletRotation = Quaternion.Euler(0, 0, angle);
        
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, bulletRotation);
        if (bullet == null) return;
        
        bullet.SetActive(true);
        
        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(directionToTarget, bulletSpeed, damage, EnemyBullet.BulletType.White, gameObject, damageToBlackEnemy, blackEnemyLayer);
        }
        else
        {
            Destroy(bullet);
        }
    }
    
    protected override void OnDamaged()
    {
        base.OnDamaged();
    }
    
    protected override void OnDeath()
    {
        base.OnDeath();
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // 攻击范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 最小距离
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minDistanceFromPlayer);
        
        // 最大距离
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxDistanceFromPlayer);
        
        // 黑色敌人检测范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, blackEnemyDetectionRange);
    }
}
