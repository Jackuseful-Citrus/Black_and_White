using UnityEngine;

/// <summary>
/// Boss Minion that reuses Enemy.cs player interaction logic (damage/provoked/hit player),
/// but uses top-down no-gravity movement + periodic small dash.
/// Prefab needs only: Rigidbody2D + Collider2D + this script (inherits Enemy).
/// </summary>
public class BlackBossMinionEnemy : Enemy
{
    [Header("Minion (Boss Only) - Core")]
    [SerializeField] private bool forceNoGravity = true;
    [SerializeField] private bool alwaysProvoked = true;          // 勾上：无视颜色永远追玩家（boss房一般就是这样）
    [SerializeField] private float retargetInterval = 0.15f;       // 目标刷新频率

    [Header("Minion - Wander (between dashes)")]
    [SerializeField] private float wanderMoveSpeed = 2.0f;               // 非冲刺时移动速度（随意移动）
    [SerializeField] private float moveAccel = 20f;                // 平滑加速度
    [SerializeField] private float wanderChangeInterval = 0.45f;   // 多久换一次随意方向
    [SerializeField, Range(0f, 1f)] private float towardPlayerWeight = 0.7f; // 越大越贴近玩家
    [SerializeField, Range(0f, 1f)] private float strafeWeight = 0.25f;      // 越大越绕圈

    [Header("Minion - Dash Attack")]
    [SerializeField] private float dashSpeed = 8f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 0.9f;
    [SerializeField] private float dashTelegraphPause = 0.05f;
    [SerializeField] private float minDashDistance = 0.25f;

    [Header("Minion - Hit Player Cooldown")]
    [SerializeField] private float hitPlayerCooldown = 0.2f;

    private float nextRetargetTime;
    private float nextDashTime;

    private bool isDashing;
    private float dashEndTime;
    private Vector2 dashDir;

    private float nextAllowedHitTime;

    private Vector2 wanderDir;
    private float nextWanderChangeTime;

    protected override void Start()
    {
        // 复用 Enemy 的初始化（血量、rb、collider、FindPlayer、TakeDamage/Die 体系）
        base.Start();

        // boss房 minion：强制黑色近战（用于命中玩家时触发 HitByBlackEnemy）
        enemyColor = EnemyColor.Black;
        attackType = AttackType.Melee;

        // 顶视角/无重力
        if (rb != null && forceNoGravity)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        if (alwaysProvoked) isProvoked = true;

        nextRetargetTime = Time.time + retargetInterval;
        nextDashTime = Time.time + Random.Range(0.1f, 0.3f);

        wanderDir = Random.insideUnitCircle.normalized;
        nextWanderChangeTime = Time.time + wanderChangeInterval;
    }

