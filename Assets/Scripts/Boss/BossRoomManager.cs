using System.Collections;
using UnityEngine;

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

    [Header("Final Phase Overlay")]
    public SpriteRenderer finalPhaseOverlay;
    public float finalPhaseOverlayFadeDuration = 2f;
    public float finalPhaseOverlayRotationSpeed = 60f;
    [Range(0f, 1f)] public float finalPhaseOverlayTargetAlpha = 0.5f;

    [Header("Roar Effect")]
    public GameObject roarEffectPrefab;
    public int roarWaves = 3;
    public float roarWaveDuration = 0.4f;
    public float roarWaveInterval = 0.15f;
    public Vector3 roarStartScale = new Vector3(0.2f, 0.2f, 1f);
    public Vector3 roarEndScale = new Vector3(2f, 2f, 1f);
    public float roarTiltAngle = 30f;
    public float roarScaleMultiplier = 1.1f;

    private GameObject mirrorInstance;
    private WhiteBoss activeWhite;
    private BlackBoss activeBlack;
    private bool fightRunning;
    private bool whitePhaseFinished;
    private bool blackPhaseFinished;

    private void Awake()
    {
        // 兜底：如果忘了在 Inspector 里拖 mainPlayer，就自动找场景里的 PlayerControl
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

        // 物理同步（横向速度）
        var phys = mirrorInstance.GetComponent<MirrorPhysicalController>();
        if (phys != null)
        {
            phys.mainPlayer = mainPlayer;
        }
        else
        {
            Debug.LogWarning("[BossRoomManager] mirrorPlayerPrefab 上没有 MirrorPhysicalController（如果你想要物理倒挂，这个脚本要挂上）");
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

        // 动画同步（黑白形态 + 行走）
        var mirrorAnimSync = mirrorInstance.GetComponent<MirrorAnimationSync>();
        if (mirrorAnimSync != null)
        {
            mirrorAnimSync.mainPlayer = mainPlayer;
        }
        else
        {
            Debug.LogWarning("[BossRoomManager] mirrorPlayerPrefab 上没有 MirrorAnimationSync");
        }

        // 攻击同步（如果你已经挂好 MirrorAttackController）
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
        // 2. 生成白 Boss 并等待阶段结束
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
            // 入场吼叫效果（与黑 Boss 相同的光波 + 仰头）
            StartCoroutine(PlayRoarWaves(activeWhite.transform));
        }
        else
        {
            Debug.LogWarning("[BossRoomManager] 白 Boss 预制体上没有 WhiteBoss 组件！");
            whitePhaseFinished = true; // 避免流程卡死
        }

        yield return new WaitUntil(() => whitePhaseFinished);

        // 3. 白 Boss 退场（无视 Collider）
        yield return RetreatWhiteBoss();

        // 4. 黑 Boss 从右上角入场并吼叫特效
        yield return SpawnBlackBossWithEntrance();

        // 5. 等待黑 Boss 阶段结束（死亡或时间结束），然后召唤双 Boss 同时吼叫
        yield return WaitForBlackPhaseEnd();
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
            : start + new Vector3(3f, 3f, 0f); // fallback: move to upper-right

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
        Vector3 entranceEnd = blackEntranceEnd != null ? blackEntranceEnd.position : (whiteSpawnPoint != null ? whiteSpawnPoint.position : transform.position);

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

        Coroutine overlay = null;
        if (finalPhaseOverlay != null)
        {
            overlay = StartCoroutine(PlayFinalOverlay());
        }
        else
        {
            Debug.LogWarning("[BossRoomManager] finalPhaseOverlay 未设置，跳过终幕贴图效果");
        }

        if (overlay != null) yield return overlay;

        // Start rotation coroutine separately so we don't wait for it
        if (finalPhaseOverlay != null)
        {
            StartCoroutine(RotateFinalOverlay());
        }

        if (activeBlack != null)
        {
            activeBlack.BeginFight();
        }

        // Final phase: survive the timer, then both bosses roar once and despawn (regardless of HP)
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

        if (roarEffectPrefab == null)
        {
            Debug.LogWarning("[BossRoomManager] roarEffectPrefab 未设置，跳过吼叫特效");
            yield break;
        }

        // 暂停白 Boss 的移动/攻击，让吼叫时保持静止
        var white = boss.GetComponent<WhiteBoss>();
        if (white != null)
        {
            white.SetRoarLock(true);
        }

        Quaternion originalRot = boss.rotation;
        Vector3 originalScale = boss.localScale;

        // 吼叫期间稍微上仰 + 放大
        boss.rotation = Quaternion.Euler(0f, 0f, roarTiltAngle);
        boss.localScale = originalScale * roarScaleMultiplier;

        for (int i = 0; i < roarWaves; i++)
        {
            if (boss == null) break;

            Vector3 spawnPos = boss.position;
            GameObject wave = Instantiate(roarEffectPrefab, spawnPos, Quaternion.identity);
            yield return ScaleOverTime(wave.transform, roarStartScale, roarEndScale, roarWaveDuration);
            Destroy(wave);
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
        // use camera to guess an off-screen top-right entrance point
        Camera cam = Camera.main;
        if (cam == null) return transform.position + new Vector3(5f, 5f, 0f);

        Vector3 topRight = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, -cam.transform.position.z));
        topRight.z = 0f;
        return topRight;
    }

    private IEnumerator PlayFinalOverlay()
    {
        if (finalPhaseOverlay == null) yield break;

        finalPhaseOverlay.gameObject.SetActive(true);
        Color c = finalPhaseOverlay.color;
        c.a = 0f;
        finalPhaseOverlay.color = c;

        float timer = 0f;
        while (timer < finalPhaseOverlayFadeDuration)
        {
            float t = Mathf.Clamp01(timer / Mathf.Max(finalPhaseOverlayFadeDuration, 0.01f));
            c.a = Mathf.Lerp(0f, finalPhaseOverlayTargetAlpha, t);
            finalPhaseOverlay.color = c;
            // Rotate while fading
            finalPhaseOverlay.transform.Rotate(Vector3.forward, finalPhaseOverlayRotationSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        c.a = finalPhaseOverlayTargetAlpha;
        finalPhaseOverlay.color = c;
    }

    private IEnumerator RotateFinalOverlay()
    {
        while (finalPhaseOverlay != null && finalPhaseOverlay.gameObject.activeInHierarchy)
        {
            finalPhaseOverlay.transform.Rotate(Vector3.forward, finalPhaseOverlayRotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

}
