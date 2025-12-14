using UnityEngine;
using UnityEngine.InputSystem;

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
        if (actions == null)
        {
            return;
        }
        actions.Enable();
        actions.Player.Enable();
    }


    private void OnDisable()
    {
        var actions = InputManager.Instance?.PlayerInputActions;
        moveInput = Vector2.zero;
        if (rb != null) rb.velocity = Vector2.zero;
    }


    private void FixedUpdate()
    {
        if (rb == null) return;

        var actions = InputManager.Instance.PlayerInputActions;
        actions.Player.Enable();

        // x 继续用你现在的 Move（A/D 已经正常）
        Vector2 input = actions.Player.Move.ReadValue<Vector2>();

        // y 强制从键盘读（确保 W/S 一定能工作）
        var kb = Keyboard.current;
        if (kb != null)
        {
            float y = 0f;
            if (kb.wKey.isPressed) y += 1f;
            if (kb.sKey.isPressed) y -= 1f;
            input.y = y;
        }

        if (normalizeDiagonal && input.sqrMagnitude > 1e-4f)
            input = input.normalized;

        rb.velocity = input * moveSpeed;
        if (pc != null) pc.horiz = input.x;
    }
}