    protected override void Update()
    {
        if (isDead) return;

        // 只保留：确保 player 有效 + 更新目标 + 冲刺触发
        if (player == null || !player.gameObject.activeInHierarchy)
            FindPlayer();

        if (Time.time >= nextRetargetTime)
        {
            UpdateTarget(); // 我们 override 了：只追玩家
            nextRetargetTime = Time.time + retargetInterval;
        }

        if (currentTarget != null && !isDashing && Time.time >= nextDashTime)
            BeginDash();
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        if (isDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (isDashing)
        {
            rb.velocity = dashDir * dashSpeed;
            if (Time.time >= dashEndTime)
            {
                isDashing = false;
                rb.velocity = Vector2.zero;
            }
            return;
        }

        // 非冲刺时：随意移动（但大致围绕玩家）
        if (currentTarget != null)
        {
            TickWanderAroundTarget();
            return;
        }

        // 没目标就慢慢停
        rb.velocity = Vector2.MoveTowards(rb.velocity, Vector2.zero, moveAccel * Time.fixedDeltaTime);
    }

    private void TickWanderAroundTarget()
    {
        // 到点换一个“随意但偏向玩家”的方向
        if (Time.time >= nextWanderChangeTime)
        {
            Vector2 toward = Vector2.zero;
            Vector2 to = (Vector2)currentTarget.position - rb.position;
            if (to.sqrMagnitude > 1e-4f) toward = to.normalized;

            Vector2 randomDir = Random.insideUnitCircle.normalized;

            // 让它有一点绕圈：toward 旋转 90°
            Vector2 strafe = (toward.sqrMagnitude > 1e-4f)
                ? new Vector2(-toward.y, toward.x) * (Random.value < 0.5f ? -1f : 1f)
                : Vector2.zero;

            Vector2 mixed =
                toward * towardPlayerWeight +
                randomDir * (1f - towardPlayerWeight) +
                strafe * strafeWeight;

            if (mixed.sqrMagnitude < 1e-4f) mixed = randomDir;
            wanderDir = mixed.normalized;

            nextWanderChangeTime = Time.time + wanderChangeInterval;
        }

        Vector2 targetVel = wanderDir * wanderMoveSpeed;
        rb.velocity = Vector2.MoveTowards(rb.velocity, targetVel, moveAccel * Time.fixedDeltaTime);
    }

    private void BeginDash()
    {
        if (rb == null || currentTarget == null)
        {
            nextDashTime = Time.time + dashCooldown;
            return;
        }

        rb.velocity = Vector2.zero;
        CancelInvoke(nameof(DoDashNow));
        Invoke(nameof(DoDashNow), Mathf.Max(0f, dashTelegraphPause));

        // pause 期间禁止重复触发
        nextDashTime = Time.time + 99999f;
    }

    private void DoDashNow()
    {
        if (isDead || rb == null)
        {
            nextDashTime = Time.time + dashCooldown;
            return;
        }

        Vector2 dir = Vector2.right;

        if (currentTarget != null)
        {
            Vector2 to = (Vector2)currentTarget.position - rb.position;
            if (to.magnitude >= minDashDistance) dir = to.normalized;
            else dir = to.sqrMagnitude > 1e-4f ? to.normalized : Vector2.right;
        }

        dashDir = dir;
        isDashing = true;
        dashEndTime = Time.time + Mathf.Max(0.01f, dashDuration);

        nextDashTime = Time.time + Mathf.Max(0.05f, dashCooldown);
    }

    // 命中玩家：复用 Enemy.cs 的效果（黑色敌人 -> HitByBlackEnemy）
    private void OnCollisionEnter2D(Collision2D collision) => TryHitPlayer(collision.collider);
    private void OnTriggerEnter2D(Collider2D other) => TryHitPlayer(other);

    private void TryHitPlayer(Collider2D other)
    {
        if (isDead || other == null) return;
        if (!other.CompareTag("Player")) return;

        if (Time.time < nextAllowedHitTime) return;
        nextAllowedHitTime = Time.time + hitPlayerCooldown;

        if (enemyColor == EnemyColor.Black && LogicScript.Instance != null)
            LogicScript.Instance.HitByBlackEnemy();
    }

    protected override void Die()
    {
        if (isDead) return;

        isDead = true;
        CancelInvoke();
        if (rb != null) rb.velocity = Vector2.zero;

        // 可选：关掉碰撞，避免“尸体挡路”
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 直接销毁：更符合 boss minion 的一次性逻辑
        Destroy(gameObject, 0.2f);
    }


    // ✅ 只追玩家（provoked / 颜色相克 / 距离），不会找别的 Enemy
    protected override void UpdateTarget()
    {
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            currentTarget = null;
            return;
        }

        if (alwaysProvoked || isProvoked)
        {
            currentTarget = player;
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        if (distToPlayer > playerDetectionRange)
        {
            currentTarget = null;
            return;
        }

        if (playerControl != null)
        {
            bool isHostile = false;
            if (enemyColor == EnemyColor.White && playerControl.isBlack) isHostile = true;
            else if (enemyColor == EnemyColor.Black && playerControl.isWhite) isHostile = true;

            currentTarget = isHostile ? player : null;
        }
        else
        {
            currentTarget = player;
        }
    }

}
