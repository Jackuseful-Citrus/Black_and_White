using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 黑图流程控制：击杀驱动视野和玩家光照半径，终点拾取触发全局光与刷怪。
/// </summary>
public class BlackMapProgressionManager : MonoBehaviour
{
    public static BlackMapProgressionManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Light2D playerLight;
    [SerializeField] private Light2D globalLight;

    [Header("Camera")]
    [SerializeField] private float minOrthoSize = 4f;
    [SerializeField] private float maxOrthoSize = 8f;
    [SerializeField] private float sizeStep = 0.75f;
    [SerializeField] private int killsPerStep = 3;
    [SerializeField] private float sizeLerpDuration = 1.2f;

    [Header("Global Lighting")]
    [SerializeField] private float startGlobalLightIntensity = 0.05f;
    [SerializeField] private float pickupGlobalLightIntensity = 1.0f;
    [SerializeField] private float globalLightLerpDuration = 1.2f;
    [Header("Player Light")]
    [SerializeField] private float playerLightIntensity = 0.2f; // 始终保持此值

    [SerializeField] private int killsPerLightRadiusStep = 3;
    [SerializeField] private float lightOuterRadiusStep = 0.5f;
    [SerializeField] private float maxLightOuterRadius = 5f;
    [SerializeField] private float lightRadiusLerpDuration = 0.8f;

    [Header("Pickup Spawn Wave")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private List<Transform> rightSpawnPoints = new List<Transform>();
    [SerializeField] private int spawnCount = 8;
    [SerializeField] private float spawnInterval = 0.25f;

    private int killCount;
    private float targetSize;
    private Coroutine sizeRoutine;
    private Coroutine globalLightRoutine;
    private Coroutine lightRadiusRoutine;
    private bool reachedEnd;
    private bool pickupTriggered;
    private float baseOuterRadius;
    private float baseInnerRadius;
    private float innerOuterRatio = 0.5f;
    private int lightRadiusStepCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (targetCamera == null) targetCamera = Camera.main;

        targetSize = Mathf.Clamp(minOrthoSize, 0.1f, maxOrthoSize);
        if (targetCamera != null)
        {
            targetCamera.orthographicSize = targetSize;
        }

        ApplyLightImmediate(playerLightIntensity);
        CacheLightRadius();
        ApplyGlobalLightImmediate(startGlobalLightIntensity);
    }

    /// <summary>
    /// 敌人死亡时调用：拉升相机视野、提升玩家光照半径。
    /// </summary>
    public void NotifyEnemyKilled()
    {
        killCount++;

        if (targetCamera != null && killsPerStep > 0 && sizeStep > 0f)
        {
            int steps = killCount / killsPerStep;
            float desired = Mathf.Min(minOrthoSize + steps * sizeStep, maxOrthoSize);
            if (desired > targetSize + 0.01f)
            {
                targetSize = desired;
                RestartSizeLerp(targetSize);
            }
        }

        TryIncreaseLightRadius();
    }

    /// <summary>
    /// 到达终点：只拉满视野（玩家光照保持不变，环境光不变）。
    /// </summary>
    public void NotifyReachedEnd()
    {
        if (reachedEnd) return;
        reachedEnd = true;

        if (targetCamera != null)
        {
            targetSize = maxOrthoSize;
            RestartSizeLerp(targetSize);
        }

    }

    /// <summary>
    /// 拾取后：全局光提到满亮，并刷怪。
    /// </summary>
    public void NotifyPickupCollected()
    {
        if (pickupTriggered) return;
        pickupTriggered = true;

        StartGlobalLightLerp(pickupGlobalLightIntensity);
        if (enemyPrefab != null && rightSpawnPoints.Count > 0 && spawnCount > 0)
        {
            StartCoroutine(SpawnWaveFromRight());
        }
    }

    private void RestartSizeLerp(float newTarget)
    {
        if (targetCamera == null) return;
        if (sizeRoutine != null) StopCoroutine(sizeRoutine);
        sizeRoutine = StartCoroutine(LerpCameraSize(newTarget));
    }

