using UnityEngine;

/// <summary>
/// 黑图用的敌人，死亡时向 BlackMapProgressionManager 汇报击杀。
/// </summary>
public class BlackMapEnemy : Enemy
{
    [SerializeField] private BlackMapProgressionManager progressionOverride;

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
