using UnityEngine;

public class BlackMapEnemy : Enemy
{
    [SerializeField] private BlackMapProgressionManager progressionOverride;

    protected override void UpdateTarget()
    {
        // 始终优先锁定玩家（无视颜色）
        if (player != null && player.gameObject.activeInHierarchy)
        {
            float distToPlayer = Vector2.Distance(transform.position, player.position);
            if (distToPlayer <= playerDetectionRange)
            {
                currentTarget = player;
                isProvoked = true;
                return;
            }
        }

        // 否则沿用基类对其他敌人的寻找
        base.UpdateTarget();
    }

    protected override void OnDeath()
    {
        base.OnDeath();

        BlackMapProgressionManager mgr = progressionOverride != null
            ? progressionOverride
            : BlackMapProgressionManager.Instance;

        if (mgr != null)
        {
            mgr.NotifyEnemyKilled();
        }
    }
}
