using System.Collections;
using UnityEngine;

public class MirrorAnimationSync : MonoBehaviour
{
    [Header("主角状态源")]
    public PlayerControl mainPlayer;
    private Rigidbody2D mainRb;

    [Header("镜像 Animator")]
    public Animator blackAnimator;
    public Animator whiteAnimator;

    [Header("镜像 Outlook 物体")]
    public GameObject blackOutlookGO;
    public GameObject whiteOutlookGO;

    [Header("切换动画前后等待时长（与主角动画节奏匹配）")]
    public float switchPreDuration = 0.8f;   // 与主角协程前半段对应
    public float switchPostDuration = 0.5f;  // 与主角协程后半段对应

    private static readonly int IsWalkingHash     = Animator.StringToHash("IsWalking");
    private static readonly int SwitchToWhiteHash = Animator.StringToHash("SwitchToWhite");
    private static readonly int SwitchToBlackHash = Animator.StringToHash("SwitchToBlack");

    private bool currentMirrorIsWhite;
    private bool isSwitching = false;

    private void Start()
    {
        if (mainPlayer != null)
        {
            mainRb = mainPlayer.GetComponent<Rigidbody2D>();
            mainPlayer.OnSwitchStart += HandleMainSwitchStart;
        }

        currentMirrorIsWhite = ComputeMirrorIsWhiteFromMain();
        ApplyOutlookActive(currentMirrorIsWhite);
    }

    private void OnDestroy()
    {
        if (mainPlayer != null)
        {
            mainPlayer.OnSwitchStart -= HandleMainSwitchStart;
        }
    }

    private bool ComputeMirrorIsWhiteFromMain()
    {
        if (mainPlayer == null) return false;
        // 镜像颜色永远取主角的反色
        return !mainPlayer.isWhite;
    }

    private void ApplyOutlookActive(bool mirrorIsWhite)
    {
        if (blackOutlookGO != null)
            blackOutlookGO.SetActive(!mirrorIsWhite);
        if (whiteOutlookGO != null)
            whiteOutlookGO.SetActive(mirrorIsWhite);
    }

    private void Update()
    {
        if (mainPlayer == null || mainRb == null) return;

        // 同步行走动画（用主角的速度）
        bool isMoving = Mathf.Abs(mainRb.velocity.x) > 0.01f;

        if (currentMirrorIsWhite)
        {
            if (whiteAnimator != null)
                whiteAnimator.SetBool(IsWalkingHash, isMoving);
            if (blackAnimator != null)
                blackAnimator.SetBool(IsWalkingHash, false);
        }
        else
        {
            if (blackAnimator != null)
                blackAnimator.SetBool(IsWalkingHash, isMoving);
            if (whiteAnimator != null)
                whiteAnimator.SetBool(IsWalkingHash, false);
        }
    }

    private void HandleMainSwitchStart(bool mainToWhite)
    {
        bool mirrorToWhite = !mainToWhite;
        if (isSwitching) return;
        StartCoroutine(PlaySwitchCoroutine(mirrorToWhite));
    }

    private IEnumerator PlaySwitchCoroutine(bool nextMirrorIsWhite)
    {
        isSwitching = true;

        // 触发对应的切换动画（只让当前形态动画播放，避免另一形态提前显现）
        if (nextMirrorIsWhite)
        {
            // 镜像从黑 -> 白：在黑 Animator 上打 SwitchToWhite
            if (blackAnimator != null)
            {
                blackAnimator.ResetTrigger(SwitchToWhiteHash);
                blackAnimator.SetTrigger(SwitchToWhiteHash);
            }
        }
        else
        {
            // 镜像从白 -> 黑：在白 Animator 上打 SwitchToBlack
            if (whiteAnimator != null)
            {
                whiteAnimator.ResetTrigger(SwitchToBlackHash);
                whiteAnimator.SetTrigger(SwitchToBlackHash);
            }
        }

        // 前半段：保持当前 Outlook，避免目标形态提前露出
        yield return new WaitForSeconds(switchPreDuration);

        // 真正切形态
        currentMirrorIsWhite = nextMirrorIsWhite;
        ApplyOutlookActive(currentMirrorIsWhite);

        // 后半段：给新形态一点时间完成切换动画
        yield return new WaitForSeconds(switchPostDuration);

        isSwitching = false;
    }
}
