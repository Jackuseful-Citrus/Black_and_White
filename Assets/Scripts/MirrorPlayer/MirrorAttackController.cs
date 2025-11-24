using UnityEngine;

public class MirrorAttackController : MonoBehaviour
{
    [Header("主角引用")]
    public PlayerControl mainPlayer;     // 真玩家

    [Header("镜像镰刀（黑形态攻击）")]
    public ScytheScript mirrorScythe;    // 镜像身上的 ScytheScript（usePlayerInput = false）

    [Header("镜像光球（白形态攻击）")]
    public GameObject lightBallPrefab;   // LightBall 预制体
    public Transform mirrorLightSpawnPoint;  // 镜像发射光球的位置
    public float lightBallCooldown = 0.35f;  // 镜像光球冷却

    [Header("镜像瞄准点（可直接用主角的 AimPoint）")]
    public GameObject mirrorAimPoint;

    private bool lastAttackPressed = false;
    private float lastLightShootTime = -999f;

    private void Start()
    {
        // 确保镰刀有瞄准点（否则只会指向初始方向）
        if (mirrorScythe != null && mirrorAimPoint != null)
        {
            mirrorScythe.SetAimPoint(mirrorAimPoint);
        }
    }

    private void Update()
    {
        if (mainPlayer == null) return;

        bool attackPressed = mainPlayer.isAttacking;
        bool justPressed   = attackPressed && !lastAttackPressed;

        if (justPressed)
        {
            // 镜像形态 = 主角形态的反相
            bool mirrorIsWhite = !mainPlayer.isWhite;

            if (mirrorIsWhite)
            {
                // 镜像是白 → 用光球攻击
                TryShootLightBall();
            }
            else
            {
                // 镜像是黑 → 用镰刀攻击
                TryScytheAttack();
            }
        }

        lastAttackPressed = attackPressed;
    }

    private void TryScytheAttack()
    {
        if (mirrorScythe == null) return;

        // 直接调用我们之前加的接口（注：ScytheScript 里要有 ForceAttack）
        mirrorScythe.ForceAttack();
    }

    private void TryShootLightBall()
    {
        if (lightBallPrefab == null || mirrorLightSpawnPoint == null)
        {
            Debug.LogWarning("[MirrorAttackController] 缺少 lightBallPrefab 或 mirrorLightSpawnPoint");
            return;
        }

        // 简单冷却，避免一按就狂刷
        if (Time.time - lastLightShootTime < lightBallCooldown)
            return;

        Instantiate(lightBallPrefab, mirrorLightSpawnPoint.position, Quaternion.identity);
        lastLightShootTime = Time.time;
    }
}
