using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FragmentCollectionManager : MonoBehaviour
{
    public static FragmentCollectionManager Instance { get; private set; }

    [System.Serializable]
    public class FragmentSlot
    {
        public FragmentId id;      // 对应哪块碎片
        public Image icon;         // Slot 上的 Image（已经放好暗淡版本的碎片）
    }

    [Header("三个碎片 UI 槽位")]
    public List<FragmentSlot> slots = new List<FragmentSlot>();

    [Header("透明度设置")]
    [Range(0f, 1f)] public float dimAlpha  = 0.25f;   // 未收集用的暗淡透明度
    [Range(0f, 1f)] public float litAlpha  = 1.00f;   // 已收集用的高亮透明度

    // 跨场景的真实收集状态
    private HashSet<FragmentId> collected = new HashSet<FragmentId>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        RefreshAllIcons();
    }

    private void OnEnable()
    {
        // 场景切换回来时，重新套一次透明度
        RefreshAllIcons();
    }

    /// <summary> 是否已经收集了某个碎片（跨场景）。 </summary>
    public bool IsCollected(FragmentId id)
    {
        return collected.Contains(id);
    }

    /// <summary> 标记某个碎片已经被收集，并立即点亮对应 UI。 </summary>
    public void MarkCollected(FragmentId id)
    {
        // 如果之前收集过，就啥也不干
        if (!collected.Add(id))
            return;

        RefreshIcon(id);
    }

    /// <summary> 刷新所有 Slot 的透明度（根据当前 collected 集合）。 </summary>
    public void RefreshAllIcons()
    {
        foreach (var slot in slots)
        {
            if (slot.icon == null) continue;

            bool has = collected.Contains(slot.id);
            ApplyAlpha(slot.icon, has ? litAlpha : dimAlpha);
        }
    }

    /// <summary> 只刷新某一个碎片的图标。 </summary>
    private void RefreshIcon(FragmentId id)
    {
        foreach (var slot in slots)
        {
            if (slot.id != id || slot.icon == null) continue;

            bool has = collected.Contains(id);
            ApplyAlpha(slot.icon, has ? litAlpha : dimAlpha);
            break;
        }
    }

    private void ApplyAlpha(Image img, float alpha)
    {
        if (img == null) return;
        var c = img.color;
        c.a = alpha;
        img.color = c;
    }
}
