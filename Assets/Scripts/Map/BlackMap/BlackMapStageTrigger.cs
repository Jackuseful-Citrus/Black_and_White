using UnityEngine;

/// <summary>
/// Start/End trigger to control stage 1 and stage 2.
/// - Stage1: BlackMapStageOne (start/stop)
/// - Stage2: BlackMapEnemyRing (spawn/clear)
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BlackMapStageTrigger : MonoBehaviour
{
    public enum TriggerMode { Start, End }

    [SerializeField] private TriggerMode mode = TriggerMode.Start;
    [SerializeField] private BlackMapStageOne stageOne;
    [SerializeField] private BlackMapEnemyRing stageTwo;
    [SerializeField] private bool singleUse = true; // trigger once then disable
    [SerializeField] private float stageStartDelay = 0.05f; // avoid starting coroutines while inactive
    [SerializeField] private GameObject[] enableOnStart;
    [SerializeField] private GameObject[] disableOnStart;
    [SerializeField] private GameObject[] enableOnEnd;
    [SerializeField] private GameObject[] disableOnEnd;

    private bool triggered;
    private Coroutine triggerRoutine;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (singleUse && triggered) return;
        triggered = true;

        if (triggerRoutine != null) StopCoroutine(triggerRoutine);
        if (stageStartDelay > 0f)
        {
            triggerRoutine = StartCoroutine(TriggerWithDelay(stageStartDelay));
        }
        else
        {
            TriggerStages();
        }
    }

    private System.Collections.IEnumerator TriggerWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        TriggerStages();
        triggerRoutine = null;
    }

    private void TriggerStages()
    {
        if (mode == TriggerMode.Start)
        {
            if (stageOne != null) stageOne.StartStage();
            if (stageTwo != null) stageTwo.SpawnRing();
            SetActiveBatch(enableOnStart, true);
            SetActiveBatch(disableOnStart, false);

            // BGM 切换：Stage1 -> Stage2
            if (BlackMapAudioManager.Instance != null)
            {
                if (stageTwo != null)
                    BlackMapAudioManager.Instance.PlayStage2();
                else
                    BlackMapAudioManager.Instance.PlayStage1();
            }
        }
        else
        {
            if (stageOne != null) stageOne.StopStage();
            if (stageTwo != null) stageTwo.ClearRing();
            SetActiveBatch(enableOnEnd, true);
            SetActiveBatch(disableOnEnd, false);
        }
    }

    private void SetActiveBatch(GameObject[] targets, bool active)
    {
        if (targets == null) return;
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
            {
                targets[i].SetActive(active);
            }
        }
    }

    public void ResetTriggerState()
    {
        triggered = false;
    }
}
