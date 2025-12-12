using UnityEngine;

public enum FragmentId
{
    Black = 0,  // 黑图碎片
    White = 1,  // 白图碎片
    Gray  = 2   // 灰图碎片
}

/// <summary>
/// 场景终点碎片：
/// - 玩家走进来后按键 / 自动收集
/// - 通知 FragmentCollectionManager：对应碎片已收集，点亮 UI
/// - 黑图可以额外通知 BlackMapProgressionManager 做光照/刷怪
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FragmentEndPoint : MonoBehaviour
{
    [Header("碎片标识（3 张图各用一个）")]
    [SerializeField] private FragmentId fragmentId = FragmentId.Black;

    [Header("黑图流程管理（只有黑图需要，其它图留空）")]
    [SerializeField] private BlackMapProgressionManager progression;

    [Header("拾取方式")]
    [SerializeField] private bool autoCollectOnReach = false;  // true = 一碰就收
    [SerializeField] private KeyCode collectKey = KeyCode.E;   // 需要按键时的按键
    [SerializeField] private GameObject pickupVfx;
    [SerializeField] private bool disableInsteadOfDestroy = true;

    private bool collected;
    private bool playerInside;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Start()
    {
        // 1. 如果这个碎片已经在别的场景收集过，直接隐藏自己
        var fragMgr = FragmentCollectionManager.Instance;
        if (fragMgr != null && fragMgr.IsCollected(fragmentId))
        {
            collected = true;
            if (disableInsteadOfDestroy) gameObject.SetActive(false);
            else Destroy(gameObject);
            return;
        }

        // 保底：确保是 trigger
        var c2d = GetComponent<Collider2D>();
        if (c2d != null) c2d.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;

        // 黑图的话，第一次走进终点可以通知拉视野之类
        if (progression != null)
        {
            progression.NotifyReachedEnd();
        }

        if (!collected && autoCollectOnReach)
        {
            Collect();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
    }

    private void Update()
    {
        if (collected) return;

        // 手动按键收集
        if (!autoCollectOnReach && playerInside && Input.GetKeyDown(collectKey))
        {
            Collect();
        }
    }

    private void Collect()
    {
        if (collected) return;
        collected = true;

        // 1. 通知全局收集系统，点亮 UI
        var fragMgr = FragmentCollectionManager.Instance;
        if (fragMgr != null)
        {
            fragMgr.MarkCollected(fragmentId);
        }

        // 2. 特效
        if (pickupVfx != null)
        {
            Instantiate(pickupVfx, transform.position, Quaternion.identity);
        }

        // 3. 黑图额外逻辑（光照、刷怪、平衡条之类）
        if (progression != null)
        {
            progression.NotifyPickupCollected();
        }

        // 4. 隐藏 / 销毁这个场景碎片
        if (disableInsteadOfDestroy) gameObject.SetActive(false);
        else Destroy(gameObject);
    }

    // 如果以后你需要在“重开关卡”时重置这个碎片，可以调用这个
    public void ResetState()
    {
        collected = false;
        playerInside = false;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }
}
