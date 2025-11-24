using UnityEngine;

public class MirrorVisualController : MonoBehaviour
{
    [Header("主角引用")]
    public PlayerControl mainPlayer;

    // 如果你有单独的视觉根节点，就拖它；不填就用自身
    public Transform visualRoot;

    // 记录初始缩放，避免覆盖原有缩放配置
    private Vector3 baseScale;

    private void Awake()
    {
        CacheBaseScale();
        ApplyMirrorFlip(false); // 生成当帧就翻转，避免先出现正向
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
            dir = Mathf.Sign(mainPlayer.transform.localScale.x);
            if (dir == 0) dir = 1f;
        }

        // 保留初始缩放，x 跟随主角朝向，y 永远反转贴天花板
        target.localScale = new Vector3(
            Mathf.Abs(baseScale.x) * dir,
            -Mathf.Abs(baseScale.y),
            baseScale.z
        );
    }
}
