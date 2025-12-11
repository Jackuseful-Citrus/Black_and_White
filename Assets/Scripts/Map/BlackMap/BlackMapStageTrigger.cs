using UnityEngine;

/// <summary>
/// 进入开始触发器后开启生成，进入结束触发器后停止生成。
/// 可分别挂在 StartTrigger / EndTrigger 上，选择对应模式。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BlackMapStageTrigger : MonoBehaviour
{
    public enum TriggerMode { Start, End }

    [SerializeField] private TriggerMode mode = TriggerMode.Start;
    [SerializeField] private BlackMapStageOne stageOne;
    [SerializeField] private bool singleUse = true; // 触发一次后即失效

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

        if (stageOne == null) return;

        if (mode == TriggerMode.Start)
        {
            stageOne.StartStage();
        }
        else
        {
            stageOne.StopStage();
        }
    }
}
