using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BouncingBoss : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 6f;
    public float randomBounceAngle = 25f; // random tweak applied after each bounce

    [Header("Attack (8-way)")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private float bulletSpeed = 8f;
    [SerializeField] private float bulletDamage = 10f;
    [SerializeField] private EnemyBullet.BulletType bulletType = EnemyBullet.BulletType.Black;
    [SerializeField] private float bulletSpawnOffset = 0.7f; // how far from firePoint to spawn so it doesn't overlap the boss

    [Header("Arena Bounds (optional)")]
    [SerializeField] private BoxCollider2D arenaBounds; // bounding box for movement; strongly recommended

    private Rigidbody2D rb;
    private float nextAttackTime;
    private Vector2 moveDir;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // prevent gravity from yeeting the boss out of view
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Start()
    {
        // pick a random start direction
        Vector2 dir = Random.insideUnitCircle;
        if (dir == Vector2.zero) dir = Vector2.right;
        moveDir = dir.normalized;
        rb.velocity = moveDir * speed;

        nextAttackTime = Time.time + attackInterval;
    }

    private void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            FireInEightDirections();
            nextAttackTime = Time.time + attackInterval;
        }
    }

    private void FixedUpdate()
    {
        MoveAndBounceInsideBounds();
    }

    private void FireInEightDirections()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("[BouncingBoss] bulletPrefab not set; cannot fire.");
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
                // fallback: if prefab is not EnemyBullet, just push it forward
                var bulletRb = bulletObj.GetComponent<Rigidbody2D>();
                if (bulletRb != null)
                {
                    bulletRb.velocity = dir * bulletSpeed;
                }
            }
        }
    }

    private void MoveAndBounceInsideBounds()
    {
        if (arenaBounds == null)
        {
            // fallback: simple free move; keep speed alive
            if (moveDir == Vector2.zero) moveDir = Vector2.right;
            rb.velocity = moveDir.normalized * speed;
            return;
        }

        Bounds b = arenaBounds.bounds;

        // Ensure direction is valid
        if (moveDir.sqrMagnitude < 0.0001f)
        {
            moveDir = Random.insideUnitCircle.normalized;
            if (moveDir == Vector2.zero) moveDir = Vector2.right;
        }

        Vector2 pos = rb.position;
        Vector2 nextPos = pos + moveDir * speed * Time.fixedDeltaTime;
        bool bounced = false;

        if (nextPos.x < b.min.x)
        {
            nextPos.x = b.min.x;
            moveDir.x = Mathf.Abs(moveDir.x);
            bounced = true;
        }
        else if (nextPos.x > b.max.x)
        {
            nextPos.x = b.max.x;
            moveDir.x = -Mathf.Abs(moveDir.x);
            bounced = true;
        }

        if (nextPos.y < b.min.y)
        {
            nextPos.y = b.min.y;
            moveDir.y = Mathf.Abs(moveDir.y);
            bounced = true;
        }
        else if (nextPos.y > b.max.y)
        {
            nextPos.y = b.max.y;
            moveDir.y = -Mathf.Abs(moveDir.y);
            bounced = true;
        }

        if (bounced)
        {
            // randomize slightly to avoid perfect loops
            float angle = Random.Range(-randomBounceAngle, randomBounceAngle);
            moveDir = (Quaternion.Euler(0, 0, angle) * moveDir).normalized;
        }

        rb.MovePosition(nextPos);
        rb.velocity = moveDir * speed;
    }
}
