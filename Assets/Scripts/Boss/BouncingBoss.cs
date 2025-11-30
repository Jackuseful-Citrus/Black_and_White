using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BouncingBoss : MonoBehaviour
{
    private enum BossPhase { White, Black }

    [Header("Movement")]
    public float speed = 6f;
    public float randomBounceAngle = 25f; // random tweak applied after each bounce

    [Header("Attack (white phase 8-way)")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private float bulletSpeed = 8f;
    [SerializeField] private float bulletDamage = 10f;
    [SerializeField] private EnemyBullet.BulletType bulletType = EnemyBullet.BulletType.Black;
    [SerializeField] private float bulletSpawnOffset = 0.7f; // how far from firePoint to spawn so it doesn't overlap the boss

    [Header("Phase Switch")]
    [SerializeField] private float whitePhaseDuration = 5f;
    [SerializeField] private float whitePhaseMaxHealth = 10f;

    [Header("Bodies (visual)")]
    [SerializeField] private GameObject whiteBody;
    [SerializeField] private GameObject blackBody;

    [Header("Arena Bounds (optional)")]
    [SerializeField] private BoxCollider2D arenaBounds; // bounding box for movement; strongly recommended

    [Header("Black Phase (charge)")]
    [SerializeField] private float blackChargeSpeed = 10f;
    [SerializeField] private float blackRestDuration = 3f;
    [SerializeField] private float minChargeDuration = 0.25f;
    [SerializeField] private float maxChargeDuration = 1.5f;

    [Header("Targeting")]
    [SerializeField] private PlayerControl mainPlayer;
    [SerializeField] private Transform mirrorPlayer; // optional: 如果镜像有特殊脚本，这里可以直接拖

    private Rigidbody2D rb;
    private float nextAttackTime;
    private Vector2 moveDir;
    private BossPhase currentPhase = BossPhase.White;
    private float phaseStartTime;
    private float currentHealth;
    private bool isCharging;
    private float chargeTimer;
    private float restTimer;
    private Vector2 chargeDir;
    private Transform cachedMirror;

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
        currentHealth = whitePhaseMaxHealth;
        phaseStartTime = Time.time;
        ApplyBodyVisuals();

        if (mainPlayer == null)
        {
            mainPlayer = FindObjectOfType<PlayerControl>();
        }

        if (mirrorPlayer == null)
        {
            cachedMirror = GameObject.FindWithTag("PlayerMirror")?.transform;
        }
        else
        {
            cachedMirror = mirrorPlayer;
        }
    }

    private void Update()
    {
        if (currentPhase == BossPhase.White && Time.time >= nextAttackTime)
        {
            FireInEightDirections();
            nextAttackTime = Time.time + attackInterval;
        }

        if (currentPhase == BossPhase.White && Time.time - phaseStartTime >= whitePhaseDuration)
        {
            SwitchToBlackPhase();
        }
    }

    private void FixedUpdate()
    {
        if (currentPhase == BossPhase.White)
        {
            MoveAndBounceInsideBounds();
        }
        else
        {
            TickBlackPhase();
        }
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

    private void TickBlackPhase()
    {
        if (isCharging)
        {
            chargeTimer -= Time.fixedDeltaTime;
            Vector2 nextPos = rb.position + chargeDir * blackChargeSpeed * Time.fixedDeltaTime;

            if (arenaBounds != null)
            {
                Bounds b = arenaBounds.bounds;
                Vector2 clamped = new Vector2(
                    Mathf.Clamp(nextPos.x, b.min.x, b.max.x),
                    Mathf.Clamp(nextPos.y, b.min.y, b.max.y));
                bool hitEdge = clamped != nextPos;
                nextPos = clamped;
                if (hitEdge)
                {
                    StopCharge();
                }
            }

            rb.MovePosition(nextPos);
            rb.velocity = chargeDir * blackChargeSpeed;

            if (chargeTimer <= 0f)
            {
                StopCharge();
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
            restTimer -= Time.fixedDeltaTime;
            if (restTimer <= 0f)
            {
                StartCharge();
            }
        }
    }

    private void SwitchToBlackPhase()
    {
        if (currentPhase == BossPhase.Black) return;

        currentPhase = BossPhase.Black;
        rb.velocity = Vector2.zero;
        isCharging = false;
        restTimer = 0f; // start charging immediately
        ApplyBodyVisuals();
    }

    private void StartCharge()
    {
        Vector3 targetPos = GetBlackPlayerPosition();
        Vector2 dir = (targetPos - transform.position).normalized;
        if (dir == Vector2.zero) dir = Vector2.right;

        chargeDir = dir;
        float estimatedTime = Vector2.Distance(transform.position, targetPos) / Mathf.Max(blackChargeSpeed, 0.01f);
        chargeTimer = Mathf.Clamp(estimatedTime, minChargeDuration, maxChargeDuration);
        isCharging = true;
    }

    private void StopCharge()
    {
        isCharging = false;
        rb.velocity = Vector2.zero;
        restTimer = blackRestDuration;
    }

    private Vector3 GetBlackPlayerPosition()
    {
        if (mainPlayer == null)
        {
            mainPlayer = FindObjectOfType<PlayerControl>();
        }

        // 优先瞄准当前“黑形态”的角色：主角为黑 => 打主角；主角为白 => 打镜像（镜像是黑）
        if (mainPlayer != null && mainPlayer.isBlack)
        {
            return mainPlayer.transform.position;
        }

        if (mirrorPlayer == null)
        {
            if (cachedMirror == null)
            {
                cachedMirror = GameObject.FindWithTag("PlayerMirror")?.transform;
            }
        }
        else
        {
            cachedMirror = mirrorPlayer;
        }

        if (cachedMirror != null)
        {
            return cachedMirror.position;
        }

        return transform.position + Vector3.right; // fallback
    }

    private void ApplyBodyVisuals()
    {
        if (whiteBody != null) whiteBody.SetActive(currentPhase == BossPhase.White);
        if (blackBody != null) blackBody.SetActive(currentPhase == BossPhase.Black);
    }

    // Called externally by damage sources
    public void TakeDamage(float amount)
    {
        if (currentPhase != BossPhase.White) return; // only white HP gates the phase change

        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            SwitchToBlackPhase();
        }
    }
}
