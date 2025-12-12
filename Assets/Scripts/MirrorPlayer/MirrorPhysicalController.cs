using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MirrorPhysicalController : MonoBehaviour
{
    [Header("主角引用")]
    public PlayerControl mainPlayer;

    [Header("对称设置")]
    [Tooltip("镜像轴参考点，留空则使用世界原点")]
    public Transform symmetryPivot;
    [Tooltip("镜像法线，默认 Vector2.right 表示绕竖直线镜像")]
    public Vector2 mirrorNormal = Vector2.right;

    [Header("选项")]
    [Tooltip("是否同步速度（沿镜像轴反射）")]
    public bool copyVelocity = true;

    private Rigidbody2D rb;
    private Rigidbody2D mainRb;
    private Vector2 norm;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // 忽略重力，防止旋转
        rb.gravityScale = 0f;
        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;

        norm = mirrorNormal.sqrMagnitude > 0.0001f ? mirrorNormal.normalized : Vector2.right;
    }

    private void FixedUpdate()
    {
        if (mainPlayer == null) return;

        if (mainRb == null)
            mainRb = mainPlayer.GetComponent<Rigidbody2D>();
        if (mainRb == null) return;

        Vector2 pivot = symmetryPivot != null ? (Vector2)symmetryPivot.position : Vector2.zero;
        Vector2 toMain = (Vector2)mainPlayer.transform.position - pivot;

        // 位置镜像：v' = v - 2(n·v)n
        Vector2 mirroredOffset = toMain - 2f * Vector2.Dot(toMain, norm) * norm;
        Vector2 targetPos = pivot + mirroredOffset;

        rb.MovePosition(targetPos);

        if (copyVelocity)
        {
            Vector2 v = mainRb.velocity;
            Vector2 mirroredVel = v - 2f * Vector2.Dot(v, norm) * norm;
            rb.velocity = mirroredVel;
        }
    }
}
