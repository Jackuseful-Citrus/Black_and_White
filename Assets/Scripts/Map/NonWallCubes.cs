using UnityEngine;
using System.Collections;

public class NonWallCubes : MonoBehaviour
{
    [Header("透明度设置")]
    [Range(0f, 1f)]
    public float normalAlpha = 1f;     // 正常状态透明度
    [Range(0f, 1f)]
    public float triggerAlpha = 0.3f;  // 触发时透明度
    
    [Header("过渡时间")]
    public float fadeDuration = 0.2f;  // 淡入淡出时间
    
    private Renderer areaRenderer;
    private Material originalMaterial;
    private Color originalColor;
    private bool isTriggered = false;
    
    void Start()
    {
        areaRenderer = GetComponent<Renderer>();
        if (areaRenderer != null)
        {
            // 创建材质的实例，避免修改原始材质
            originalMaterial = areaRenderer.material;
            originalColor = originalMaterial.color;
        }
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 检查进入的物体是否符合条件（可根据需要修改）
        if (collision.gameObject.CompareTag("Player"))
        {
            isTriggered = true;
            ChangeTransparency(triggerAlpha);
        }
    }
    
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isTriggered = false;
            ChangeTransparency(normalAlpha);
        }
    }
    
    void ChangeTransparency(float targetAlpha)
    {
        if (areaRenderer != null)
        {
            Color newColor = areaRenderer.material.color;
            newColor.a = targetAlpha;
            areaRenderer.material.color = newColor;
            
            // 启用透明渲染模式
            if (targetAlpha < 1f)
            {
                areaRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                areaRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                areaRenderer.material.EnableKeyword("_ALPHABLEND_ON");
                areaRenderer.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            else
            {
                // 恢复不透明模式
                areaRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                areaRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                areaRenderer.material.DisableKeyword("_ALPHABLEND_ON");
                areaRenderer.material.renderQueue = -1;
            }
        }
    }
    
    // 可选：使用协程实现平滑过渡
    IEnumerator FadeTransparency(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        Color color = areaRenderer.material.color;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            areaRenderer.material.color = color;
            yield return null;
        }
    }
}