    private IEnumerator LerpCameraSize(float newTarget)
    {
        float start = targetCamera.orthographicSize;
        float duration = Mathf.Max(0.05f, sizeLerpDuration);
        float timer = 0f;

        while (timer < duration)
        {
            float t = Mathf.Clamp01(timer / duration);
            targetCamera.orthographicSize = Mathf.Lerp(start, newTarget, t);
            timer += Time.deltaTime;
            yield return null;
        }

        targetCamera.orthographicSize = newTarget;
        sizeRoutine = null;
    }

    private void ApplyLightImmediate(float value)
    {
        if (playerLight != null)
        {
            playerLight.intensity = value;
        }
    }

    private void StartGlobalLightLerp(float targetIntensity)
    {
        if (globalLight == null) return;
        if (globalLightRoutine != null) StopCoroutine(globalLightRoutine);
        globalLightRoutine = StartCoroutine(LerpGlobalLightIntensity(targetIntensity));
    }

    private IEnumerator LerpGlobalLightIntensity(float targetIntensity)
    {
        float start = globalLight.intensity;
        float duration = Mathf.Max(0.05f, globalLightLerpDuration);
        float timer = 0f;

        while (timer < duration)
        {
            float t = Mathf.Clamp01(timer / duration);
            globalLight.intensity = Mathf.Lerp(start, targetIntensity, t);
            timer += Time.deltaTime;
            yield return null;
        }

        globalLight.intensity = targetIntensity;
        globalLightRoutine = null;
    }

    private void ApplyGlobalLightImmediate(float value)
    {
        if (globalLight != null)
        {
            globalLight.intensity = value;
        }
    }

    private void CacheLightRadius()
    {
        if (playerLight == null) return;
        baseOuterRadius = playerLight.pointLightOuterRadius;
        baseInnerRadius = playerLight.pointLightInnerRadius;
        if (baseOuterRadius > 0.001f)
        {
            innerOuterRatio = baseInnerRadius / baseOuterRadius;
        }
        else
        {
            innerOuterRatio = 0.5f;
        }
    }

    private void TryIncreaseLightRadius()
    {
        if (playerLight == null || killsPerLightRadiusStep <= 0 || lightOuterRadiusStep <= 0f) return;

        int desiredStep = killCount / killsPerLightRadiusStep;
        if (desiredStep <= lightRadiusStepCount) return;

        lightRadiusStepCount = desiredStep;
        float targetOuter = Mathf.Min(baseOuterRadius + lightRadiusStepCount * lightOuterRadiusStep, maxLightOuterRadius);
        float targetInner = targetOuter * innerOuterRatio;

        if (lightRadiusRoutine != null) StopCoroutine(lightRadiusRoutine);
        lightRadiusRoutine = StartCoroutine(LerpLightRadius(targetInner, targetOuter));
    }

    private IEnumerator LerpLightRadius(float targetInner, float targetOuter)
    {
        if (playerLight == null) yield break;

        float duration = Mathf.Max(0.05f, lightRadiusLerpDuration);
        float timer = 0f;

        float startInner = playerLight.pointLightInnerRadius;
        float startOuter = playerLight.pointLightOuterRadius;

        while (timer < duration)
        {
            float t = Mathf.Clamp01(timer / duration);
            playerLight.pointLightInnerRadius = Mathf.Lerp(startInner, targetInner, t);
            playerLight.pointLightOuterRadius = Mathf.Lerp(startOuter, targetOuter, t);
            timer += Time.deltaTime;
            yield return null;
        }

        playerLight.pointLightInnerRadius = targetInner;
        playerLight.pointLightOuterRadius = targetOuter;
        lightRadiusRoutine = null;
    }

    private IEnumerator SpawnWaveFromRight()
    {
        int spawned = 0;
        float interval = Mathf.Max(0.01f, spawnInterval);

        while (spawned < spawnCount)
        {
            foreach (Transform point in rightSpawnPoints)
            {
                if (point == null) continue;
                Instantiate(enemyPrefab, point.position, point.rotation);
                spawned++;
                if (spawned >= spawnCount) break;
            }
            yield return new WaitForSeconds(interval);
        }
    }
}
