using UnityEngine;
using UnityEngine.EventSystems;

public class ExitGame : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Shader Material")]
    public Material invertMat;

    [Header("Hover Base")]
    public float hoverRadius = 0.12f;
    public float hoverGrowDuration = 0.15f;

    [Header("Hover Breath (呼吸效果)")]
    public bool enableBreath = true;
    public float breathAmplitude = 0.2f;
    public float breathSpeed = 2.0f;

    [Header("Feather")]
    public float featherOnHover = 0.06f;

    bool isHovering = false;
    RectTransform rect;
    Vector2 baseCenterUV;
    Coroutine hoverRoutine;
    Coroutine growRoutine;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    float EaseOutCubic(float t)
    {
        t = Mathf.Clamp01(t);
        t = 1f - t;
        return 1f - t * t * t;
    }

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
        if (invertMat != null)
            invertMat.SetVector("_Center", uv);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (invertMat == null) return;

        isHovering = true;
        UpdateBaseCenterUV();
        SetCenter(baseCenterUV);
        invertMat.SetFloat("_Feather", featherOnHover);

        if (hoverRoutine != null) StopCoroutine(hoverRoutine);
        if (growRoutine != null) StopCoroutine(growRoutine);
        growRoutine = StartCoroutine(GrowThenHover());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (invertMat == null) return;

        isHovering = false;

        if (hoverRoutine != null) StopCoroutine(hoverRoutine);
        if (growRoutine != null) StopCoroutine(growRoutine);

        float currentR = invertMat.GetFloat("_Radius");
        StartCoroutine(AnimateRadius(currentR, 0f, hoverGrowDuration));
    }

    public void OnExitClicked()
    {
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    System.Collections.IEnumerator GrowThenHover()
    {
        float startR = invertMat.GetFloat("_Radius");
        float endR = hoverRadius;
        float t = 0f;

        while (t < hoverGrowDuration && isHovering)
        {
            t += Time.unscaledDeltaTime;
            float k = EaseOutCubic(t / hoverGrowDuration);
            float r = Mathf.Lerp(startR, endR, k);
            invertMat.SetFloat("_Radius", r);
            yield return null;
        }

        invertMat.SetFloat("_Radius", endR);

        if (isHovering)
        {
            hoverRoutine = StartCoroutine(HoverLoop());
        }
    }

    System.Collections.IEnumerator HoverLoop()
    {
        float baseR = hoverRadius;
        float t = 0f;

        while (isHovering)
        {
            t += Time.unscaledDeltaTime;

            float r = baseR;
            if (enableBreath && breathAmplitude > 0f)
            {
                float s = Mathf.Sin(t * breathSpeed * Mathf.PI * 2f);
                r = baseR * (1f + breathAmplitude * s);
            }

            invertMat.SetFloat("_Radius", r);
            SetCenter(baseCenterUV); // 圆心固定，不抖动

            yield return null;
        }
    }

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
}
