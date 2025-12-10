using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class InvertCircleTrigger : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Shader Material")]
    public Material invertMat;          // 用 UI_InvertCircle 的材质

    [Header("Scene")]
    public string nextSceneName;        // 点击之后要切的场景名

    [Header("Hover Base")]
    public float hoverRadius = 0.12f;   // 悬停时的“基础半径”
    public float hoverGrowDuration = 0.15f; // 从 0 长到 hoverRadius 的时间

    [Header("Hover Breath (呼吸效果)")]
    public bool enableBreath = true;
    public float breathAmplitude = 0.2f;   // 0~0.5 比较合理
    public float breathSpeed = 2.0f;     

    [Header("Hover Shake (晃动效果)")]
    public bool enableShake = true;
    public float shakeAmplitude = 0.01f;
    [Tooltip("抖动速度")]
    public float shakeSpeed = 6.0f;

    [Header("Click Expansion")]
    public float clickRadius = 1.4f;     
    public float clickDuration = 0.6f;
    public float clickOvershoot = 1.05f;

    [Header("Feather")]
    public float featherOnHover = 0.06f;
    public float featherOnClick = 0.25f;

    bool isHovering = false;
    bool isClickAnimating = false;

    RectTransform rect;
    Vector2 baseCenterUV;        // 按钮中心在屏幕上的 UV
    Coroutine hoverRoutine;
    Coroutine growRoutine;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    // 缓动：头快尾慢，稍微偏“卡通”一点
    float EaseOutCubic(float t)
    {
        t = Mathf.Clamp01(t);
        t = 1f - t;
        return 1f - t * t * t;
    }

    // 获取按钮中心点的屏幕 UV（0~1）
    void UpdateBaseCenterUV()
    {
        Vector3 worldCenter = rect.TransformPoint(rect.rect.center);
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, worldCenter);
        baseCenterUV = new Vector2(
            screenPos.x / Screen.width,
            screenPos.y / Screen.height
        );
    }

    void SetCenter(Vector2 uv)
    {
        invertMat.SetVector("_Center", uv);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (invertMat == null || isClickAnimating) return;

        isHovering = true;
        UpdateBaseCenterUV();
        SetCenter(baseCenterUV);
        invertMat.SetFloat("_Feather", featherOnHover);

        // 停掉旧协程，重新从 0 长到 hoverRadius，再进入呼吸状态
        if (hoverRoutine != null) StopCoroutine(hoverRoutine);
        if (growRoutine != null) StopCoroutine(growRoutine);
        growRoutine = StartCoroutine(GrowThenHover());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (invertMat == null || isClickAnimating) return;

        isHovering = false;

        if (hoverRoutine != null) StopCoroutine(hoverRoutine);
        if (growRoutine != null) StopCoroutine(growRoutine);

        // 半径收回
        float currentR = invertMat.GetFloat("_Radius");
        StartCoroutine(AnimateRadius(currentR, 0f, hoverGrowDuration));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (invertMat == null || isClickAnimating) return;

        isClickAnimating = true;
        isHovering = false;

        if (hoverRoutine != null) StopCoroutine(hoverRoutine);
        if (growRoutine != null) StopCoroutine(growRoutine);

        UpdateBaseCenterUV();
        SetCenter(baseCenterUV);

        StartCoroutine(ClickAndLoadScene());
    }

    // 进入 hover 时：从当前半径长到 hoverRadius，然后进入呼吸循环
    System.Collections.IEnumerator GrowThenHover()
    {
        float startR = invertMat.GetFloat("_Radius");
        float endR = hoverRadius;
        float t = 0f;

        while (t < hoverGrowDuration && isHovering && !isClickAnimating)
        {
            t += Time.unscaledDeltaTime;
            float k = EaseOutCubic(t / hoverGrowDuration);
            float r = Mathf.Lerp(startR, endR, k);
            invertMat.SetFloat("_Radius", r);
            yield return null;
        }

        invertMat.SetFloat("_Radius", endR);

        if (isHovering && !isClickAnimating)
        {
            hoverRoutine = StartCoroutine(HoverLoop());
        }
    }

    // 悬停时的“呼吸 + 微抖动”
    System.Collections.IEnumerator HoverLoop()
    {
        float baseR = hoverRadius;
        float t = 0f;

        while (isHovering && !isClickAnimating)
        {
            t += Time.unscaledDeltaTime;

            // 呼吸：半径在 baseR 周围上下浮动
            float r = baseR;
            if (enableBreath && breathAmplitude > 0f)
            {
                float s = Mathf.Sin(t * breathSpeed * Mathf.PI * 2f);
                r = baseR * (1f + breathAmplitude * s);
            }

            invertMat.SetFloat("_Radius", r);

            // 抖动：在按钮中心附近加一个小偏移
            Vector2 center = baseCenterUV;
            if (enableShake && shakeAmplitude > 0f)
            {
                float nx = (Mathf.PerlinNoise(t * shakeSpeed, 0f) - 0.5f) * 2f;
                float ny = (Mathf.PerlinNoise(0f, t * shakeSpeed) - 0.5f) * 2f;
                center += new Vector2(nx, ny) * shakeAmplitude;
            }

            SetCenter(center);

            yield return null;
        }
    }

    // 通用半径插值（收回用）
    System.Collections.IEnumerator AnimateRadius(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = EaseOutCubic(t / duration);
            float r = Mathf.Lerp(from, to, k);
            invertMat.SetFloat("_Radius", r);
            yield return null;
        }
        invertMat.SetFloat("_Radius", to);
    }

    // 点击后扩散 + overshoot，再切场景
    System.Collections.IEnumerator ClickAndLoadScene()
    {
        float from = invertMat.GetFloat("_Radius");
        float target = clickRadius * Mathf.Max(1f, clickOvershoot);

        float t = 0f;
        while (t < clickDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = EaseOutCubic(t / clickDuration);

            float r = Mathf.Lerp(from, target, k);
            invertMat.SetFloat("_Radius", r);

            float f = Mathf.Lerp(featherOnHover, featherOnClick, k);
            invertMat.SetFloat("_Feather", f);

            // 点击时不抖动：圆心锁在按钮中心
            SetCenter(baseCenterUV);

            yield return null;
        }

        invertMat.SetFloat("_Radius", target);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
