using UnityEngine;

/// <summary>
/// 覆盖 PlayerControl 的移动：取消重力，WASD 上下左右平移（顶视角测试用）。
/// 挂在与 PlayerControl 同一个物体上即可，不改动原脚本。
/// </summary>
[RequireComponent(typeof(PlayerControl))]
[RequireComponent(typeof(Rigidbody2D))]
[DefaultExecutionOrder(100)] // 确保在原 PlayerControl 之后写入速度
public class BossTestTopDownPlayer : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private bool normalizeDiagonal = true;
    [SerializeField] private bool fallbackToLegacyAxes = true; // 输入系统未配置竖直轴时，使用 Input.GetAxisRaw

    private PlayerControl pc;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        pc = GetComponent<PlayerControl>();
        rb = GetComponent<Rigidbody2D>();

        // 顶视角：去掉重力与旋转
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        if (pc != null)
        {
            pc.topDownMode = true;
        }
    }

    private void OnEnable()
    {
        var actions = InputManager.Instance?.PlayerInputActions;
        if (actions != null)
        {
            actions.Player.Move.performed += OnMovePerformed;
            actions.Player.Move.canceled += OnMoveCanceled;
        }
    }

    private void OnDisable()
    {
        var actions = InputManager.Instance?.PlayerInputActions;
        if (actions != null)
        {
            actions.Player.Move.performed -= OnMovePerformed;
            actions.Player.Move.canceled -= OnMoveCanceled;
        }
        moveInput = Vector2.zero;
        if (rb != null) rb.velocity = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        // 如果没有绑定垂直轴，允许使用旧 Input.GetAxisRaw 兜底
        if (fallbackToLegacyAxes && Mathf.Abs(moveInput.y) < 0.001f)
        {
            float legacyY = Input.GetAxisRaw("Vertical");
            if (Mathf.Abs(legacyY) > 0.001f)
            {
                moveInput.y = legacyY;
            }
        }

        Vector2 v = moveInput;
        if (normalizeDiagonal && v.sqrMagnitude > 1e-4f)
        {
            v = v.normalized;
        }

        rb.velocity = v * moveSpeed;

        // 为了动画/朝向：同步原脚本的 horiz
        if (pc != null)
        {
            pc.horiz = moveInput.x;
        }

        // Debug 日志：查看当前输入与速度（可按需关闭）
        //Debug.Log($"[TopDown] raw:{moveInput} vel:{rb.velocity}");
    }

    private void OnMovePerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }
}
