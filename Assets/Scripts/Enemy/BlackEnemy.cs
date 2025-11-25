using UnityEngine;

public class BlackEnemy : Enemy
{
    [Header("近战攻击设置")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackCooldown = 1.5f;
    
    [Header("移动设置")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float chaseAcceleration = 8f;
    [SerializeField] private float chaseDeceleration = 15f;
    [SerializeField] private float rotationSpeed = 360f;
    
    private float lastAttackTime;
    private bool isAttacking = false;
    private float attackAnimationDuration = 0.3f;
    
    protected override void Start()
    {
        base.Start();
        lastAttackTime = -attackCooldown;
    }
    
    protected override void EnemyBehavior()
    {
        if (player == null || isDead) return;
        
        float distanceToPlayer = GetDistanceToPlayer();
        
        LookAtPlayerSmooth();
        
        if (distanceToPlayer <= attackRange)
        {
            MaintainPosition();
            TryAttack();
        }
        else if (distanceToPlayer <= attackRange * 1.5f)
        {
            ChasePlayerSlow();
            isAttacking = false;
        }
        else
        {
            ChasePlayer();
            isAttacking = false;
        }
    }
    
    private void ChasePlayer()
    {
        if (isAttacking) return;
        
        Vector2 direction = GetDirectionToPlayer();
        Vector2 targetVelocity = direction * chaseSpeed;
        
        if (rb != null)
        {
            rb.velocity = Vector2.MoveTowards(rb.velocity, targetVelocity, chaseAcceleration * Time.deltaTime);
        }
        else
        {
            transform.Translate(direction * chaseSpeed * Time.deltaTime, Space.World);
        }
    }
    
    private void ChasePlayerSlow()
    {
        if (isAttacking) return;
        
        Vector2 direction = GetDirectionToPlayer();
        Vector2 targetVelocity = direction * (chaseSpeed * 0.5f);
        
        if (rb != null)
        {
            rb.velocity = Vector2.MoveTowards(rb.velocity, targetVelocity, chaseAcceleration * 0.5f * Time.deltaTime);
        }
        else
        {
            transform.Translate(direction * chaseSpeed * 0.5f * Time.deltaTime, Space.World);
        }
    }
    
    private void MaintainPosition()
    {
        if (rb != null && player != null)
        {
            Vector2 direction = GetDirectionToPlayer();
            float distance = GetDistanceToPlayer();
            
            if (distance > attackRange * 0.5f)
            {
                Vector2 targetVelocity = direction * 0.5f;
                rb.velocity = Vector2.MoveTowards(rb.velocity, targetVelocity, chaseDeceleration * Time.deltaTime);
            }
            else
            {
                rb.velocity = Vector2.MoveTowards(rb.velocity, Vector2.zero, chaseDeceleration * Time.deltaTime);
            }
        }
    }
    
    private void StopMoving()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.MoveTowards(rb.velocity, Vector2.zero, chaseDeceleration * Time.deltaTime);
            
            if (rb.velocity.magnitude < 0.01f)
            {
                rb.velocity = Vector2.zero;
            }
        }
    }
    
    private void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown)
        {
            return;
        }
        
        Attack();
    }
    
    private void Attack()
    {
        lastAttackTime = Time.time;
        isAttacking = true;
        
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        
        if (player != null)
        {
            PlayerControl playerControl = player.GetComponent<PlayerControl>();
            
            if (playerControl != null && LogicScript.Instance != null)
            {
                LogicScript.Instance.HitByBlackEnemy();
            }
        }
        
        OnAttack();
        Invoke(nameof(EndAttack), attackAnimationDuration);
    }
    
    private void EndAttack()
    {
        isAttacking = false;
    }
    
    protected virtual void OnAttack()
    {
    }
    
    protected override void OnDamaged()
    {
        base.OnDamaged();
        if (rb != null && player != null)
        {
            Vector2 knockbackDirection = (transform.position - player.position).normalized;
            rb.AddForce(knockbackDirection * 30f, ForceMode2D.Impulse);
        }
    }
    
    protected override void OnDeath()
    {
        base.OnDeath();
        CancelInvoke();
    }
    
    private void LookAtPlayerSmooth()
    {
        if (player == null) return;
        
        Vector2 direction = GetDirectionToPlayer();
        if (direction.magnitude < 0.01f) return;
        
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float currentAngle = transform.eulerAngles.z;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
        
        transform.rotation = Quaternion.Euler(0, 0, newAngle);
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, attackRange);
    }
}
