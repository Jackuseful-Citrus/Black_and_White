using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 将玩家传送到 BossTest 场景指定的出生点。
/// 挂在带触发器的 BoxCollider2D 物体上，可选择自动传送或按键交互。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BossTestPortal : MonoBehaviour
{
    [Header("Teleport")]
    [SerializeField] private string targetScene = "BossTest";
    [SerializeField] private Vector3 targetSpawnPoint = Vector3.zero; // BossTest 场景中的出生点坐标

    [Header("Interaction")]
    [SerializeField] private bool requireKeyPress = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject promptUI; // 可选：提示“按 E”一类的 UI

    private bool playerInside;
    private LogicScript logic;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Awake()
    {
        logic = LogicScript.Instance;
        if (logic == null)
        {
            logic = FindObjectOfType<LogicScript>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        SetPrompt(true);

        if (!requireKeyPress)
        {
            Teleport();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        SetPrompt(false);
    }

    private void Update()
    {
        if (!requireKeyPress || !playerInside) return;
        if (Input.GetKeyDown(interactKey))
        {
            Teleport();
        }
    }

    private void Teleport()
    {
        if (logic == null)
        {
            Debug.LogError("[BossTestPortal] 未找到 LogicScript，无法传送。");
            return;
        }

        logic.TeleportPlayerToScene(targetSpawnPoint, targetScene);
        SetPrompt(false);
    }

    private void SetPrompt(bool show)
    {
        if (promptUI != null)
        {
            promptUI.SetActive(show && requireKeyPress);
        }
    }
}
