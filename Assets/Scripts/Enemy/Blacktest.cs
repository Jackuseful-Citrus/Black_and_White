using UnityEngine;

/// <summary>
/// 平台小怪 Black：
/// 1. 看不到玩家 → 在当前平台内巡逻（以 Enemy.patrolCenter 为中心，Enemy.patrolRadius 为左右最远点）
/// 2. 看到玩家（同层/高度差不大）→ 只按水平方向匀速追击，不改 y 方向
/// 3. 距离足够近时停下并近战攻击
/// 4. 无论是巡逻还是追击，都不会自己从平台边缘走下去（使用 HasGroundAhead 判边）
/// </summary>
public class Blacktest : Enemy
{
    [Header("近战攻击设置")]
    [SerializeField] private float attackRange = 1.0f;           // 水平攻击距离
    [SerializeField] private float attackCooldown = 1.0f;        // 攻击冷却
    [SerializeField] private float attackHeightTolerance = 0.8f; // 攻击时允许的高度差（竖直方向）

    [Header("水平检测设置")]
    [SerializeField] private float detectHeightTolerance = 1.0f; // 只在这个高度差内才“看到玩家”

    [Header("平台边缘检测")]
    [SerializeField] private LayerMask groundMask;               // 地面/平台的 Layer
    [SerializeField] private float groundCheckExtraHeight = 0.1f;// 射线额外长度
    [SerializeField] private float ledgeCheckDistance = 0.2f;    // 向前探出的水平距离

    

    private Collider2D bodyCollider;                             // 自己的碰撞体（拿 bounds 用）
    private int patrolDirection = 1;                             // 巡逻方向：1 右 / -1 左
    private float lastAttackTime = -999f;                        // 上一次攻击时间

    protected override void Start()
    {
        base.Start(); // Enemy.Start() 里会初始化 rb、patrolCenter 等
        bodyCollider = GetComponent<Collider2D>();
    }

    /// <summary>
    protected override void Update()
    {
        if (isDead) return;

        if (player == null || !player.gameObject.activeInHierarchy)
        {
            FindPlayer();
        }

        if (player != null && Vector2.Distance(transform.position, player.position) <= playerDetectionRange)
        {
            EnemyBehavior();
        }
        else if (enablePatrol)
        {
            Patrol();
        }
        else if (rb != null)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }

    /// <summary>
    /// 玩家在 Enemy.detectionRange 内时的行为逻辑。
    /// </summary>
    private void EnemyBehavior()
    {
        if (isDead || player == null || rb == null) return;

        float dx = player.position.x - transform.position.x;
        float absDx = Mathf.Abs(dx);
        float absDy = Mathf.Abs(player.position.y - transform.position.y);

        // 1）高度差过滤：高差太大就当没看到玩家，继续按“没看到”的逻辑（巡逻/站着）
        if (absDy > detectHeightTolerance)
        {
            if (enablePatrol)
            {
                Patrol(); // 用我们重写的 Patrol，在平台上走
            }
            else
            {
                // 不巡逻就原地站着（只保留竖直速度）
                rb.velocity = new Vector2(0f, rb.velocity.y);
            }
            return;
        }

        // 2）高度差在允许范围内：才算“同一层玩家”，开始处理朝向 + 追击/攻击

        // 翻转朝向：只改 localScale.x，不改 rotation
        if (absDx > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(dx) * Mathf.Abs(scale.x); // 玩家在右边就面向右
            transform.localScale = scale;
        }

        // 3）攻击判定：要求水平距离在 attackRange 内，且高差在 attackHeightTolerance 内
        if (absDx <= attackRange && absDy <= attackHeightTolerance)
        {
            // 停水平速度，只保留竖直方向（例如被击飞时仍可下落）
            rb.velocity = new Vector2(0f, rb.velocity.y);
            TryAttack();
        }
        else
        {
            // 4）否则：只按水平匀速追击，不改 y；但不能走出平台边缘
            float moveDir = Mathf.Sign(dx); // 玩家在右边 = 1，在左边 = -1

            // 前方没有地面了就停下，不再往前走
            if (!HasGroundAhead(moveDir))
            {
                rb.velocity = new Vector2(0f, rb.velocity.y);
                return;
            }

            rb.velocity = new Vector2(moveDir * moveSpeed, rb.velocity.y);
        }
    }

    /// <summary>
    /// 看不到玩家（或高度差太大时）调用的巡逻逻辑：
    /// 在 patrolCenter.x 左右 patrolRadius 范围内来回走，不掉下平台。
    /// </summary>
    protected override void Patrol()
    {
        if (isDead || rb == null) return;

        float leftLimit = patrolCenter.x - patrolRadius;
        float rightLimit = patrolCenter.x + patrolRadius;
        float x = transform.position.x;

        // 到达巡逻边界就掉头
        if (x >= rightLimit)
        {
            patrolDirection = -1;
        }
        else if (x <= leftLimit)
        {
            patrolDirection = 1;
        }

        // 如果前方没有地面（平台边缘），也要掉头
        if (!HasGroundAhead(patrolDirection))
        {
            patrolDirection *= -1;
        }

        // 匀速水平走，不改 y
        rb.velocity = new Vector2(patrolDirection * patrolSpeed, rb.velocity.y);

        // 巡逻时顺便翻转朝向
        if (patrolDirection != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(patrolDirection) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    /// <summary>
    /// 尝试近战攻击（做冷却判断 + 调用伤害逻辑）
    /// </summary>
    private void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;

        // 你项目里的受伤逻辑（沿用原来 BlackEnemy 的写法）
        if (LogicScript.Instance != null)
        {
            LogicScript.Instance.HitByBlackEnemy();
        }

        // TODO: 如果之后要加攻击动画，可以在这里触发 Animator
    }

    /// <summary>
    /// 检查前方 moveDirX 方向是否还有地面（防止小怪走出平台边缘）
    /// </summary>
    private bool HasGroundAhead(float moveDirX)
    {
        // 不移动 / 没有 collider 时，直接认为有地面（避免乱停）
        if (bodyCollider == null || Mathf.Abs(moveDirX) < 0.01f)
            return true;

        Bounds bounds = bodyCollider.bounds;

        // 根据方向取“前脚”：向右走用 bounds.max.x，向左走用 bounds.min.x
        float x = moveDirX > 0 ? bounds.max.x : bounds.min.x;

        // 射线起点：前脚稍微往前一点点，再稍微往上避免在边缘卡住
        Vector2 origin = new Vector2(x + moveDirX * ledgeCheckDistance, bounds.min.y + 0.05f);
        float rayLength = groundCheckExtraHeight + 0.05f;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, groundMask);

#if UNITY_EDITOR
        Debug.DrawRay(origin, Vector2.down * rayLength, hit.collider ? Color.yellow : Color.magenta);
#endif

        return hit.collider != null;
    }

    /// <summary>
    /// 在 Scene 视图里画出攻击范围（红圈），方便调试。
    /// Enemy 基类已经会画 detectionRange / 巡逻范围，我们在此基础上再画一层。
    /// </summary>
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
