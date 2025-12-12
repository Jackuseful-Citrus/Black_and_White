using UnityEngine;

public class MirrorVisualController : MonoBehaviour
{
    [Header("主角引用")]
    public PlayerControl mainPlayer;

    // 可选：单独的视觉根节点
    public Transform visualRoot;

    // 记录初始缩放，避免覆盖原有缩放
    private Vector3 baseScale;

    private void Awake()
    {
        CacheBaseScale();
    }

    private void LateUpdate()
    {
        ApplyMirrorFlip(true);
    }

    private void CacheBaseScale()
    {
        Transform target = visualRoot != null ? visualRoot : transform;
        baseScale = target.localScale;
    }

    private void ApplyMirrorFlip(bool followMainFacing)
    {
        Transform target = visualRoot != null ? visualRoot : transform;

        float dir = 1f;
        if (followMainFacing && mainPlayer != null)
        {
            // 优先使用横向输入（horiz）决定朝向；否则用缩放
            float vx = mainPlayer.horiz;
            if (Mathf.Abs(vx) < 0.01f)
            {
                vx = mainPlayer.transform.localScale.x;
            }
            dir = Mathf.Sign(vx);
            if (dir == 0) dir = 1f;
        }

        target.localScale = new Vector3(
            Mathf.Abs(baseScale.x) * -dir,
            Mathf.Abs(baseScale.y), // 不再翻转 Y
            baseScale.z
        );
    }
}
