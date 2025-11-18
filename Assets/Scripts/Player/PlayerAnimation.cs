using UnityEngine;

[RequireComponent(typeof(PlayerControl))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Animators")]
    [SerializeField] private Animator blackAnimator;
    [SerializeField] private Animator whiteAnimator;

    private PlayerControl player;
    private Rigidbody2D rb;

    // 行走状态
    private static readonly int IsWalkingHash     = Animator.StringToHash("IsWalking");
    // 形态切换 Trigger（注意名字要和 Animator 里的一样）
    private static readonly int SwitchToWhiteHash = Animator.StringToHash("SwitchToWhite");
    private static readonly int SwitchToBlackHash = Animator.StringToHash("SwitchToBlack");

    private void Awake()
    {
        player = GetComponent<PlayerControl>();
        rb     = GetComponent<Rigidbody2D>();

        // 自动从 Outlook 上找 Animator
        if (blackAnimator == null && player.BlackOutlook != null)
            blackAnimator = player.BlackOutlook.GetComponent<Animator>();

        if (whiteAnimator == null && player.WhiteOutlook != null)
            whiteAnimator = player.WhiteOutlook.GetComponent<Animator>();

        if (blackAnimator == null)
            Debug.LogError("AnimCtrl: blackAnimator 为空，请检查 BlackOutlook 上有没有 Animator，并拖到脚本里。");

        if (whiteAnimator == null)
            Debug.LogError("AnimCtrl: whiteAnimator 为空，请检查 WhiteOutlook 上有没有 Animator，并拖到脚本里。");
    }

    private void Update()
    {
        if (rb == null || player == null) return;

        bool isMoving = Mathf.Abs(rb.velocity.x) > 0.01f;
        HandleWalkAnimation(isMoving);
    }

    /// <summary>
    /// 在“开始切换”的那一刻调用
    /// toWhite = true 表示要从黑切到白
    /// </summary>
    public void PlaySwitch(bool toWhite)
    {
        if (toWhite)
        {
            // 当前是黑 -> 要切到白：在黑形态 Animator 上播 BlackToWhite 动画
            if (blackAnimator != null &&
                blackAnimator.isActiveAndEnabled &&
                blackAnimator.runtimeAnimatorController != null)
            {
                blackAnimator.ResetTrigger(SwitchToWhiteHash);
                blackAnimator.SetTrigger(SwitchToWhiteHash);
            }
        }
        else
        {
            // 当前是白 -> 要切到黑：在白形态 Animator 上播 WhiteToBlack 动画
            if (whiteAnimator != null &&
                whiteAnimator.isActiveAndEnabled &&
                whiteAnimator.runtimeAnimatorController != null)
            {
                whiteAnimator.ResetTrigger(SwitchToBlackHash);
                whiteAnimator.SetTrigger(SwitchToBlackHash);
            }
        }
    }

    /// <summary>
    /// 按当前形态给对应 Animator 设置 IsWalking
    /// </summary>
    private void HandleWalkAnimation(bool isMoving)
    {
        if (player.isWhite)
        {
            // 白形态：只让白形态动起来
            if (whiteAnimator != null &&
                whiteAnimator.isActiveAndEnabled &&
                whiteAnimator.runtimeAnimatorController != null)
            {
                whiteAnimator.SetBool(IsWalkingHash, isMoving);
            }

            // 黑形态停住（注意 isActiveAndEnabled，避免 "Animator is not playing..." 警告）
            if (blackAnimator != null &&
                blackAnimator.isActiveAndEnabled &&
                blackAnimator.runtimeAnimatorController != null)
            {
                blackAnimator.SetBool(IsWalkingHash, false);
            }
        }
        else
        {
            // 黑形态：只让黑形态动起来
            if (blackAnimator != null &&
                blackAnimator.isActiveAndEnabled &&
                blackAnimator.runtimeAnimatorController != null)
            {
                blackAnimator.SetBool(IsWalkingHash, isMoving);
            }

            // 白形态停住
            if (whiteAnimator != null &&
                whiteAnimator.isActiveAndEnabled &&
                whiteAnimator.runtimeAnimatorController != null)
            {
                whiteAnimator.SetBool(IsWalkingHash, false);
            }
        }
    }
}
