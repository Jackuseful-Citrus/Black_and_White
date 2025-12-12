using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossRoomManager : MonoBehaviour
{
    [Header("主角")]
    public PlayerControl mainPlayer;
    public BossEntrance bossEntrance;

    [Header("分身预制体")]
    public GameObject mirrorPlayerPrefab;
    public float mirrorLineY;

    [Header("White Boss")]
    public GameObject whiteBossPrefab;
    public Transform whiteSpawnPoint;
    public float whitePhaseDuration = 5f;
    public float whitePhaseMaxHealth = 10f;
    public Transform whiteRetreatTarget;
    public float whiteRetreatDuration = 1.2f;

    [Header("Black Boss")]
    public GameObject blackBossPrefab;
    public Transform blackEntranceStart;
    public Transform blackEntranceEnd;
    public float blackEntranceDuration = 1.2f;
    public float blackPhaseDuration = 8f;
    public float blackPhaseMaxHealth = 15f;

    [Header("Final Dual Phase")]
    public Transform finalWhiteSpawnPoint;
    public Transform finalBlackSpawnPoint;
    public float finalBlackPhaseDuration = 10f;
    public float finalBlackPhaseMaxHealth = 20f;

    [Header("Final Phase Overlay (碎片父物体)")]
    [SerializeField] private Transform finalOverlayRoot; // 三个碎片的父物体
    [SerializeField]
    private System.Collections.Generic.List<SpriteRenderer> finalOverlayPieces =
        new System.Collections.Generic.List<SpriteRenderer>(); // 碎片 sprite
    [SerializeField] private float finalOverlayDuration = 2.5f;      // 碎片旋转+放大+变白的总时间
    [SerializeField] private float finalOverlayRotationSpeed = 360f; // 每秒旋转角速度（度）
    [SerializeField] private float finalOverlayScaleMultiplier = 4f; // 最终放大倍数
    [SerializeField, Range(0f, 1f)] private float finalOverlayStartAlpha = 0f;
    [SerializeField, Range(0f, 1f)] private float finalOverlayEndAlpha = 1f;

    [Header("Ending")]
    [SerializeField] private string nextSceneName; // 结局之后跳转的场景（可选）

    [Header("Roar Effect")]
    public GameObject roarEffectPrefab;
    public int roarWaves = 3;
    public float roarWaveDuration = 0.4f;
    public float roarWaveInterval = 0.15f;
    public Vector3 roarStartScale = new Vector3(0.2f, 0.2f, 1f);
    public Vector3 roarEndScale = new Vector3(2f, 2f, 1f);
    public float roarTiltAngle = 30f;
    public float roarScaleMultiplier = 1.1f;
    [SerializeField] private AudioClip roarSfx;
    [SerializeField] private AudioSource sfxAudioSource;
    [Header("White Screen UI")]
    [SerializeField] private SpriteRenderer whiteFadeQuad; // 场景里的白色方块（SpriteRenderer）
    [SerializeField] private float whiteFadeDuration = 1.5f;


    private GameObject mirrorInstance;
    private WhiteBoss activeWhite;
    private BlackBoss activeBlack;
    private bool fightRunning;
    private bool whitePhaseFinished;
    private bool blackPhaseFinished;

    // 终幕 overlay 状态
    private Vector3 finalOverlayBaseScale;

    private void Awake()
    {
        // 开局隐藏碎片父物体
        if (finalOverlayRoot != null)
        {
            finalOverlayRoot.gameObject.SetActive(false);
        }

        // 自动找主角
        if (mainPlayer == null)
        {
            mainPlayer = FindObjectOfType<PlayerControl>();
            if (mainPlayer == null)
            {
                Debug.LogError("[BossRoomManager] 场景里找不到 PlayerControl，mainPlayer 为空！");
            }
        }
    }

    public void StartBossFight()
    {
        if (fightRunning) return;
        fightRunning = true;

        if (mainPlayer == null)
        {
            Debug.LogError("[BossRoomManager] StartBossFight 调用，但 mainPlayer 仍然是 null！");
            return;
        }

        if (mirrorPlayerPrefab == null)
        {
            Debug.LogError("[BossRoomManager] mirrorPlayerPrefab 未设置！");
            return;
        }

        // 1. 生成分身（严格对称：x 用主角的 x 坐标）
        Vector3 p = mainPlayer.transform.position;
        Vector3 spawnPos = new Vector3(p.x, 2f * mirrorLineY - p.y, p.z);

        mirrorInstance = Instantiate(mirrorPlayerPrefab, spawnPos, Quaternion.identity);
        mirrorInstance.tag = "PlayerMirror";

        // 物理同步
        var phys = mirrorInstance.GetComponent<MirrorPhysicalController>();
        if (phys != null)
        {
            phys.mainPlayer = mainPlayer;
        }
        else
        {
            Debug.LogWarning("[BossRoomManager] mirrorPlayerPrefab 上没有 MirrorPhysicalController");
        }

        // 视觉倒挂
        var visual = mirrorInstance.GetComponent<MirrorVisualController>();
        if (visual != null)
        {
            visual.mainPlayer = mainPlayer;
        }
        else
        {
            Debug.LogWarning("[BossRoomManager] mirrorPlayerPrefab 上没有 MirrorVisualController");
        }

        // 动画同步
        var mirrorAnimSync = mirrorInstance.GetComponent<MirrorAnimationSync>();
        if (mirrorAnimSync != null)
        {
            mirrorAnimSync.mainPlayer = mainPlayer;
        }
        else
        {
            Debug.LogWarning("[BossRoomManager] mirrorPlayerPrefab 上没有 MirrorAnimationSync");
        }

        // 攻击同步
        var mirrorAttack = mirrorInstance.GetComponent<MirrorAttackController>();
        if (mirrorAttack != null)
        {
            mirrorAttack.mainPlayer = mainPlayer;
        }

        StartCoroutine(BossSequence());
    }

    public void ResetEncounter()
    {
        StopAllCoroutines();
        fightRunning = false;
        whitePhaseFinished = false;
        blackPhaseFinished = false;

        if (activeWhite != null)
        {
            Destroy(activeWhite.gameObject);
            activeWhite = null;
        }

        if (activeBlack != null)
        {
            Destroy(activeBlack.gameObject);
            activeBlack = null;
        }

        if (mirrorInstance != null)
        {
            Destroy(mirrorInstance);
            mirrorInstance = null;
        }

        if (bossEntrance == null)
        {
            bossEntrance = FindObjectOfType<BossEntrance>();
        }

        if (bossEntrance != null)
        {
            bossEntrance.gameObject.SetActive(true);
        }
    }

    private IEnumerator BossSequence()
    {
        // 白 Boss 阶段
        if (whiteBossPrefab == null || whiteSpawnPoint == null)
        {
            Debug.LogError("[BossRoomManager] 白 Boss 预制体或生成点未设置，流程中断！");
            yield break;
        }

        whitePhaseFinished = false;
        GameObject whiteObj = Instantiate(whiteBossPrefab, whiteSpawnPoint.position, Quaternion.identity);
        activeWhite = whiteObj.GetComponent<WhiteBoss>();
        if (activeWhite != null)
        {
            activeWhite.ConfigurePhase(whitePhaseDuration, whitePhaseMaxHealth);
            activeWhite.onPhaseEnded += OnWhitePhaseEnded;
            StartCoroutine(PlayRoarWaves(activeWhite.transform));
        }
        else
        {
            Debug.LogWarning("[BossRoomManager] 白 Boss 预制体上没有 WhiteBoss 组件！");
            whitePhaseFinished = true;
        }

        yield return new WaitUntil(() => whitePhaseFinished);

        // 白 Boss 退场
        yield return RetreatWhiteBoss();

        // 黑 Boss 入场
        yield return SpawnBlackBossWithEntrance();

        // 黑 Boss 阶段结束
        yield return WaitForBlackPhaseEnd();

        // 双 Boss 吼叫 + 最终阶段
        yield return SpawnDualBossRoarPhase();
    }

    private void OnWhitePhaseEnded()
    {
        whitePhaseFinished = true;
    }

    private void OnBlackPhaseEnded()
    {
        blackPhaseFinished = true;
    }

    private IEnumerator RetreatWhiteBoss()
    {
        if (activeWhite == null) yield break;

        activeWhite.EnterRetreatMode();

        Transform t = activeWhite.transform;
        Vector3 start = t.position;
        Vector3 end = whiteRetreatTarget != null
            ? whiteRetreatTarget.position
            : start + new Vector3(3f, 3f, 0f);

        float timer = 0f;
        while (timer < whiteRetreatDuration)
        {
            float normalized = Mathf.Clamp01(timer / Mathf.Max(whiteRetreatDuration, 0.01f));
            t.position = Vector3.Lerp(start, end, normalized);
            timer += Time.deltaTime;
            yield return null;
        }

        t.position = end;
        Destroy(activeWhite.gameObject);
        activeWhite = null;
    }

    private IEnumerator SpawnBlackBossWithEntrance()
    {
        if (blackBossPrefab == null)
        {
            Debug.LogError("[BossRoomManager] 黑 Boss 预制体未设置！");
            yield break;
        }

        Vector3 entranceStart = blackEntranceStart != null ? blackEntranceStart.position : GetFallbackTopRight();
        Vector3 entranceEnd = blackEntranceEnd != null ? blackEntranceEnd.position :
            (whiteSpawnPoint != null ? whiteSpawnPoint.position : transform.position);

        GameObject blackObj = Instantiate(blackBossPrefab, entranceStart, Quaternion.identity);
        activeBlack = blackObj.GetComponent<BlackBoss>();
        if (activeBlack != null)
        {
            blackPhaseFinished = false;
            activeBlack.ConfigurePhase(blackPhaseDuration, blackPhaseMaxHealth);
            activeBlack.onPhaseEnded += OnBlackPhaseEnded;
            activeBlack.PauseFight();
        }
        else
        {
            Debug.LogWarning("[BossRoomManager] 黑 Boss 预制体上没有 BlackBoss 组件！");
        }

        yield return MoveTransform(blackObj.transform, entranceEnd, blackEntranceDuration);
        yield return PlayRoarWaves(blackObj.transform);

        if (activeBlack != null)
        {
            activeBlack.BeginFight();
        }
    }

    private IEnumerator WaitForBlackPhaseEnd()
    {
        yield return new WaitUntil(() => blackPhaseFinished || activeBlack == null);

        if (activeBlack != null)
        {
            activeBlack.onPhaseEnded -= OnBlackPhaseEnded;
            Destroy(activeBlack.gameObject);
            activeBlack = null;
        }
    }

    private IEnumerator SpawnDualBossRoarPhase()
    {
        if (whiteBossPrefab == null || blackBossPrefab == null)
        {
            Debug.LogError("[BossRoomManager] 需要白/黑 Boss 预制体来启动第三阶段！");
            yield break;
        }

        Vector3 whitePos = finalWhiteSpawnPoint != null ? finalWhiteSpawnPoint.position :
            (whiteSpawnPoint != null ? whiteSpawnPoint.position : transform.position);
        Vector3 blackPos = finalBlackSpawnPoint != null ? finalBlackSpawnPoint.position :
            (blackEntranceEnd != null ? blackEntranceEnd.position : transform.position);

        GameObject whiteObj = Instantiate(whiteBossPrefab, whitePos, Quaternion.identity);
        activeWhite = whiteObj.GetComponent<WhiteBoss>();
        if (activeWhite != null)
        {
            activeWhite.ConfigurePhase(whitePhaseDuration, whitePhaseMaxHealth);
        }

        GameObject blackObj = Instantiate(blackBossPrefab, blackPos, Quaternion.identity);
        activeBlack = blackObj.GetComponent<BlackBoss>();
        if (activeBlack != null)
        {
            activeBlack.PauseFight();
            activeBlack.ConfigurePhase(finalBlackPhaseDuration, finalBlackPhaseMaxHealth);
        }

        Coroutine whiteRoar = null;
        if (activeWhite != null)
        {
            whiteRoar = StartCoroutine(PlayRoarWaves(activeWhite.transform));
        }

        Coroutine blackRoar = null;
        if (activeBlack != null)
        {
            blackRoar = StartCoroutine(PlayRoarWaves(activeBlack.transform));
        }

        if (whiteRoar != null) yield return whiteRoar;
        if (blackRoar != null) yield return blackRoar;

        if (activeBlack != null)
        {
            activeBlack.BeginFight();
        }

        // 最终黑阶段：计时结束即可
        yield return new WaitForSeconds(finalBlackPhaseDuration);

        if (activeWhite != null)
        {
            yield return PlayRoarWaves(activeWhite.transform);
            Destroy(activeWhite.gameObject);
            activeWhite = null;
        }

        if (activeBlack != null)
        {
            activeBlack.PauseFight();
            yield return PlayRoarWaves(activeBlack.transform);
            Destroy(activeBlack.gameObject);
            activeBlack = null;
        }

        // 终幕：碎片旋转放大 + 渐白
        if (finalOverlayRoot != null)
        {
            yield return PlayFinalOverlayAndWhite();
        }
        else
        {
            Debug.LogWarning("[BossRoomManager] finalOverlayRoot 未设置，跳过终幕碎片效果");
        }

        // 如果配置了结局场景，就跳转
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private IEnumerator MoveTransform(Transform target, Vector3 destination, float duration)
    {
        Vector3 start = target.position;
        float timer = 0f;
        while (timer < duration)
        {
            float normalized = Mathf.Clamp01(timer / Mathf.Max(duration, 0.01f));
            target.position = Vector3.Lerp(start, destination, normalized);
            timer += Time.deltaTime;
            yield return null;
        }

        target.position = destination;
    }

    private IEnumerator PlayRoarWaves(Transform boss)
    {
        if (boss == null) yield break;

        var white = boss.GetComponent<WhiteBoss>();
        if (white != null)
        {
            white.SetRoarLock(true);
        }

        Quaternion originalRot = boss.rotation;
        Vector3 originalScale = boss.localScale;

        if (roarEffectPrefab == null)
        {
            Debug.LogWarning("[BossRoomManager] roarEffectPrefab 未设置，跳过吼叫特效");
            if (white != null) white.SetRoarLock(false);
            // 仍然可以播放音效
        }

        // 吼叫姿态
        boss.rotation = Quaternion.Euler(0f, 0f, roarTiltAngle);
        boss.localScale = originalScale * roarScaleMultiplier;

        // 播放一次吼叫音效
        if (roarSfx != null)
        {
            if (sfxAudioSource != null)
            {
                sfxAudioSource.PlayOneShot(roarSfx);
            }
            else
            {
                AudioSource.PlayClipAtPoint(roarSfx, boss.position);
            }
        }

        for (int i = 0; i < roarWaves; i++)
        {
            if (boss == null) break;

            if (roarEffectPrefab != null)
            {
                Vector3 spawnPos = boss.position;
                GameObject wave = Instantiate(roarEffectPrefab, spawnPos, Quaternion.identity);
                yield return ScaleOverTime(wave.transform, roarStartScale, roarEndScale, roarWaveDuration);
                Destroy(wave);
            }
            else
            {
                yield return new WaitForSeconds(roarWaveDuration);
            }
            yield return new WaitForSeconds(roarWaveInterval);
        }

        if (boss != null)
        {
            boss.rotation = originalRot;
            boss.localScale = originalScale;

            if (white != null)
            {
                white.SetRoarLock(false);
            }
        }
    }

    private IEnumerator ScaleOverTime(Transform target, Vector3 from, Vector3 to, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            float normalized = Mathf.Clamp01(timer / Mathf.Max(duration, 0.01f));
            target.localScale = Vector3.Lerp(from, to, normalized);
            timer += Time.deltaTime;
            yield return null;
        }

        target.localScale = to;
    }

    private Vector3 GetFallbackTopRight()
    {
        Camera cam = Camera.main;
        if (cam == null) return transform.position + new Vector3(5f, 5f, 0f);

        Vector3 topRight = cam.ScreenToWorldPoint(
            new Vector3(cam.pixelWidth, cam.pixelHeight, -cam.transform.position.z));
        topRight.z = 0f;
        return topRight;
    }

    // ------------ 终幕碎片旋转 + 放大 + “白屏” ------------
    private IEnumerator PlayFinalOverlayAndWhite()
    {
        if (finalOverlayRoot == null) yield break;

        finalOverlayRoot.gameObject.SetActive(true);

        // 父物体 Z 锁 0
        Vector3 rootPos = finalOverlayRoot.position;
        rootPos.z = 0f;
        finalOverlayRoot.position = rootPos;

        // 如果没填 list，就自动从子物体抓 sprite
        if (finalOverlayPieces == null || finalOverlayPieces.Count == 0)
        {
            finalOverlayPieces = new System.Collections.Generic.List<SpriteRenderer>(
                finalOverlayRoot.GetComponentsInChildren<SpriteRenderer>());
        }

        finalOverlayBaseScale = finalOverlayRoot.localScale;

        // 初始化：子物体激活、Z=0，alpha 设为起始值
        foreach (var sr in finalOverlayPieces)
        {
            if (sr == null) continue;

            sr.gameObject.SetActive(true);

            var p = sr.transform.position;
            p.z = 0f;
            sr.transform.position = p;

            var c = sr.color;
            c.a = finalOverlayStartAlpha;
            sr.color = c;
        }

        float timer = 0f;
        while (timer < finalOverlayDuration)
        {
            float t = Mathf.Clamp01(timer / Mathf.Max(finalOverlayDuration, 0.01f));

            // 1. 像陀螺一样自转
            float angle = finalOverlayRotationSpeed * timer;
            finalOverlayRoot.rotation = Quaternion.Euler(0f, 0f, angle);

            // 2. 整体放大
            float scale = Mathf.Lerp(1f, finalOverlayScaleMultiplier, t);
            finalOverlayRoot.localScale = finalOverlayBaseScale * scale;

            // 3. 碎片渐白（多重叠加 ≈ 白屏）
            float alpha = Mathf.Lerp(finalOverlayStartAlpha, finalOverlayEndAlpha, t);
            ApplyOverlayAlpha(alpha);

            timer += Time.deltaTime;
            yield return null;
        }

        // 收尾：完全白、完全放大
        finalOverlayRoot.rotation = Quaternion.Euler(0f, 0f, finalOverlayRotationSpeed * finalOverlayDuration);
        finalOverlayRoot.localScale = finalOverlayBaseScale * finalOverlayScaleMultiplier;
        ApplyOverlayAlpha(finalOverlayEndAlpha);

        // 同步让白色方块淡入
        if (whiteFadeQuad != null)
        {
            yield return FadeWhiteQuad();
        }
        else
        {
            // 兜底：稍微停一小会儿再切场景
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void ApplyOverlayAlpha(float alpha)
    {
        if (finalOverlayPieces == null) return;

        foreach (var sr in finalOverlayPieces)
        {
            if (sr == null) continue;
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    private IEnumerator FadeWhiteQuad()
    {
        if (whiteFadeQuad == null) yield break;

        Color c = whiteFadeQuad.color;
        c.a = 0f;
        whiteFadeQuad.color = c;
        whiteFadeQuad.gameObject.SetActive(true);

        float timer = 0f;
        while (timer < whiteFadeDuration)
        {
            float t = Mathf.Clamp01(timer / Mathf.Max(whiteFadeDuration, 0.01f));
            c.a = t;
            whiteFadeQuad.color = c;
            timer += Time.deltaTime;
            yield return null;
        }

        c.a = 1f;
        whiteFadeQuad.color = c;
    }
}
