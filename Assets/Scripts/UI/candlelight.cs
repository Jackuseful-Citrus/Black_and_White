using UnityEngine;
using UnityEngine.Rendering.Universal; // 导入 URP 命名空间以访问 Light 2D 组件

public class CandleLight : MonoBehaviour
{
    // 对 Light 2D 组件的引用
    private Light2D light2D;

    [Header("强度闪烁设置 (Intensity)")]
    [Tooltip("光的最小强度（例如：4.0）")]
    public float minIntensity = 4.0f;
    [Tooltip("光的最大强度（例如：5.0）")]
    public float maxIntensity = 5.0f;
    [Tooltip("强度变化的速度")]
    public float intensitySpeed = 10f;

    [Header("半径闪烁设置 (Radius)")]
    [Tooltip("光的最小半径（例如：0.6）")]
    public float minRadius = 0.6f;
    [Tooltip("光的最大半径（例如：0.7）")]
    public float maxRadius = 0.7f;
    [Tooltip("半径变化的速度")]
    public float radiusSpeed = 8f;

    // 用于在 Perlin Noise 中采样的偏移量
    private float timeOffset;


    void Start()
    {
        // 尝试获取 Light 2D 组件
        light2D = GetComponent<Light2D>();

        if (light2D == null)
        {
            Debug.LogError("CandleFlicker 脚本需要一个 Light2D 组件才能工作！");
            enabled = false;
            return;
        }

        // 初始化一个随机时间偏移量，确保多个烛光不会同步闪烁
        timeOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        // 1. 强度 (Intensity) 闪烁
        // 使用 Perlin Noise 函数获取平滑的随机值
        // (Time.time * intensitySpeed) 确保随时间变化
        float intensityNoise = Mathf.PerlinNoise(Time.time * intensitySpeed, timeOffset);

        // 将 0 到 1 的 Noise 值映射到 minIntensity 到 maxIntensity 范围
        light2D.intensity = Mathf.Lerp(minIntensity, maxIntensity, intensityNoise);


        // 2. 半径 (Radius) 闪烁
        // 重新计算另一个 Perlin Noise 值，最好使用不同的时间或偏移量，防止与强度完全同步
        float radiusNoise = Mathf.PerlinNoise(Time.time * radiusSpeed, timeOffset + 100f);
        
        // 将 0 到 1 的 Noise 值映射到 minRadius 到 maxRadius 范围
        light2D.pointLightOuterRadius = Mathf.Lerp(minRadius, maxRadius, radiusNoise);
        // 如果您也需要控制 Inner Radius，则取消注释下面一行并设置范围
        // light2D.pointLightInnerRadius = Mathf.Lerp(minInnerRadius, maxInnerRadius, radiusNoise);
    }
}