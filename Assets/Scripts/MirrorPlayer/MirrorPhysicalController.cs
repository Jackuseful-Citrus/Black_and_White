using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MirrorPhysicalController : MonoBehaviour
{
    [Header("主角引用")]
    public PlayerControl mainPlayer;

    [Tooltip("是否在 Y 轴上也复制速度（true 则一起跳）")]
    public bool copyVerticalVelocity = true;

    private Rigidbody2D rb;
    private Rigidbody2D mainRb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // ★ 防止被 Boss 撞得乱转
        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
    }

    private void FixedUpdate()
    {
        if (mainPlayer == null) return;

        if (mainRb == null)
            mainRb = mainPlayer.GetComponent<Rigidbody2D>();
        if (mainRb == null) return;

        // 完全复制主角刚体的速度
        Vector2 v = mainRb.velocity;

        v.y = copyVerticalVelocity ? -v.y : rb.velocity.y;

        rb.velocity = v;
    }
}
