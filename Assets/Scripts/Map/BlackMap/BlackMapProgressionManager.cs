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

    [Header("Pickup Extra Effects")]
    [SerializeField] private float cameraShakeDuration = 1f;
    [SerializeField] private float cameraShakeIntensity = 0.35f;
    [SerializeField] private float cameraShakeTailIntensity = 0.05f; // residual shake after duration
    [SerializeField] private bool cameraShakeSustain = true;        // keep shaking lightly after duration
    [SerializeField] private GameObject diagonalEnemyPrefab;
    [SerializeField] private Transform diagonalSpawnAnchor;
    [SerializeField] private int diagonalRows = 3;
    [SerializeField] private int diagonalCols = 4;
    [SerializeField] private float diagonalRowSpacing = 1.5f;
    [SerializeField] private float diagonalColSpacing = 1.5f;
    [SerializeField] private float diagonalAngleDeg = -30f; // formation tilt
    [SerializeField] private Vector2 diagonalMoveDir = new Vector2(-1f, -1f); // formation move direction
    [SerializeField] private float diagonalMoveSpeed = 3f;
    [SerializeField] private float diagonalLifeTime = 12f;
    [SerializeField] private float diagonalRowInterval = 0.15f;
    [SerializeField] private float diagonalContactDamage = 10f;
    [SerializeField] private Vector2 diagonalWobbleAmpRange = new Vector2(0.1f, 0.25f);
    [SerializeField] private Vector2 diagonalWobbleFreqRange = new Vector2(0.6f, 1.2f);

    [Header("Pickup Stage3 Horizontal Wave")]
    [SerializeField] private GameObject horizontalEnemyPrefab;
    [SerializeField] private Transform horizontalSpawnAnchor;
    [SerializeField] private int horizontalRows = 3;
    [SerializeField] private int horizontalCols = 6;
    [SerializeField] private float horizontalRowSpacing = 1.5f;
    [SerializeField] private float horizontalColSpacing = 1.5f;
    [SerializeField] private Vector2 horizontalMoveDir = new Vector2(0f, -1f);
    [SerializeField] private float horizontalMoveSpeed = 2.5f;
    [SerializeField] private float horizontalLifeTime = 12f;
    [SerializeField] private float horizontalRowInterval = 0.15f;
    [SerializeField] private float horizontalContactDamage = 10f;
    [SerializeField] private Vector2 horizontalWobbleAmpRange = new Vector2(0.1f, 0.25f);
    [SerializeField] private Vector2 horizontalWobbleFreqRange = new Vector2(0.6f, 1.2f);

    [Header("Stage1 Jitter Control")]
    [SerializeField] private MonoBehaviour stageOneJitter; // e.g., BlackMapStageOneJitter; kept disabled until pickup

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
        if (targetCamera != null && cameraShakeDuration > 0f && cameraShakeIntensity > 0f)
        {
            StartCoroutine(ShakeCamera());
        }

        if (diagonalSpawnAnchor != null)
        {
            StartCoroutine(SpawnDiagonalWave());
        }

        if (horizontalSpawnAnchor != null)
        {
            StartCoroutine(SpawnHorizontalWave());
        }

        if (stageOneJitter != null)
        {
            stageOneJitter.enabled = true;
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

    private IEnumerator ShakeCamera()
    {
        if (targetCamera == null) yield break;

        Transform camTransform = targetCamera.transform;
        Vector3 original = camTransform.localPosition;
        float timer = 0f;

        // phase 1: decay from main intensity to tail intensity
        while (timer < cameraShakeDuration)
        {
            float t = timer / Mathf.Max(0.0001f, cameraShakeDuration);
            float currentIntensity = Mathf.Lerp(cameraShakeIntensity, cameraShakeTailIntensity, t);
            Vector2 offset = Random.insideUnitCircle * currentIntensity;
            camTransform.localPosition = original + new Vector3(offset.x, offset.y, 0f);
            timer += Time.deltaTime;
            yield return null;
        }

        // phase 2: sustain low-intensity shake
        if (cameraShakeSustain && cameraShakeTailIntensity > 0f)
        {
            while (true)
            {
                Vector2 offset = Random.insideUnitCircle * cameraShakeTailIntensity;
                camTransform.localPosition = original + new Vector3(offset.x, offset.y, 0f);
                yield return null;
            }
        }
        else
        {
            camTransform.localPosition = original;
        }
    }

    private IEnumerator SpawnDiagonalWave()
    {
        GameObject prefabToUse = diagonalEnemyPrefab;
        if (prefabToUse == null || diagonalSpawnAnchor == null) yield break;

        int rows = Mathf.Max(1, diagonalRows);
        int cols = Mathf.Max(1, diagonalCols);
        float rowSpacing = Mathf.Max(0.1f, diagonalRowSpacing);
        float colSpacing = Mathf.Max(0.1f, diagonalColSpacing);
        float interval = Mathf.Max(0f, diagonalRowInterval);
        float angleRad = diagonalAngleDeg * Mathf.Deg2Rad;
        Vector2 right = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)); // local +X after tilt
        Vector2 down = new Vector2(-Mathf.Sin(angleRad), Mathf.Cos(angleRad)); // local -Y after tilt (perpendicular)
        Vector2 moveDir = diagonalMoveDir.sqrMagnitude > 0.0001f ? diagonalMoveDir.normalized : right.normalized;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector2 local = (-c * colSpacing) * right + (-r * rowSpacing) * down;
                Vector3 pos = diagonalSpawnAnchor.position + new Vector3(local.x, local.y, 0f);
                GameObject enemy = Instantiate(prefabToUse, pos, Quaternion.identity);

                // 锁定自带 AI，但保留碰撞伤害
                Enemy ai = enemy.GetComponent<Enemy>();
                if (ai != null) ai.enabled = false;

                Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                    rb.isKinematic = true;
                    rb.simulated = true;
                }

                BlackMapContactDamage dmg = enemy.GetComponent<BlackMapContactDamage>();
                if (dmg == null)
                {
                    dmg = enemy.AddComponent<BlackMapContactDamage>();
                }
                dmg.damage = diagonalContactDamage;

                BlackMapDiagonalMover mover = enemy.GetComponent<BlackMapDiagonalMover>();
                if (mover == null)
                {
                    mover = enemy.AddComponent<BlackMapDiagonalMover>();
                }
                float wobbleAmp = Random.Range(diagonalWobbleAmpRange.x, diagonalWobbleAmpRange.y);
                float wobbleFreq = Random.Range(diagonalWobbleFreqRange.x, diagonalWobbleFreqRange.y);
                Vector2 wobbleDir = new Vector2(-moveDir.y, moveDir.x); // perpendicular wobble
                float wobblePhase = Random.Range(0f, Mathf.PI * 2f);
                mover.Init(moveDir, diagonalMoveSpeed, diagonalLifeTime, wobbleAmp, wobbleFreq, wobbleDir, wobblePhase);
            }

            if (interval > 0f)
            {
                yield return new WaitForSeconds(interval);
            }
        }
    }

    private IEnumerator SpawnHorizontalWave()
    {
        GameObject prefabToUse = horizontalEnemyPrefab != null ? horizontalEnemyPrefab : diagonalEnemyPrefab;
        if (prefabToUse == null || horizontalSpawnAnchor == null) yield break;

        int rows = Mathf.Max(1, horizontalRows);
        int cols = Mathf.Max(1, horizontalCols);
        float rowSpacing = Mathf.Max(0.1f, horizontalRowSpacing);
        float colSpacing = Mathf.Max(0.1f, horizontalColSpacing);
        float interval = Mathf.Max(0f, horizontalRowInterval);
        Vector2 moveDir = horizontalMoveDir.sqrMagnitude > 0.0001f ? horizontalMoveDir.normalized : Vector2.down;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 offset = new Vector3(c * colSpacing, -r * rowSpacing, 0f);
                Vector3 pos = horizontalSpawnAnchor.position + offset;
                GameObject enemy = Instantiate(prefabToUse, pos, Quaternion.identity);

                Enemy ai = enemy.GetComponent<Enemy>();
                if (ai != null) ai.enabled = false;

                Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                    rb.isKinematic = true;
                    rb.simulated = true;
                }

                BlackMapContactDamage dmg = enemy.GetComponent<BlackMapContactDamage>();
                if (dmg == null)
                {
                    dmg = enemy.AddComponent<BlackMapContactDamage>();
                }
                dmg.damage = horizontalContactDamage;

                BlackMapDiagonalMover mover = enemy.GetComponent<BlackMapDiagonalMover>();
                if (mover == null)
                {
                    mover = enemy.AddComponent<BlackMapDiagonalMover>();
                }
                float wobbleAmp = Random.Range(horizontalWobbleAmpRange.x, horizontalWobbleAmpRange.y);
                float wobbleFreq = Random.Range(horizontalWobbleFreqRange.x, horizontalWobbleFreqRange.y);
                Vector2 wobbleDir = new Vector2(-moveDir.y, moveDir.x);
                float wobblePhase = Random.Range(0f, Mathf.PI * 2f);
                mover.Init(moveDir, horizontalMoveSpeed, horizontalLifeTime, wobbleAmp, wobbleFreq, wobbleDir, wobblePhase);
            }

            if (interval > 0f)
            {
                yield return new WaitForSeconds(interval);
            }
        }
    }
}
