using UnityEngine;

public enum FragmentId
{
    Black = 0,  // 黑图碎片
    White = 1,  // 白图碎片
    Grey = 2   // 灰图碎片
}

/// <summary>
/// 场景终点碎片：
/// - 玩家走进来后按键 / 自动收集
/// - 通知 FragmentCollectionManager：对应碎片已收集，点亮 UI
/// - 黑图可以额外通知 BlackMapProgressionManager 做光照/刷怪
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FragmentEndPoint : MonoBehaviour
{
    [Header("碎片标识（3 张图各用一个）")]
    [SerializeField] private FragmentId fragmentId = FragmentId.Black;

    [Header("黑图流程管理（只有黑图需要，其它图留空）")]
    [SerializeField] private BlackMapProgressionManager progression;

    [Header("拾取方式")]
    [SerializeField] private bool autoCollectOnReach = false;  // true = 一碰就收
    [SerializeField] private KeyCode collectKey = KeyCode.E;   // 需要按键时的按键
    [SerializeField] private GameObject pickupVfx;
    [SerializeField] private bool disableInsteadOfDestroy = true;

    [Header("收集飞行到UI")]
    [SerializeField] private bool flyToUI = true;
    [SerializeField] private float flyDuration = 0.45f;
    [SerializeField] private AnimationCurve flyEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Vector3 uiEndScaleMultiplier = Vector3.one; // 允许微调最终大小
    [SerializeField] private bool rotateToUI = true;
    [SerializeField] private bool scaleToUI = true;


    private bool collected;
    private bool playerInside;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Start()
    {
        // 1. 如果这个碎片已经在别的场景收集过，直接隐藏自己
        var fragMgr = FragmentCollectionManager.Instance;
        if (fragMgr != null && fragMgr.IsCollected(fragmentId))
        {
            collected = true;
            if (disableInsteadOfDestroy) gameObject.SetActive(false);
            else Destroy(gameObject);
            return;
        }

        // 保底：确保是 trigger
        var c2d = GetComponent<Collider2D>();
        if (c2d != null) c2d.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;

        // 黑图的话，第一次走进终点可以通知拉视野之类
        if (progression != null)
        {
            progression.NotifyReachedEnd();
        }

        if (!collected && autoCollectOnReach)
        {
            Collect();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
    }

    private void Update()
    {
        if (collected) return;

        // 手动按键收集
        if (!autoCollectOnReach && playerInside && Input.GetKeyDown(collectKey))
        {
            Collect();
        }
    }

    private void Collect()
    {
        if (collected) return;
        collected = true;

        // 先关掉碰撞，避免重复触发
        var c2d = GetComponent<Collider2D>();
        if (c2d != null) c2d.enabled = false;

        if (flyToUI)
        {
            StartCoroutine(FlyToUIThenCollect());
        }
        else
        {
            FinishCollectImmediate();
        }
    }

    private void FinishCollectImmediate()
    {
        // 1) 点亮 UI
        var fragMgr = FragmentCollectionManager.Instance;
        if (fragMgr != null) fragMgr.MarkCollected(fragmentId);

        // 2) 特效
        if (pickupVfx != null) Instantiate(pickupVfx, transform.position, Quaternion.identity);

        // 3) 黑图额外逻辑
        if (progression != null) progression.NotifyPickupCollected();

        // 4) 隐藏 / 销毁场景碎片
        if (disableInsteadOfDestroy) gameObject.SetActive(false);
        else Destroy(gameObject);
    }
    private System.Collections.IEnumerator FlyToUIThenCollect()
    {
        var fragMgr = FragmentCollectionManager.Instance;
        if (fragMgr == null)
        {
            FinishCollectImmediate();
            yield break;
        }

        RectTransform targetRect = fragMgr.GetIconRect(fragmentId);
        Canvas canvas = fragMgr.GetOrCreateFlyCanvas(); // ✅ 飞行专用 Overlay Canvas

        if (targetRect == null || canvas == null)
        {
            FinishCollectImmediate();
            yield break;
        }

        // --- 取场景碎片外观 ---
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
        {
            FinishCollectImmediate();
            yield break;
        }

        // --- 在 FlyCanvas 上创建临时 UI Image ---
        GameObject flyGO = new GameObject($"FlyFrag_{fragmentId}");
        flyGO.transform.SetParent(canvas.transform, false);

        var img = flyGO.AddComponent<UnityEngine.UI.Image>();
        img.sprite = sr.sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;

        RectTransform flyRect = img.rectTransform;
        flyRect.SetAsLastSibling();

        // FlyCanvas 是 Overlay：UI 相机用 null
        Camera uiCam = null;

        // 取得 FlyCanvas 的 RectTransform
        RectTransform canvasRect = canvas.transform as RectTransform;
        if (canvasRect == null)
        {
            Destroy(flyGO);
            FinishCollectImmediate();
            yield break;
        }

        // --- 起点：世界 -> 屏幕 (用主相机) -> FlyCanvas local ---
        Camera worldCam = Camera.main;
        Vector3 startWorld = sr.bounds.center;
        Vector2 startScreen = worldCam != null
            ? (Vector2)worldCam.WorldToScreenPoint(startWorld)
            : (Vector2)startWorld;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, startScreen, uiCam, out Vector2 startLocal);

        // --- 终点：UI Rect -> 屏幕(Overlay用null) -> FlyCanvas local ---
        // 用 icon 的中心点（世界坐标）
        Vector3 endWorld = targetRect.TransformPoint(targetRect.rect.center);
        Vector2 endScreen = RectTransformUtility.WorldToScreenPoint(uiCam, endWorld);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, endScreen, uiCam, out Vector2 endLocal);

        // 初始位置
        flyRect.anchoredPosition = startLocal;

        // --- 大小：起始用 sprite 像素估算，终点用 icon 的 rect ---
        Vector2 startSize = new Vector2(sr.sprite.rect.width, sr.sprite.rect.height) * 0.5f;
        flyRect.sizeDelta = startSize;

        Vector2 endSize = targetRect.rect.size;
        if (endSize.sqrMagnitude < 1f) endSize = flyRect.sizeDelta;

        // 旋转：起始用碎片朝向，终点用 icon 朝向（一般是 0）
        float startZ = sr.transform.eulerAngles.z;
        float endZ = targetRect.eulerAngles.z;

        float t = 0f;
        float dur = Mathf.Max(0.01f, flyDuration);

        // 隐藏场景碎片，避免重影
        sr.enabled = false;

        while (t < dur)
        {
            float nt = Mathf.Clamp01(t / dur);
            float k = flyEase != null ? flyEase.Evaluate(nt) : nt;

            flyRect.anchoredPosition = Vector2.Lerp(startLocal, endLocal, k);

            if (scaleToUI)
                flyRect.sizeDelta = Vector2.Lerp(startSize, endSize, k);

            if (rotateToUI)
            {
                float z = Mathf.LerpAngle(startZ, endZ, k);
                flyRect.localRotation = Quaternion.Euler(0, 0, z);
            }

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // 定格最终
        flyRect.anchoredPosition = endLocal;

        if (scaleToUI)
        {
            Vector2 mul = new Vector2(uiEndScaleMultiplier.x, uiEndScaleMultiplier.y);
            flyRect.sizeDelta = Vector2.Scale(endSize, mul);
        }

        if (rotateToUI)
            flyRect.localRotation = Quaternion.Euler(0, 0, endZ);

        // 到位：点亮 UI + 其他逻辑
        fragMgr.MarkCollected(fragmentId);

        if (pickupVfx != null) Instantiate(pickupVfx, transform.position, Quaternion.identity);
        if (progression != null) progression.NotifyPickupCollected();

        Destroy(flyGO);

        if (disableInsteadOfDestroy) gameObject.SetActive(false);
        else Destroy(gameObject);
    }



    // 如果以后你需要在“重开关卡”时重置这个碎片，可以调用这个
    public void ResetState()
    {
        collected = false;
        playerInside = false;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }
}
