using UnityEngine;

public class BlackMapEnemy : Enemy
{
    [SerializeField] private BlackMapProgressionManager progressionOverride;

    // 已经写好的：锁玩家
    protected override void UpdateTarget()
    {
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

        base.UpdateTarget();
    }

    // 👉 这里我们改写 EngageTarget，去掉“高度差提前退出”的那段
    protected override void EngageTarget()
    {
        if (currentTarget == null) return;

        float distance = Vector2.Distance(transform.position, currentTarget.position);

        // 不再根据 detectHeightTolerance 提前 return，
        // 只负责朝向，然后交给 HandleMeleeCombat / HandleRangedCombat。
        LookAtTargetSmooth();

        if (attackType == AttackType.Ranged)
        {
            HandleRangedCombat(distance);
        }
        else
        {
            HandleMeleeCombat(distance);
        }
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

