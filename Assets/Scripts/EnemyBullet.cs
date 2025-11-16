using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public enum BulletType { White, Black }
    public BulletType bulletType = BulletType.White;
    
    private Vector2 direction;
    private float speed;
    private float damage;
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
    }
    
    public void Initialize(Vector2 moveDirection, float bulletSpeed, float bulletDamage, BulletType type = BulletType.White, GameObject shooterObject = null)
    {
        direction = moveDirection.normalized;
        speed = bulletSpeed;
        damage = bulletDamage;
        bulletType = type;
        shooter = shooterObject;
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
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (shooter != null && collision.gameObject == shooter)
        {
            return;
        }
        
        if (collision.CompareTag("Player"))
        {
            PlayerControl player = collision.GetComponent<PlayerControl>();
            BloodControl blood = collision.GetComponent<BloodControl>();
            
            if (player != null && blood != null)
            {
                if (player.isWhite && bulletType == BulletType.White)
                {
                    blood.AddWhiteMinusBlack(damage);
                }
                else if (player.isBlack && bulletType == BulletType.Black)
                {
                    blood.AddBlackMinusWhite(damage);
                }
            }
            
            Destroy(gameObject);
        }
        else if (HasTag(collision.gameObject, "Wall") || 
                 HasTag(collision.gameObject, "Ground") || 
                 HasTag(collision.gameObject, "Obstacle"))
        {
            Destroy(gameObject);
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (shooter != null && collision.gameObject == shooter)
        {
            return;
        }
        
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerControl player = collision.gameObject.GetComponent<PlayerControl>();
            BloodControl blood = collision.gameObject.GetComponent<BloodControl>();
            
            if (player != null && blood != null)
            {
                if (player.isWhite && bulletType == BulletType.White)
                {
                    blood.AddWhiteMinusBlack(damage);
                }
                else if (player.isBlack && bulletType == BulletType.Black)
                {
                    blood.AddBlackMinusWhite(damage);
                }
            }
            
            Destroy(gameObject);
        }
        else if (HasTag(collision.gameObject, "Wall") || 
                 HasTag(collision.gameObject, "Ground") || 
                 HasTag(collision.gameObject, "Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
