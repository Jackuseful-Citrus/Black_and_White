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

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    private void Awake()
    {
        player = GetComponent<PlayerControl>();
        rb     = GetComponent<Rigidbody2D>();

        // 自动从 Outlook 上找 Animator，防止没拖
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

        if (player.isWhite)
        {
            // 只控制「白形态」
            if (whiteAnimator != null &&
                whiteAnimator.runtimeAnimatorController != null &&
                whiteAnimator.gameObject.activeInHierarchy)
            {
                whiteAnimator.SetBool(IsWalkingHash, isMoving);
            }
        }
        else
        {
            // 只控制「黑形态」
            if (blackAnimator != null &&
                blackAnimator.runtimeAnimatorController != null &&
                blackAnimator.gameObject.activeInHierarchy)
            {
                blackAnimator.SetBool(IsWalkingHash, isMoving);
            }
        }
    }
}
