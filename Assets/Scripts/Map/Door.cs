using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    [Header("传送设置（根据黑/白条差值选择场景）")]
    [SerializeField] private string blackWinsScene = "BlackMap";
    [SerializeField] private string whiteWinsScene = "WhiteMap";
    [SerializeField] private string balancedScene = "GreyMap";

    [Tooltip("传送点相对于门的位置偏移")]
    [SerializeField] private Vector3 teleportOffset = Vector3.zero;

    [Header("交互设置")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool showDebugLog = true;

    [Header("依赖的可互动提示脚本")]
    [SerializeField] private InteractablePrompt interactable;

    private bool wasInRange = false;

    private void Reset()
    {
        if (interactable == null)
            interactable = GetComponent<InteractablePrompt>();
    }

    private void Start()
    {
        if (interactable == null)
            interactable = GetComponent<InteractablePrompt>();
    }

    private void Update()
    {
        if (interactable == null) return;

        // 检测玩家进入/离开范围，自动显示/隐藏 E 提示
        if (interactable.PlayerInRange && !wasInRange)
        {
            interactable.SetHintVisible(true);
            if (showDebugLog)
                Debug.Log($"[Door] 玩家进入门范围，按 {interactKey} 传送（会根据黑白条决定场景）。");
        }
        else if (!interactable.PlayerInRange && wasInRange)
        {
            interactable.SetHintVisible(false);
            if (showDebugLog)
                Debug.Log("[Door] 玩家离开门范围。");
        }
        wasInRange = interactable.PlayerInRange;

        // 在范围内按 E 触发传送
        if (interactable.PlayerInRange && Input.GetKeyDown(interactKey))
        {
            TryTeleportByBars();
        }
    }

    /// <summary>
    /// 按 E 后，根据黑/白条的差值决定要去哪个场景
    /// </summary>
    private void TryTeleportByBars()
    {
        if (LogicScript.Instance == null)
        {
            Debug.LogError("[Door] LogicScript.Instance 为 null，确认场景里有挂 LogicScript 的物体。");
            return;
        }

        // 读取当前黑白条数值
        int blackBar = LogicScript.Instance.GetBlackBar();
        int whiteBar = LogicScript.Instance.GetWhiteBar();
        int difference = blackBar - whiteBar;

        string targetScene;

        if (difference > 10)
        {
            targetScene = blackWinsScene;
            if (showDebugLog)
                Debug.Log($"[Door] 黑条领先 {difference}，传送到：{blackWinsScene}");
        }
        else if (difference < -10)
        {
            targetScene = whiteWinsScene;
            if (showDebugLog)
                Debug.Log($"[Door] 白条领先 {-difference}，传送到：{whiteWinsScene}");
        }
        else
        {
            targetScene = balancedScene;
            if (showDebugLog)
                Debug.Log($"[Door] 黑白条平衡（差值：{difference}），传送到：{balancedScene}");
        }

        // 传给 LogicScript 的还是"门的位置 + 偏移"
        Vector3 teleportPoint = GetSpawnPoint();
        LogicScript.Instance.TeleportViaDoor(targetScene);
    }

    public Vector3 GetSpawnPoint()
    {
        return transform.position + teleportOffset;
    }
}
