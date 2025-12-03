using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WhiteBoss : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 6f;
    public float randomBounceAngle = 25f; // small random tweak after each bounce

    [Header("Attack (8-way)")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private float bulletSpeed = 8f;
    [SerializeField] private float bulletDamage = 10f;
    [SerializeField] private EnemyBullet.BulletType bulletType = EnemyBullet.BulletType.Black;
    [SerializeField] private float bulletSpawnOffset = 0.7f;

    [Header("Phase Control")]
    [SerializeField] private float whitePhaseDuration = 5f;
    [SerializeField] private float whitePhaseMaxHealth = 10f;

    [Header("Body (visual)")]
    [SerializeField] private GameObject whiteBody;

    private Rigidbody2D rb;
    private Vector2 moveDir;
    private float nextAttackTime;
    private float currentHealth;
    private float phaseStartTime;
    private bool phaseEnded;
    private bool roarLocked;

    public System.Action onPhaseEnded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Start()
    {
        Vector2 dir = Random.insideUnitCircle;
        if (dir == Vector2.zero) dir = Vector2.right;
        moveDir = dir.normalized;
        rb.velocity = moveDir * speed;

        nextAttackTime = Time.time + attackInterval;
        currentHealth = whitePhaseMaxHealth;
        phaseStartTime = Time.time;

        if (whiteBody != null) whiteBody.SetActive(true);
    }

    private void Update()
    {
        if (phaseEnded || roarLocked) return;

        if (Time.time >= nextAttackTime)
        {
            FireInEightDirections();
            nextAttackTime = Time.time + attackInterval;
        }

        if (whitePhaseDuration > 0f && Time.time - phaseStartTime >= whitePhaseDuration)
        {
            EndPhase();
        }
    }

    private void FixedUpdate()
    {
        if (phaseEnded || roarLocked) return;

        // keep constant speed
        if (moveDir == Vector2.zero) moveDir = Vector2.right;
        rb.velocity = moveDir.normalized * speed;
    }

    private void FireInEightDirections()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("[WhiteBoss] bulletPrefab not set; cannot fire.");
            return;
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector2[] dirs =
        {
            Vector2.up,
            (Vector2.up + Vector2.right).normalized,
            Vector2.right,
            (Vector2.down + Vector2.right).normalized,
            Vector2.down,
            (Vector2.down + Vector2.left).normalized,
            Vector2.left,
            (Vector2.up + Vector2.left).normalized
        };

        foreach (var dir in dirs)
        {
            Vector3 offsetPos = spawnPos + (Vector3)(dir * bulletSpawnOffset);
            GameObject bulletObj = Instantiate(bulletPrefab, offsetPos, Quaternion.identity);
            var enemyBullet = bulletObj.GetComponent<EnemyBullet>();
            if (enemyBullet != null)
            {
                enemyBullet.Initialize(dir, bulletSpeed, bulletDamage, bulletType, gameObject);
            }
            else
            {
                var bulletRb = bulletObj.GetComponent<Rigidbody2D>();
                if (bulletRb != null)
                {
                    bulletRb.velocity = dir * bulletSpeed;
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contactCount == 0) return;

        // reflect off the surface normal to bounce around the scene
        Vector2 normal = collision.GetContact(0).normal;
        moveDir = Vector2.Reflect(moveDir, normal);

        float angle = Random.Range(-randomBounceAngle, randomBounceAngle);
        moveDir = (Quaternion.Euler(0f, 0f, angle) * moveDir).normalized;
    }

    public void TakeDamage(float amount)
    {
        if (phaseEnded) return;

        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            EndPhase();
        }
    }

    public void ForceEndPhase()
    {
        EndPhase();
    }

    private void EndPhase()
    {
        if (phaseEnded) return;
        phaseEnded = true;
        rb.velocity = Vector2.zero;
        onPhaseEnded?.Invoke();
    }

    public void ConfigurePhase(float duration, float maxHealth)
    {
        whitePhaseDuration = duration;
        whitePhaseMaxHealth = maxHealth;
        currentHealth = whitePhaseMaxHealth;
        phaseStartTime = Time.time;
    }

    public void EnterRetreatMode()
    {
        phaseEnded = true;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true; // stop physics collisions during retreat
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    public void SetRoarLock(bool locked)
    {
        roarLocked = locked;
        if (locked && rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }
}
