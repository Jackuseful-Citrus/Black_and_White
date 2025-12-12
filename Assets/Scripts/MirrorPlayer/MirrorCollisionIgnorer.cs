using UnityEngine;

/// <summary>
/// 让主角与镜像彼此不发生碰撞。挂在任意物体（例如镜像根节点）上即可。
/// 基于 Tag 查找：Player 与 Mirrorplayer。
/// </summary>
public class MirrorCollisionIgnorer : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string mirrorTag = "Mirrorplayer";

    private void Awake()
    {
        TryIgnoreCollisions();
    }

    private void OnEnable()
    {
        TryIgnoreCollisions();
    }

    private void TryIgnoreCollisions()
    {
        var player = GameObject.FindGameObjectWithTag(playerTag);
        var mirror = GameObject.FindGameObjectWithTag(mirrorTag);
        if (player == null || mirror == null) return;

        var playerCols = player.GetComponentsInChildren<Collider2D>();
        var mirrorCols = mirror.GetComponentsInChildren<Collider2D>();

        foreach (var pc in playerCols)
        {
            if (pc == null || !pc.enabled) continue;
            foreach (var mc in mirrorCols)
            {
                if (mc == null || !mc.enabled) continue;
                Physics2D.IgnoreCollision(pc, mc, true);
            }
        }
    }
}
