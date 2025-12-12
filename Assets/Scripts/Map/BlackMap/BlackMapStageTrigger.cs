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
    [SerializeField] private GameObject[] enableOnStart;
    [SerializeField] private GameObject[] disableOnStart;
    [SerializeField] private GameObject[] enableOnEnd;
    [SerializeField] private GameObject[] disableOnEnd;

    private bool triggered;

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

        TriggerStages();
    }

    private void TriggerStages()
    {
        if (mode == TriggerMode.Start)
        {
            if (stageOne != null) stageOne.StartStage();
            if (stageTwo != null) stageTwo.SpawnRing();
            SetActiveBatch(enableOnStart, true);
            SetActiveBatch(disableOnStart, false);
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
