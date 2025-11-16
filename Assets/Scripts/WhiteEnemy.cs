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
    
    private float lastAttackTime;
    private Vector3 lastRecordedPosition;
    private float combatStateStartTime;
    private bool isInCombatTransition;
    
    protected override void Start()
    {
        base.Start();
        lastAttackTime = -attackCooldown;
        lastRecordedPosition = transform.position;
        combatStateStartTime = -combatTransitionTime;
        isInCombatTransition = false;
        
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }
    
    protected override void Update()
    {
        bool wasPreviouslyInCombat = isInCombatTransition;
        base.Update();
        
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
        if (player == null || isDead) return;
        
        float distanceToPlayer = GetDistanceToPlayer();
        
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
        
        LookAtPlayerSmooth();
        
        if (distanceToPlayer > maxDistanceFromPlayer)
        {
            MoveTowardsPlayer(isInCombatTransition ? transitionProgress : 1f);
        }
        else if (distanceToPlayer < minDistanceFromPlayer)
        {
            MoveAwayFromPlayer(isInCombatTransition ? transitionProgress : 1f);
        }
        else
        {
            StopMoving();
        }
        
        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
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
    
    private void MoveTowardsPlayer(float transitionMultiplier = 1f)
    {
        Vector2 direction = GetDirectionToPlayer();
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
    
    private void MoveAwayFromPlayer(float transitionMultiplier = 1f)
    {
        Vector2 direction = -GetDirectionToPlayer();
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
    
    private void LookAtPlayerSmooth()
    {
        if (player == null) return;
        
        Vector2 direction = GetDirectionToPlayer();
        if (direction == Vector2.zero) return;
        
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float currentAngle = transform.eulerAngles.z;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, 0, newAngle);
    }
    
    private void Attack()
    {
        if (bulletPrefab == null || firePoint == null || player == null) return;
        
        lastAttackTime = Time.time;
        
        Vector2 directionToPlayer = ((Vector2)player.position - (Vector2)firePoint.position).normalized;
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        Quaternion bulletRotation = Quaternion.Euler(0, 0, angle);
        
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, bulletRotation);
        if (bullet == null) return;
        
        bullet.SetActive(true);
        
        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(directionToPlayer, bulletSpeed, damage, EnemyBullet.BulletType.White, gameObject);
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
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minDistanceFromPlayer);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxDistanceFromPlayer);
    }
}
