using UnityEngine;

/// <summary>
/// 终点触发 + 拾取整合：
/// - 首次进入：通知管理器拉满视野、弱光。
/// - 按键拾取或自动拾取：大幅增光并刷怪。
/// - 如未拾取且需要交互，可锁定玩家移动。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BlackMapEndPoint : MonoBehaviour
{
    [Header("管理器")]
    [SerializeField] private BlackMapProgressionManager progression;

    [Header("拾取设置")]
    [SerializeField] private bool autoCollectOnReach = false; // 进入时是否自动拾取
    [SerializeField] private bool lockMovementBeforePickup = true; // 未拾取时是否锁定玩家
    [SerializeField] private KeyCode collectKey = KeyCode.E;  // 需要交互时按的键
    [SerializeField] private GameObject pickupVfx;
    [SerializeField] private bool disableInsteadOfDestroy = true;

    private bool reachedEnd;
    private bool collected;
    private PlayerControl lockedPlayer;
    private Rigidbody2D lockedRb;
    private bool movementLocked;
    private RigidbodyConstraints2D originalConstraints;
    private bool playerInside;
    private bool stayLogged;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;

        BlackMapProgressionManager mgr = progression != null ? progression : BlackMapProgressionManager.Instance;
        if (mgr == null) return;

        if (!reachedEnd)
        {
            reachedEnd = true;
            mgr.NotifyReachedEnd();
            if (!autoCollectOnReach && lockMovementBeforePickup && !collected)
            {
                LockPlayer(other);
            }

            if (autoCollectOnReach && !collected)
            {
                Collect(mgr);
            }
        }
        else
        {
            if (!collected && autoCollectOnReach)
            {
                Collect(mgr);
            }
            else if (!collected && lockMovementBeforePickup)
            {
                LockPlayer(other);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (collected || !reachedEnd) return;
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        if (autoCollectOnReach) return;

        if (lockMovementBeforePickup && !movementLocked)
        {
            LockPlayer(other);
        }

        if (Input.GetKeyDown(collectKey))
        {
            BlackMapProgressionManager mgr = progression != null ? progression : BlackMapProgressionManager.Instance;
            if (mgr != null) Collect(mgr);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        if (movementLocked)
        {
            UnlockPlayer();
        }
    }

    private void Update()
    {
        if (movementLocked && lockedRb != null)
        {
            lockedRb.velocity = Vector2.zero;
        }

        // 兜底：即使 OnTriggerStay 未触发，也允许在 Update 里按键收集
        if (playerInside && reachedEnd && !collected && !autoCollectOnReach && Input.GetKeyDown(collectKey))
        {
            BlackMapProgressionManager mgr = progression != null ? progression : BlackMapProgressionManager.Instance;
            if (mgr != null)
        {
            Collect(mgr);
        }
    }
    }

    private void Collect(BlackMapProgressionManager mgr)
    {
        if (collected) return;
        collected = true;
        if (pickupVfx != null)
        {
            Instantiate(pickupVfx, transform.position, Quaternion.identity);
        }

        mgr.NotifyPickupCollected();
        UnlockPlayer();

        if (disableInsteadOfDestroy)
        {
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LockPlayer(Collider2D other)
    {
        if (!lockMovementBeforePickup || movementLocked) return;
        lockedPlayer = other.GetComponent<PlayerControl>();
        lockedRb = other.attachedRigidbody;

        if (lockedRb != null)
        {
            lockedRb.velocity = Vector2.zero;
            originalConstraints = lockedRb.constraints;
            lockedRb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        if (lockedPlayer != null)
        {
            lockedPlayer.enabled = false;
        }
        movementLocked = true;
    }

    private void UnlockPlayer()
    {
        if (!movementLocked) return;
        if (lockedPlayer != null)
        {
            lockedPlayer.enabled = true;
        }
        if (lockedRb != null)
        {
            lockedRb.constraints = originalConstraints;
        }
        lockedPlayer = null;
        lockedRb = null;
        movementLocked = false;
    }
}
