using UnityEngine;

public class BossRoomManager : MonoBehaviour
{
    [Header("主角")]
    public PlayerControl mainPlayer;

    [Header("分身预制体")]
    public GameObject mirrorPlayerPrefab;
    public float mirrorLineY;

    [Header("Boss")]
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;

    private GameObject mirrorInstance;
    private GameObject bossInstance;

    private void Awake()
    {
        // 兜底：如果忘了在 Inspector 里拖 mainPlayer，就自动找场景里的 PlayerControl
        if (mainPlayer == null)
        {
            mainPlayer = FindObjectOfType<PlayerControl>();
            if (mainPlayer == null)
            {
                Debug.LogError("[BossRoomManager] 场景里找不到 PlayerControl，mainPlayer 为空！");
            }
        }
    }

    public void StartBossFight()
    {
        if (mainPlayer == null)
        {
            Debug.LogError("[BossRoomManager] StartBossFight 调用时 mainPlayer 仍然为 null！");
            return;
        }

        if (mirrorPlayerPrefab == null)
        {
            Debug.LogError("[BossRoomManager] mirrorPlayerPrefab 未设置！");
            return;
        }

        // 1. 生成分身（严格对称：x 用主角的 x）
        Vector3 p = mainPlayer.transform.position;
        Vector3 spawnPos = new Vector3(p.x, 2f * mirrorLineY - p.y, p.z);

        mirrorInstance = Instantiate(mirrorPlayerPrefab, spawnPos, Quaternion.identity);
        mirrorInstance.tag = "PlayerMirror";

        // === 配置镜像“物理 + 视觉 + 动画 + 攻击” ===

        // 物理同步（横向速度）
        var phys = mirrorInstance.GetComponent<MirrorPhysicalController>();
        if (phys != null)
        {
            phys.mainPlayer = mainPlayer;
        }
        else
        {
            Debug.LogWarning("[BossRoomManager] mirrorPlayerPrefab 上没有 MirrorPhysicalController（如果你想要物理倒挂，这个脚本要挂上）");
        }

        // 视觉倒挂
        var visual = mirrorInstance.GetComponent<MirrorVisualController>();
        if (visual != null)
        {
            visual.mainPlayer = mainPlayer;
        }
        else
        {
            Debug.LogWarning("[BossRoomManager] mirrorPlayerPrefab 上没有 MirrorVisualController");
        }

        // 动画同步（黑白形态 + 行走）
        var mirrorAnimSync = mirrorInstance.GetComponent<MirrorAnimationSync>();
        if (mirrorAnimSync != null)
        {
            mirrorAnimSync.mainPlayer = mainPlayer;
        }
        else
        {
            Debug.LogWarning("[BossRoomManager] mirrorPlayerPrefab 上没有 MirrorAnimationSync");
        }

        // 攻击同步（如果你已经挂好了 MirrorAttackController）
        var mirrorAttack = mirrorInstance.GetComponent<MirrorAttackController>();
        if (mirrorAttack != null)
        {
            mirrorAttack.mainPlayer = mainPlayer;
        }

        // 2. 生成 Boss 黑球
        if (bossPrefab != null && bossSpawnPoint != null)
        {
            bossInstance = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("[BossRoomManager] bossPrefab 或 bossSpawnPoint 没设置，Boss 不会生成。");
        }
    }
}
