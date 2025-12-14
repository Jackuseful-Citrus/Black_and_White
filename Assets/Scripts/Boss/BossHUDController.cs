using UnityEngine;
using UnityEngine.UI;

public class BossHUDController : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private GameObject phase12Root;
    [SerializeField] private GameObject finalRoot;

    [Header("Phase 1/2 UI")]
    [SerializeField] private GameObject whitePanel;
    [SerializeField] private GameObject blackPanel;
    [SerializeField] private Image whiteFill;
    [SerializeField] private Image blackFill;

    [Header("Final UI (countdown drains both)")]
    [SerializeField] private Image finalWhiteFill;
    [SerializeField] private Image finalBlackFill;

    private WhiteBoss whiteBoss;
    private BlackBoss blackBoss;

    private bool finalCountdownRunning;
    private float finalDuration;
    private float finalStartTime;

    private void Awake()
    {
        ShowPhase12();
        SetFillSafe(whiteFill, 0f);
        SetFillSafe(blackFill, 0f);
        SetFillSafe(finalWhiteFill, 1f);
        SetFillSafe(finalBlackFill, 1f);
    }

    private void Update()
    {
        if (finalCountdownRunning)
        {
            float t = Mathf.Clamp01((Time.time - finalStartTime) / Mathf.Max(finalDuration, 0.01f));
            float remain = 1f - t;

            // “两个条贴一起按倒计时衰减” → 这里让两条一起从 1 掉到 0
            SetFillSafe(finalWhiteFill, remain);
            SetFillSafe(finalBlackFill, remain);
            return;
        }

        // Phase 1/2：谁存在就更新谁
        if (whiteBoss != null)
        {
            float w = SafeRatio(whiteBoss.CurrentHealth, whiteBoss.MaxHealth);
            SetFillSafe(whiteFill, w);
        }
        if (blackBoss != null)
        {
            float b = SafeRatio(blackBoss.CurrentHealth, blackBoss.MaxHealth);
            SetFillSafe(blackFill, b);
        }

        UpdatePhase12Panels();
    }

    public void BindWhiteBoss(WhiteBoss boss)
    {
        whiteBoss = boss;
        ShowPhase12();
        if (whiteBoss != null)
        {
            SetFillSafe(whiteFill, 1f);
        }
        UpdatePhase12Panels();
    }

    public void BindBlackBoss(BlackBoss boss)
    {
        blackBoss = boss;
        ShowPhase12();
        if (blackBoss != null)
        {
            SetFillSafe(blackFill, 1f);
        }
        UpdatePhase12Panels();
    }

    public void ClearWhiteBoss()
    {
        whiteBoss = null;
        SetFillSafe(whiteFill, 0f);
        UpdatePhase12Panels();
    }
    public void ClearBlackBoss()
    {
        blackBoss = null;
        SetFillSafe(blackFill, 0f);
        UpdatePhase12Panels();
    }

    public void StartFinalCountdown(float durationSeconds)
    {
        finalCountdownRunning = true;
        finalDuration = durationSeconds;
        finalStartTime = Time.time;

        // 切到 Final UI
        if (phase12Root) phase12Root.SetActive(false);
        if (finalRoot) finalRoot.SetActive(true);

        SetFillSafe(finalWhiteFill, 1f);
        SetFillSafe(finalBlackFill, 1f);
    }

    private void ShowPhase12()
    {
        finalCountdownRunning = false;
        if (phase12Root) phase12Root.SetActive(true);
        if (finalRoot) finalRoot.SetActive(false);

        UpdatePhase12Panels();
    }

    private void UpdatePhase12Panels()
    {
        bool whiteAlive = whiteBoss != null && !whiteBoss.HasPhaseEnded && whiteBoss.gameObject.activeInHierarchy;
        bool blackAlive = blackBoss != null && !blackBoss.HasPhaseEnded && blackBoss.gameObject.activeInHierarchy;

        if (whitePanel != null) whitePanel.SetActive(whiteAlive);
        if (blackPanel != null) blackPanel.SetActive(blackAlive);
    }

    private static float SafeRatio(float cur, float max)
    {
        if (max <= 0.0001f) return 0f;
        return Mathf.Clamp01(cur / max);
    }

    private static void SetFillSafe(Image img, float v)
    {
        if (img != null) img.fillAmount = Mathf.Clamp01(v);
    }
}
