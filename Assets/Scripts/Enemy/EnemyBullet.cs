using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public enum BulletType { White, Black }
    public BulletType bulletType = BulletType.White;
    
    private Vector2 direction;
    private float speed;
    private float damage;
    private float damageToEnemy;
    private LayerMask targetEnemyLayer;
    private bool isInitialized = false;
    private GameObject shooter;
    
    [SerializeField] private float lifetime = 5f;
    
    private Rigidbody2D rb;
    private Vector3 startPosition; 
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    public void Initialize(Vector2 moveDirection, float bulletSpeed, float bulletDamage, BulletType type = BulletType.White, GameObject shooterObject = null, float enemyDamage = 0f, LayerMask enemyLayer = default)
    {
        direction = moveDirection.normalized;
        speed = bulletSpeed;
        damage = bulletDamage;
        bulletType = type;
        shooter = shooterObject;
        damageToEnemy = enemyDamage;
        targetEnemyLayer = enemyLayer;
        isInitialized = true;
        startPosition = transform.position;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
        
        if (shooter != null)
        {
            Collider2D bulletCollider = GetComponent<Collider2D>();
            Collider2D shooterCollider = shooter.GetComponent<Collider2D>();
            
            if (bulletCollider != null && shooterCollider != null)
            {
                Physics2D.IgnoreCollision(bulletCollider, shooterCollider, true);
            }
        }
        
        Destroy(gameObject, lifetime);
    }
    
    private void Update()
    {
        if (rb != null && rb.velocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg + 180f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
    
    private void FixedUpdate()
    {
        if (!isInitialized) return;
        
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
    }
    
    private bool HasTag(GameObject obj, string tagName)
    {
        try
        {
            return obj.CompareTag(tagName);
        }
        catch
        {
            return false;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<EnemyBullet>() != null) return;

        if (shooter != null && other.gameObject == shooter)
        {
            return;
        }
        
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerControl player = other.gameObject.GetComponent<PlayerControl>();
        
            //对于player的扣血计算
            if (player != null && LogicScript.Instance != null)
            {
                if (bulletType == BulletType.White)
                {
                    LogicScript.Instance.HitByWhiteEnemy();
                }
                else if (bulletType == BulletType.Black)
                {
                    LogicScript.Instance.HitByBlackEnemy();
                }
            }
            
            Destroy(gameObject);
        }
        else if (targetEnemyLayer != 0 && ((1 << other.gameObject.layer) & targetEnemyLayer) != 0)
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damageToEnemy);
            }
            Destroy(gameObject);
        }
        else if (HasTag(other.gameObject, "Wall") || 
                HasTag(other.gameObject, "Ground") || 
                HasTag(other.gameObject, "Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}